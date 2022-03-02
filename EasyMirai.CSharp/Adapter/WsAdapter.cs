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

namespace EasyMirai.CSharp.Adapter
{
    /// <summary>
    /// Websocket adapter
    /// </summary>
    public partial class WsAdapter : IDisposable
    {
        /// <summary>
        /// Ws 数据包
        /// </summary>
        internal class NetPackage<T>
        {
            public int SyncId { get; }
            public T Data { get; }

            public NetPackage(int syncId, T data)
            {
                SyncId = syncId;
                Data = data;
            }

        }

        /// <summary>
        /// 消息块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class MemoryChunk<T> : ReadOnlySequenceSegment<T>, IDisposable
        {
            private IMemoryOwner<T> _memoryOwner;

            public MemoryChunk(IMemoryOwner<T> memoryOwner, int start, int length)
            {
                _memoryOwner = memoryOwner;
                Memory = _memoryOwner.Memory.Slice(start, length);
            }

            public MemoryChunk<T> Add(IMemoryOwner<T> memoryOwner, int start, int length, int runningIndex)
            {
                var segment = new MemoryChunk<T>(memoryOwner, start, length)
                {
                    RunningIndex = runningIndex
                };

                Next = segment;
                return segment;
            }

            public void Dispose()
            {
                if (Next is MemoryChunk<T> chunk)
                    chunk.Dispose();
            }
        }

        public const int MemoryBufferSize = 8;

        private ClientWebSocket _wsClient = new();

        internal WsAdapter() { }

        /// <summary>
        /// 从Ws中拉取
        /// </summary>
        private async Task<ReadOnlySequence<byte>> PollFromWebSocketAsync(CancellationToken cancellation)
        {
            MemoryChunk<byte>? begin = null;
            MemoryChunk<byte>? current = null;
            int currentLength = 0;
            int runningIndex = 0;

            while (!cancellation.IsCancellationRequested)
            {
                var memoryBuffer = MemoryPool<byte>.Shared.Rent(MemoryBufferSize);
                var receiveResult = await _wsClient.ReceiveAsync(memoryBuffer.Memory, cancellation);

                currentLength = receiveResult.Count;
                if (current == null)
                {
                    begin = new MemoryChunk<byte>(memoryBuffer, 0, currentLength);
                    current = begin;
                }
                else
                    current = current.Add(memoryBuffer, 0, currentLength, runningIndex);
                runningIndex += currentLength;
                if (receiveResult.EndOfMessage)
                    break;
            }

            var readOnlySequences = new ReadOnlySequence<byte>(begin!, 0, current!, currentLength);
            return readOnlySequences;
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
            /*request.DefaultConverter.Write(writer, request);
            writer.Flush();
            var requestJson = Encoding.UTF8.GetString(testStream.ToArray());

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes("{\"a\":1}"));

            var response = new TResponse();
            response.DefaultConverter.Read("{}", response);
            if (response == null)
                throw new Exception("Failed to deserialize object");
            return response;*/
            return default(TResponse);
        }

        /// <summary>
        /// 从配置中创建WsAdapter
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static async Task<WsAdapter> CreateAsync(MiraiConfig config, CancellationToken cancellation)
        {
            return await CreateAsync(config, "", cancellation);
        }
        
        /// <summary>
        /// 从已有SessionKey中创建WsAdapter
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static async Task<WsAdapter> CreateAsync(MiraiConfig config, string sessionKey, CancellationToken cancellation)
        {
            WsAdapter wsAdapter = new();
            wsAdapter._wsClient.Options.SetRequestHeader("verifyKey", config.VerifyKey);
            wsAdapter._wsClient.Options.SetRequestHeader("qq", config.Id.ToString());
            if (!string.IsNullOrEmpty(sessionKey))
                wsAdapter._wsClient.Options.SetRequestHeader("sessionKey", sessionKey);
            await wsAdapter._wsClient.ConnectAsync(new Uri($"ws://{config.Host}:{config.Port}/all"), cancellation);
            var response = await wsAdapter.PollFromWebSocketAsync(cancellation);
            
            var responseObj = ReadWsPackage<Verify.Response>(response);

            return wsAdapter;
        }

        /// <summary>
        /// 读取 Websock 数据包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static NetPackage<T> ReadWsPackage<T>(ReadOnlySequence<byte> sequence) where T : MiraiJsonSerializers.ISerializable<T>, new()
        {
            //var str = Encoding.UTF8.GetString(sequence);
            //var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(str));
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
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return new NetPackage<T>(syncId, obj);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
