using System;
using System.Collections.Generic;
using System.Linq;
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
                writer.WriteStartObject();
                writer.WriteEndObject();
            }

            public static IMessage Read(ref Utf8JsonReader reader)
            {
                while(reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;
                }
                return null;
            }
        }
    }
}
