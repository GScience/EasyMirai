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
        public static string SerializerClassName = "MiraiJsonConverters";

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
    public static class {SerializerClassName}
    {{{converterClassesSource}
{defineMemberConverterSource}

        public static JsonSerializerOptions DefaultOptions = new();

        static {SerializerClassName}()
        {{
{GenConverterStaticCtor()}
{getMemberConverterSource}
        }}

        private delegate void MemberSerializeDelegate<T>(ref Utf8JsonReader reader, T obj, JsonSerializerOptions options);
    }}
}}";
            }
            else
            {
                source += $@"
using System.Text.Json;

namespace {RootNamespace}
{{
    public static class {SerializerClassName}
    {{
        public static JsonSerializerOptions DefaultOptions = new();
    }}
}}";
            }
            sources[MiraiSource.GetOutputFileName(SerializerClassName, "Util")] = source;
        }

        /// <summary>
        /// 创建静态构造器
        /// </summary>
        /// <returns></returns>
        private string GenConverterStaticCtor()
        {
            var ctorBodySource = string.Join(Environment.NewLine, SerializableClasses.Select(c => 
$@"            DefaultOptions.Converters.Add(new {GetClassConverterName(c)}());"));

            return ctorBodySource;
        }

        /// <summary>
        /// 生成指定类型的序列化构造器
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        private string GenConverterSourceFor(ClassDef classDef)
        {
            foreach (var member in classDef.Members.Values)
                _converterGetterTable[GetMemberConverterName(member)] = member.GetCSharpMemberType();

            var setPropertyCaseSource = string.Join(Environment.NewLine, classDef.Members.Values.Select((m, i) => $@"
                                case ""{m.Name.ToLowerCamel()}"":
                                    if ({GetMemberConverterName(m)} is JsonConverter<{m.GetCSharpMemberType()}> tConverter{i})
                                        obj.{m.Name.ToUpperCamel()} = tConverter{i}.Read(ref reader, typeof({m.GetCSharpMemberType()}), options); 
                                    else
                                        obj.{m.Name.ToUpperCamel()} = JsonSerializer.Deserialize<{m.GetCSharpMemberType()}>(ref reader, options); 
                                    break;"));

            var getPropertyClassSource = string.Join(Environment.NewLine, classDef.Members.Values.Select((m, i) => $@"
                writer.WritePropertyName(""{m.Name.ToLowerCamel()}"");
                if ({GetMemberConverterName(m)} is JsonConverter<{m.GetCSharpMemberType()}> tConverter{i})
                    tConverter{i}.Write(writer, value.{m.Name.ToUpperCamel()}, options); 
                else
                    JsonSerializer.Serialize(writer, value.{m.Name.ToUpperCamel()}, options);"));

            var setPropertyTableSource = string.Join($", {Environment.NewLine}", classDef.Members.Values.Select(m => $@"
                {{ 
                    ""{m.Name.ToLowerCamel()}"", 
                    Setter_{m.Name.ToUpperCamel()}
                }}"));

            var setPropertyFunctionSource = string.Join(Environment.NewLine, classDef.Members.Values.Select(m => $@"
            private static void Setter_{m.Name.ToUpperCamel()}(ref Utf8JsonReader reader, {classDef.FullName} obj, JsonSerializerOptions options)
            {{
                if ({GetMemberConverterName(m)} is JsonConverter<{m.GetCSharpMemberType()}> tConverter)
                    obj.{m.Name.ToUpperCamel()} = tConverter.Read(ref reader, typeof({m.GetCSharpMemberType()}), options); 
                else
                    obj.{m.Name.ToUpperCamel()} = JsonSerializer.Deserialize<{m.GetCSharpMemberType()}>(ref reader, options); 
            }}"));

            var converterSource = $@"
        internal class {GetClassConverterName(classDef)} : JsonConverter<{classDef.FullName}>
        {{{setPropertyFunctionSource}
            private Dictionary<string, MemberSerializeDelegate<{classDef.FullName}>> _memberTable = new()
            {{{setPropertyTableSource}
            }};
            
            public override {classDef.FullName} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                            //_memberTable[propertyName](ref reader, obj, options);
                            switch (propertyName)
                            {{{setPropertyCaseSource}
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
            
            public override void Write(Utf8JsonWriter writer, {classDef.FullName} value, JsonSerializerOptions options)
            {{
                writer.WriteStartObject();{getPropertyClassSource}
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

        /// <summary>
        /// 获取成员对应转换器名称
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static string GetMemberConverterName(MemberDef member)
        {
            return $"_converter{ReplaceIllegalNameChar(member.GetCSharpMemberType().ToUpperCamel())}";
        }

        public static string ReplaceIllegalNameChar(string name)
        {
            return name.Replace('.', '_').Replace('>', '_').Replace('<', '_');
        }
    }
}
