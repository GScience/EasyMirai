using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using EasyMirai.Generator.CSharp.Extensions;
using System.Runtime.CompilerServices;

namespace EasyMirai.Generator.CSharp.Generator
{
    /// <summary>
    /// 生成序列化
    /// </summary>
    internal class SerializeGenerator : GeneratorBase
    {
        public static string RootNamespace = $"{MiraiSource.RootNamespace}.Util";
        public static string SerializerClassName = "MiraiJsonSerializers";

        public static string GetFullNameOf(string className)
        {
            return $"global::{RootNamespace}.{SerializerClassName}.{className}";
        }

        /// <summary>
        /// 是否生成序列化代码
        /// </summary>
        public static bool GenerateSerializeSource = true;

        public static readonly List<ClassDef> SerializableClasses
            = new List<ClassDef>();

        /// <summary>
        /// 转换器表
        /// </summary>
        private Dictionary<string, string> _converterGetterTable 
            = new Dictionary<string, string>();

        public override void Init()
        {
            base.Init();
            SerializableClasses.Clear();
        }

        MessageGenerator _messageGenerator;
        EventGenerator _eventGenerator;
        IMessageGenerator _iMessageGenerator;
        IEventGenerator _iEventGenerator;

        /// <summary>
        /// 生成序列化代码
        /// </summary>
        /// <param name="sources"></param>
        public override void PostProcessing(Dictionary<string, string> sources)
        {
            _messageGenerator = SourceGeneratorTable[MiraiModule.CategoryMessage] as MessageGenerator;
            _eventGenerator = SourceGeneratorTable[MiraiModule.CategoryEvent] as EventGenerator;
            _iMessageGenerator = SourceGeneratorTable[MiraiModule.CategoryIMessage] as IMessageGenerator;
            _iEventGenerator = SourceGeneratorTable[MiraiModule.CategoryIEvent] as IEventGenerator;

            var source = GenerateSourceHead();

            if (GenerateSerializeSource)
            {            
                var converterClassesSource 
                    = string.Join(
                        Environment.NewLine, 
                        SerializableClasses.Select(c => GenConverterSourceFor(c)));

                var getMemberConverterSource
                    = string.Join(
                        "",
                        _converterGetterTable.Select(pair => $@"
            {pair.Key} = DefaultOptions.GetConverter(typeof({pair.Value}));"));

                source += $@"
namespace {RootNamespace}
{{
    using {GetClassConverterName(_iMessageGenerator.InterfaceDef)} = {SerializerClassName}.IMessageConverter;
    using {GetClassConverterName(_iEventGenerator.InterfaceDef)} = {SerializerClassName}.IEventConverter;

    public static partial class {SerializerClassName}
    {{{converterClassesSource}
        public sealed partial class ConverterWrapper<T> where T : ISerializable<T>, new()
        {{
            internal delegate void ReadDelegate(ref global::System.Text.Json.Utf8JsonReader reader, T obj);
            internal delegate void WriteDelegate(global::System.Text.Json.Utf8JsonWriter writer, T value);

            internal ReadDelegate ReadHandler {{ get; }}
            internal WriteDelegate WriteHandler {{ get; }}

            internal ConverterWrapper(ReadDelegate read, WriteDelegate write)
            {{
                ReadHandler = read;
                WriteHandler = write;
            }}
        }}
        public interface ISerializable<T> where T : ISerializable<T>, new()
        {{
            ConverterWrapper<T> DefaultConverter {{ get; }}
        }}

        internal interface ISerializableMessage
        {{
            string Type {{ get; }}
        }}

        internal interface ISerializableEvent
        {{
            string Type {{ get; }}
        }}
        
        {GenEventSerializeHookTableClassCode()}

        {GenMessageDeserializerCode()}

        {GenMessageSerializerCode()}

        {GenEventSerializerCode()}

        {GenEventDeserializerCode()}
    }}
}}";
            }
            else
            {
                source += $@"
#nullable enable
using System.Text.Json;

namespace {RootNamespace}
{{
    internal static partial class {SerializerClassName}
    {{
        
    }}
}}
#nullable restore";
            }
            sources[MiraiSource.GetOutputFileName(SerializerClassName, "Util")] = source;
        }
        private string GenEventSerializeHookTableClassCode()
        {
            var eventTable = _eventGenerator.EventTable;

            var classBodyCode = string.Join(Environment.NewLine, eventTable.Select(pair => $@"
            public Action<{pair.Key.FullName}> {pair.Key.Name.ToUpperCamel()};"));

            var source = $@"
        public class EventSerializeHookTable
        {{
            {classBodyCode}
        }}";
            return source;
        }

        private string GenMessageSerializerCode()
        {
            var messageTable = _messageGenerator.MessageTable;

            var switchBodyCode = string.Join(Environment.NewLine, messageTable.Select(pair => $@"
                case ""{pair.Value}"":
                    {GetClassConverterName(pair.Key)}.Write(writer, ({pair.Key.FullName})message);
                    break;"));

            var source = $@"
        internal static void WriteISerializableMessage(global::System.Text.Json.Utf8JsonWriter writer, ISerializableMessage message)
        {{
            switch (message.Type)
            {{
                {switchBodyCode}
                
                default:
                    throw new NotImplementedException();
            }}
        }}";
            return source;
        }

        private string GenMessageDeserializerCode()
        {
            var messageTable = _messageGenerator.MessageTable;

            var switchBodyCode = string.Join(Environment.NewLine, messageTable.Select(pair => $@"
                case ""{pair.Value}"":
                    var obj{pair.Value} = new {pair.Key.FullName}();
                    {GetClassConverterName(pair.Key)}.Read(ref reader, obj{pair.Value});
                    return (ISerializableMessage)obj{pair.Value};"));

            var source = $@"
        internal static ISerializableMessage ReadISerializableMessage(ref global::System.Text.Json.Utf8JsonReader reader)
        {{
            var type = GetJsonObjectType(reader);
            switch (type)
            {{
                {switchBodyCode}
                
                default:
                    throw new NotImplementedException();
            }}
        }}";
            return source;
        }
        private string GenEventSerializerCode()
        {
            var eventTable = _eventGenerator.EventTable;

            var switchBodyCode = string.Join(Environment.NewLine, eventTable.Select(pair => $@"
                case ""{pair.Value}"":
                    {GetClassConverterName(pair.Key)}.Write(writer, ({pair.Key.FullName})e);
                    break;"));

            var source = $@"
        internal static void WriteISerializableEvent(global::System.Text.Json.Utf8JsonWriter writer, ISerializableEvent e)
        {{
            switch (e.Type)
            {{
                {switchBodyCode}
                
                default:
                    throw new NotImplementedException();
            }}
        }}";
            return source;
        }

        private string GenEventDeserializerCode()
        {
            var eventTable = _eventGenerator.EventTable;

            var switchBodyCode = string.Join(Environment.NewLine, eventTable.Select(pair => $@"
                case ""{pair.Value}"":
                    var obj{pair.Value} = new {pair.Key.FullName}();
                    {GetClassConverterName(pair.Key)}.Read(ref reader, obj{pair.Value});
                    var hook{pair.Key.Name.ToUpperCamel()} = hookTable?.{pair.Key.Name.ToUpperCamel()};
                    if (hook{pair.Key.Name.ToUpperCamel()} != null)
                        Task.Factory.StartNew(() => hook{pair.Key.Name.ToUpperCamel()}(obj{pair.Value}));
                    return (ISerializableEvent)obj{pair.Value};"));

            var source = $@"
        internal static ISerializableEvent ReadISerializableEvent(ref global::System.Text.Json.Utf8JsonReader reader, EventSerializeHookTable hookTable = null)
        {{
            var type = GetJsonObjectType(reader);
            switch (type)
            {{
                {switchBodyCode}
                
                default:
                    throw new NotImplementedException();
            }}
        }}";
            return source;
        }

        private string GenWriteValueSource(MemberDef m, bool isListComponent = false, string listComponentName = "")
        {
            string source;

            var type = m.Type;
            bool isBoxing = 
                type == MemberType.Boolean || 
                type == MemberType.Int || 
                type == MemberType.Long;
            var name = $"value.{m.Name.ToUpperCamel()}{(isBoxing ? ".Value" : "")}";
            if (isListComponent)
            {
                type = m.GetListComponentType();
                name = listComponentName;
            }

            switch (type)
            {
                case MemberType.String:
                    source = $@"writer.WriteStringValue({name});";
                    break;

                case MemberType.Int:
                case MemberType.Long:
                    source = $@"writer.WriteNumberValue({name});";
                    break;

                case MemberType.Boolean:
                    source = $@"writer.WriteBooleanValue({name});";
                    break;

                case MemberType.Object:
                    source = $@"{GetClassConverterName(m.Reference)}.Write(writer, {name});";
                    break;

                case MemberType.StringList:
                case MemberType.IntList:
                case MemberType.LongList:
                case MemberType.BooleanList:
                case MemberType.ObjectList:
                    source = $@"
                    writer.WriteStartArray();
                    if (value.{m.Name.ToUpperCamel()} != null)
                    {{
                        foreach (var element in value.{m.Name.ToUpperCamel()})
                            {GenWriteValueSource(m, true, "element")}
                    }}
                    writer.WriteEndArray();";
                    break;

                case MemberType.Stream:
                    source = "";
                    break;

                default:
                    source = $@"throw new NotImplementedException();";
                    break;
            }

            return source;
        }

        private string GenReadValueSource(MemberDef m, bool isListComponent = false)
        {
            MemberType type = m.Type;
            if (isListComponent)
                type = m.GetListComponentType();

            switch (type)
            {
                case MemberType.Boolean:
                    return $@"reader.GetBoolean()";
                case MemberType.Int:
                    return $@"reader.GetInt32()";
                case MemberType.Long:
                    return $@"reader.GetInt64()";
                case MemberType.String:
                    return $@"reader.GetString()";
                case MemberType.Object:
                    return $@"{GetClassConverterName(m.Reference)}.Read(ref reader)";
                case MemberType.BooleanList:
                case MemberType.IntList:
                case MemberType.LongList:
                case MemberType.StringList:
                case MemberType.ObjectList:
                    return $@"var list = new List<{m.GetCSharpMemberListComponentType()}>();

                                    while(true)
                                    {{
                                        reader.Read();
                                        if (reader.TokenType == global::System.Text.Json.JsonTokenType.EndArray)
                                            break;
                                        list.Add({GenReadValueSource(m, true)});
                                    }}";
                case MemberType.Stream:
                    return "";
                default:
                    return $@"throw new NotImplementedException()";
            }
        }

        private string GenWritePropertyClassSource(MemberDef m)
        {
            var source = $@"
                writer.WritePropertyName(""{m.Name.ToLowerCamel()}"");
                if (value.{m.Name.ToUpperCamel()} == null)
                    writer.WriteNullValue();
                else
                {{
                    {GenWriteValueSource(m)}
                }}";

            return source;
        }

        /// <summary>
        /// 生成指定类型的序列化构造器
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        private string GenConverterSourceFor(ClassDef classDef, bool isListComponent = false)
        {
            var readPropertyCaseSource = string.Join(Environment.NewLine, classDef.Members.Values.Select((m, i) =>
            {
                switch (m.Type)
                {
                    case MemberType.Boolean:
                        return $@"
                                case ""{m.Name.ToLowerCamel()}"":
                                    if (reader.TokenType == global::System.Text.Json.JsonTokenType.Null) break;
                                    obj.{m.Name.ToUpperCamel()} = {GenReadValueSource(m)};
                                    break;";
                    case MemberType.Int:
                        return $@"
                                case ""{m.Name.ToLowerCamel()}"":
                                    if (reader.TokenType == global::System.Text.Json.JsonTokenType.Null) break;
                                    obj.{m.Name.ToUpperCamel()} = {GenReadValueSource(m)};
                                    break;";
                    case MemberType.Long:
                        return $@"
                                case ""{m.Name.ToLowerCamel()}"":
                                    if (reader.TokenType == global::System.Text.Json.JsonTokenType.Null) break;
                                    obj.{m.Name.ToUpperCamel()} = {GenReadValueSource(m)};
                                    break;";
                    case MemberType.String:
                        return $@"
                                case ""{m.Name.ToLowerCamel()}"":
                                    if (reader.TokenType == global::System.Text.Json.JsonTokenType.Null) break;
                                    obj.{m.Name.ToUpperCamel()} = {GenReadValueSource(m)};
                                    break;";
                    case MemberType.Object:
                        return $@"
                                case ""{m.Name.ToLowerCamel()}"":
                                    if (reader.TokenType == global::System.Text.Json.JsonTokenType.Null) break;
                                    obj.{m.Name.ToUpperCamel()} = {GenReadValueSource(m)};
                                    break;";
                    case MemberType.BooleanList:
                    case MemberType.IntList:
                    case MemberType.LongList:
                    case MemberType.StringList:
                    case MemberType.ObjectList:
                        return $@"
                                case ""{m.Name.ToLowerCamel()}"":
                                    {GenReadValueSource(m)};
                                    obj.{m.Name.ToUpperCamel()} = list;
                                    break;";
                    case MemberType.Stream:
                        return "";
                    default:
                        return $@"
                                case ""{m.Name.ToLowerCamel()}"":
                                    throw new NotImplementedException();
                                    break;";
                }
            }));

            var writePropertyClassSource = string.Join(Environment.NewLine, classDef.Members.Values.Select(GenWritePropertyClassSource));

            // 消息或者事件需要额外写入type
            var writeMessageOrEventType = "";

            if (_messageGenerator.MessageTable.TryGetValue(classDef, out var typeName))
                writeMessageOrEventType = $@"
                writer.WritePropertyName(""type"");
                writer.WriteStringValue(""{typeName}"");";

            var converterSource = $@"
        internal static class {GetClassConverterName(classDef)}
        {{
            public static {classDef.FullName} Read(ref global::System.Text.Json.Utf8JsonReader reader)
            {{
                var obj = new {classDef.FullName}();
                Read(ref reader, obj);
                return obj;
            }}

            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            public static void Read(ref global::System.Text.Json.Utf8JsonReader reader, {classDef.FullName} obj)
            {{
                if (reader.TokenType == global::System.Text.Json.JsonTokenType.Null)
                    return;
                bool readToEnd = false;
                do
                {{
                    reader.Read();
                    
                    switch (reader.TokenType)
                    {{
                        case global::System.Text.Json.JsonTokenType.EndObject:
                            readToEnd = true;
                            break;
                        case global::System.Text.Json.JsonTokenType.PropertyName:
                            var propertyName = reader.GetString();
                            reader.Read();
                            switch (propertyName)
                            {{{readPropertyCaseSource}
                                default:
                                    break;
                            }}
                            break;
                        default:
                            break;
                    }}
                }} while (!readToEnd);
            }}

            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            public static void Write(global::System.Text.Json.Utf8JsonWriter writer, {classDef.FullName} value)
            {{
                if (value == null)
                {{
                    writer.WriteNullValue();
                    return;
                }}
                writer.WriteStartObject();{writeMessageOrEventType}{writePropertyClassSource}
                writer.WriteEndObject();
            }}
        }}";

            return converterSource;
        }

        /// <summary>
        /// 获取类型对应转换器的名称
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        public static string GetClassConverterName(ClassDef classDef)
        {
            return $"{ReplaceIllegalNameChar(classDef.FullName.ToUpperCamel())}Converter";
        }

        private static string ReplaceIllegalNameChar(string name)
        {
            return new string(name.Select(c =>
            {
                if (c == '<' || c == '>' || c == ':' || c == '.')
                    return '_';
                return c;
            }).ToArray());
        }
    }
}
