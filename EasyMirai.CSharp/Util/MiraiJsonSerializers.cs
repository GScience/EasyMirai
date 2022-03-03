using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Buffers;
using System.Threading.Tasks;
using EasyMirai.CSharp.Message;
using EasyMirai.CSharp.Event;

namespace EasyMirai.CSharp.Util
{
    public static partial class MiraiJsonSerializers
    {
        internal static class IEventConverter
        {
            public static void Write(Utf8JsonWriter writer, IEvent value)
            {
                WriteISerializableEvent(writer, (ISerializableEvent)value);
            }

            public static IEvent Read(ref Utf8JsonReader reader)
            {
                var reader2 = reader;
                reader2.Read();
                return (IEvent)ReadISerializableEvent(ref reader);
            }
        }

        internal static class IMessageConverter
        {
            public static void Write(Utf8JsonWriter writer, IMessage value)
            {
                WriteISerializableMessage(writer, (ISerializableMessage)value);
            }

            public static IMessage Read(ref Utf8JsonReader reader)
            {
                var reader2 = reader;
                reader2.Read();
                return (IMessage)ReadISerializableMessage(ref reader);
            }
        }

        /// <summary>
        /// 获取Json对象类型
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static string GetJsonObjectType(Utf8JsonReader reader)
        {
            int depth = 0;

            while (true)
            {
                reader.Read();
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        if (depth != 0)
                            break;
                        var name = reader.GetString();
                        if (name != "type")
                            break;
                        reader.Read();
                        return reader.GetString() ?? "";
                    case JsonTokenType.StartObject:
                        ++depth;
                        break;
                    case JsonTokenType.EndObject:
                        --depth;
                        break;
                }
            }
        }

        public partial class ConverterWrapper<T> where T : ISerializable<T>, new()
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Read(ReadOnlySequence<byte> data, T obj)
            {
                var reader = new Utf8JsonReader(data);
                ReadHandler(ref reader, obj);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Read(ref Utf8JsonReader reader, T obj)
            {
                ReadHandler(ref reader, obj);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Read(ReadOnlySequence<byte> data)
            {
                var reader = new Utf8JsonReader(data);
                var obj = new T();
                ReadHandler(ref reader, obj);
                return obj;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Read(string json, T obj)
            {
                var jsonReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                ReadHandler(ref jsonReader, obj);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Read(ref Utf8JsonReader reader)
            {
                var obj = new T();
                ReadHandler(ref reader, obj);
                return obj;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Read(string json)
            {
                var obj = new T();
                var jsonReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                ReadHandler(ref jsonReader, obj);
                return obj;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(Utf8JsonWriter writer, T obj)
            {
                WriteHandler(writer, obj);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(Stream stream, T obj)
            {
                using var writer = new Utf8JsonWriter(stream);
                WriteHandler(writer, obj);
            }
        }
    }
}
