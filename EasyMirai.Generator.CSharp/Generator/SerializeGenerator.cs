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

        /// <summary>
        /// 生成序列化代码
        /// </summary>
        /// <param name="sources"></param>
        public override void PostProcessing(Dictionary<string, string> sources)
        {
            var source = string.Join(Environment.NewLine, SerializableClasses.Select(c=>c.FullName));

            source = GenerateSourceHead();

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

                var defineMemberConverterSource
                    = string.Join(
                        "",
                        _converterGetterTable.Select(pair => $@"
        private static JsonConverter {pair.Key};"));

                source += $@"
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using {ApiGenerator.RootNamespace};
using {EventGenerator.RootNamespace};
using {MessageGenerator.RootNamespace};

namespace {RootNamespace}
{{
    public static partial class {SerializerClassName}
    {{{converterClassesSource}
{defineMemberConverterSource}
    }}
}}";
            }
            else
            {
                source += $@"
using System.Text.Json;

namespace {RootNamespace}
{{
    public static partial class {SerializerClassName}
    {{
        
    }}
}}";
            }
            sources[MiraiSource.GetOutputFileName(SerializerClassName, "Util")] = source;
        }

        private string GenWriteValueSource(MemberDef m, bool isListComponent = false, string listComponentName = "")
        {
            string source;

            var type = m.Type;
            var name = $"value.{m.Name.ToUpperCamel()}";
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
                        {GenWriteValueSource(m, true, "element")};
                }}
                writer.WriteEndArray();";
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
                default:
                    return $@"
                                        var list = new List<{m.GetCSharpMemberListComponentType()}>();

                                        while(true)
                                        {{
                                            reader.Read();
                                            if (reader.TokenType == JsonTokenType.EndArray)
                                                break;
                                            list.Add({GenReadValueSource(m, true)});
                                        }}";
            }
        }

        private string GenWritePropertyClassSource(MemberDef m)
        {
            var source = $@"
                writer.WritePropertyName(""{m.Name.ToLowerCamel()}"");
                {GenWriteValueSource(m)}";

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
                                        obj.{m.Name.ToUpperCamel()} = {GenReadValueSource(m)};
                                        break;";
                    case MemberType.Int:
                        return $@"
                                    case ""{m.Name.ToLowerCamel()}"":
                                        obj.{m.Name.ToUpperCamel()} = {GenReadValueSource(m)};
                                        break;";
                    case MemberType.Long:
                        return $@"
                                    case ""{m.Name.ToLowerCamel()}"":
                                        obj.{m.Name.ToUpperCamel()} = {GenReadValueSource(m)};
                                        break;";
                    case MemberType.String:
                        return $@"
                                    case ""{m.Name.ToLowerCamel()}"":
                                        obj.{m.Name.ToUpperCamel()} = {GenReadValueSource(m)};
                                        break;";
                    case MemberType.Object:
                        return $@"
                                    case ""{m.Name.ToLowerCamel()}"":
                                        obj.{m.Name.ToUpperCamel()} = {GenReadValueSource(m)};
                                        break;";
                    default:
                        return $@"
                                    case ""{m.Name.ToLowerCamel()}"":
                                        {GenReadValueSource(m)};
                                        obj.{m.Name.ToUpperCamel()} = list;
                                        break;";
                }
            }));

            var writePropertyClassSource = string.Join(Environment.NewLine, classDef.Members.Values.Select(GenWritePropertyClassSource));

            var converterSource = $@"
        public class {GetClassConverterName(classDef)}
        {{
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static {classDef.FullName} Read(ref Utf8JsonReader reader)
            {{
                var obj = new {classDef.FullName}();
                
                bool readToEnd = false;
                do
                {{
                    reader.Read();
                    switch (reader.TokenType)
                    {{
                        case JsonTokenType.EndObject:
                            readToEnd = true;
                            break;
                        case JsonTokenType.PropertyName:
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
                return obj;
            }}

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Write(Utf8JsonWriter writer, {classDef.FullName} value)
            {{
                if (value == null)
                {{
                    writer.WriteNullValue();
                    return;
                }}
                writer.WriteStartObject();{writePropertyClassSource}
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

        public static string ReplaceIllegalNameChar(string name)
        {
            return name.Replace('.', '_').Replace('>', '_').Replace('<', '_');
        }
    }
}
