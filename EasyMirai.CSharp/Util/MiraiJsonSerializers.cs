using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasyMirai.CSharp.Message;

namespace EasyMirai.CSharp.Util
{
    public static partial class MiraiJsonSerializers
    {
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
            var jsonDocument = JsonDocument.ParseValue(ref reader);
            var type = jsonDocument.RootElement.GetProperty("type").GetString();
            return type!;
        }

        public partial class ConverterWrapper<T> where T : ISerializable<T>, new()
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Read(ref Utf8JsonReader reader, T obj)
            {
                ReadHandler(ref reader, obj);
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
