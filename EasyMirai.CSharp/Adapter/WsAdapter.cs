using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasyMirai.CSharp.Api;
using EasyMirai.CSharp.Util;
using System.Net.WebSockets;
using System.Buffers;
using System.Collections.Concurrent;

namespace EasyMirai.CSharp.Adapter
{
    /// <summary>
    /// Websocket adapter
    /// </summary>
    public partial class WsAdapter : IDisposable
    {
        private MiraiConfig _config;

        public MiraiJsonSerializers.EventSerializeHookTable EventHookTable = new();

        /// <summary>
        /// Ws 响应包
        /// </summary>
        internal struct ResponsePackage<T>
            where T : MiraiJsonSerializers.ISerializable<T>, new()
        {
            public int SyncId { get; }
            public T Data { get; }

            public ResponsePackage(int syncId, T data)
            {
                SyncId = syncId;
                Data = data;
            }

        }

        /// <summary>
        /// Ws 请求包
        /// </summary>
        internal struct RequestPackage<T>
            where T : MiraiJsonSerializers.ISerializable<T>, new()
        {
            public int SyncId { get; }
            public string Command { get; }
            public string? SubCommand { get; }
            public T Content { get; }

            public RequestPackage(int syncId, string command, string? subCommand, T content)
            {
                SyncId = syncId;
                Command = command;
                SubCommand = subCommand;
                Content = content;
            }
        }

        /// <summary>
        /// Websocket缓存段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class WsBufferSegmentSegment<T> : ReadOnlySequenceSegment<T>, IDisposable
        {
            private readonly IMemoryOwner<T> _memoryOwner;

            public WsBufferSegmentSegment(IMemoryOwner<T> memoryOwner, int start, int length)
            {
                _memoryOwner = memoryOwner;
                Memory = _memoryOwner.Memory.Slice(start, length);
            }

            public WsBufferSegmentSegment<T> Add(IMemoryOwner<T> memoryOwner, int start, int length, int runningIndex)
            {
                var segment = new WsBufferSegmentSegment<T>(memoryOwner, start, length)
                {
                    RunningIndex = runningIndex
                };

                Next = segment;
                return segment;
            }

            public void Dispose()
            {
                if (Next is WsBufferSegmentSegment<T> chunk)
                    chunk.Dispose();
                _memoryOwner.Dispose();
                GC.SuppressFinalize(this);
            }
        }
        /// <summary>
        /// websocket缓存序列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class WsBufferSequence<T> : IDisposable
        {
            private readonly WsBufferSegmentSegment<T>? _start;
            public readonly ReadOnlySequence<T> Sequence;

            public WsBufferSequence(WsBufferSegmentSegment<T>? start, int startIndex, WsBufferSegmentSegment<T>? end, int endIndex)
            {
                _start = start;
                if (start == null || end == null)
                    Sequence = new ReadOnlySequence<T>();
                else
                    Sequence = new ReadOnlySequence<T>(start, startIndex, end, endIndex);
            }

            public void Dispose()
            {
                _start?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
        private ClientWebSocket _wsClient = new();
        private object _wsSendLock = new();

        /// <summary>
        /// 负责记录syncId对应的响应
        /// </summary>
        private ConcurrentDictionary<ushort, Action<WsBufferSequence<byte>>> _syncResponseTable = new();

        /// <summary>
        /// 当前Sync Id
        /// </summary>
        private ushort _currentSyncId = 1;

        internal WsAdapter(MiraiConfig config) 
        {
            _config = config;
        }

        /// <summary>
        /// 从Ws中拉取
        /// </summary>
        private async Task<WsBufferSequence<byte>> PollFromWebSocketAsync(CancellationToken cancellation)
        {
            WsBufferSegmentSegment<byte>? begin = null;
            WsBufferSegmentSegment<byte>? current = null;
            int currentReceivedCount = 0;
            int totalReceivedCount = 0;

            while (!cancellation.IsCancellationRequested)
            {
                var receiveBuffer = MemoryPool<byte>.Shared.Rent();
                var receiveResult = await _wsClient.ReceiveAsync(receiveBuffer.Memory, cancellation);

                currentReceivedCount = receiveResult.Count;
                if (current == null)
                {
                    begin = new WsBufferSegmentSegment<byte>(receiveBuffer, 0, currentReceivedCount);
                    current = begin;
                }
                else
                    current = current.Add(receiveBuffer, 0, currentReceivedCount, totalReceivedCount);
                totalReceivedCount += currentReceivedCount;
                if (receiveResult.EndOfMessage)
                    break;
            }

            return new WsBufferSequence<byte>(begin, 0, current, currentReceivedCount);
        }

        /// <summary>
        /// 启动WsAdapter
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellation)
        {
            _wsClient.Options.SetRequestHeader("verifyKey", _config.VerifyKey);
            _wsClient.Options.SetRequestHeader("qq", _config.Id.ToString());
            await _wsClient.ConnectAsync(new Uri($"ws://{_config.Host}:{_config.Port}/all"), cancellation);
            using var verifyResponse = await PollFromWebSocketAsync(cancellation);

            var verifyResponseObj = ReadWsResponcePackage<Verify.Response>(verifyResponse.Sequence);
            sessionKey = verifyResponseObj.Data.Session;

            _ = Task.Factory.StartNew(() => EventLoop(cancellation), cancellation);
        }

        private async void EventLoop(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    using var loopResponse = await PollFromWebSocketAsync(cancellation);
                    var testStr = Encoding.UTF8.GetString(loopResponse.Sequence);
                    var syncId = GetSyncId(loopResponse.Sequence);
                    if (syncId == -1)
                        ReadWsEvent(loopResponse.Sequence, EventHookTable);
                    else
                    {
                        var ushortSyncId = (ushort)syncId;
                        if (_syncResponseTable.TryGetValue(ushortSyncId, out var result))
                            result(loopResponse);
                    }
                }
                catch (Exception ex)
                {
                    // Log error
                    Console.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        private async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, string cmd)
            where TRequest : MiraiJsonSerializers.ISerializable<TRequest>, new()
            where TResponse : MiraiJsonSerializers.ISerializable<TResponse>, new()
        {
            var arrayBuffer = new ArrayBufferWriter<byte>();
            var syncId = _currentSyncId++;
            var package = new RequestPackage<TRequest>(syncId, cmd, null, request);

            // Send request
            WriteWsRequestPackage(arrayBuffer, package);
            var str = Encoding.UTF8.GetString(arrayBuffer.WrittenSpan);
            bool lockWasTaken = false;
            try
            {
                Monitor.Enter(_wsSendLock, ref lockWasTaken);
                await _wsClient.SendAsync(arrayBuffer.WrittenMemory, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            finally
            {
                if (lockWasTaken) 
                    Monitor.Exit(_wsSendLock);
            }

            // On response
            using Semaphore sema = new Semaphore(0, 1);
            ResponsePackage<TResponse>? response = default;
            var onResponse = (WsBufferSequence<byte> buffer) =>
            {
                response = ReadWsResponcePackage<TResponse>(buffer.Sequence);
                sema.Release();
            };

            _syncResponseTable.TryAdd(syncId, onResponse);

            // Wait response
            if (!sema.WaitOne(TimeSpan.FromSeconds(5)))
                throw new TimeoutException();

            _syncResponseTable.TryRemove(syncId, out _);

            if (response == null)
                throw new Exception();

            return response.Value.Data;
        }

        /// <summary>
        /// 写入包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <returns></returns>
        private static void WriteWsRequestPackage<T>(ArrayBufferWriter<byte> arrayBuffer, RequestPackage<T> requestPackage)
            where T : MiraiJsonSerializers.ISerializable<T>, new()
        {
            var writer = new Utf8JsonWriter(arrayBuffer);
            writer.WriteStartObject();
            writer.WriteNumber("syncId", requestPackage.SyncId);
            writer.WriteString("command", requestPackage.Command);
            writer.WriteString("subCommand", requestPackage.SubCommand);
            writer.WritePropertyName("content");
            requestPackage.Content.DefaultConverter.Write(writer, requestPackage.Content);
            writer.WriteEndObject();
            writer.Flush();
        }

        /// <summary>
        /// 读取 Websock 数据包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static ResponsePackage<T> ReadWsResponcePackage<T>(ReadOnlySequence<byte> sequence) 
            where T : MiraiJsonSerializers.ISerializable<T>, new()
        {
            var reader = new Utf8JsonReader(sequence);
            var syncId = -1;
            var obj = new T();

            // object begin
            reader.Read();
            while(true)
            {
                // property name or end object
                reader.Read();

                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                var propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "data":
                        obj.DefaultConverter.Read(ref reader, obj);
                        break;
                    case "syncId":
                        if (reader.TokenType == JsonTokenType.Number)
                            syncId = reader.GetInt32();
                        else
                            int.TryParse(reader.GetString(), out syncId);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return new ResponsePackage<T>(syncId, obj);
        }

        /// <summary>
        /// 读取 Websock 数据包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static object ReadWsEvent(ReadOnlySequence<byte> sequence, MiraiJsonSerializers.EventSerializeHookTable EventHookTable)
        {
            var reader = new Utf8JsonReader(sequence);

            // object begin
            reader.Read();
            while (true)
            {
                // property name or end object
                reader.Read();

                var propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "data":
                        return MiraiJsonSerializers.ReadISerializableEvent(ref reader, EventHookTable);
                    case "syncId":
                        reader.Skip();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static int GetSyncId(ReadOnlySequence<byte> sequence)
        {
            var reader = new Utf8JsonReader(sequence);

            int depth = 0;
            reader.Read();

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        if (depth != 0)
                            break;
                        var name = reader.GetString();
                        if (name != "syncId")
                            break;
                        reader.Read();
                        if (reader.TokenType == JsonTokenType.Number)
                            return reader.GetInt32();
                        else
                        {
                            if (int.TryParse(reader.GetString(), out var syncId))
                                return syncId;
                            return -1;
                        }
                    case JsonTokenType.StartObject:
                        ++depth;
                        break;
                    case JsonTokenType.EndObject:
                        --depth;
                        break;
                }
            }
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
