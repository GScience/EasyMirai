using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using EasyMirai.Generator.CSharp.Extensions;

namespace EasyMirai.Generator.CSharp.Generator
{
    /// <summary>
    /// 生成序列化
    /// </summary>
    internal class SerializeGenerator : GeneratorBase
    {
        public static string RootNamespace = $"{MiraiSource.RootNamespace}.Util";
        public static string SerializerClassName = "MiraiJsonConverters";

        public static readonly List<ClassDef> SerializableClasses
            = new List<ClassDef>();

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

            var converterClassesSource 
                = string.Join(
                    Environment.NewLine, 
                    SerializableClasses.Select(c => GenConverterSourceFor(c)));

            source = GenerateSourceHead();
            source += $@"
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using {ApiGenerator.RootNamespace};
using {EventGenerator.RootNamespace};
using {MessageGenerator.RootNamespace};

namespace {RootNamespace}
{{
    public static class {SerializerClassName}
    {{{converterClassesSource}

        public static JsonSerializerOptions DefaultOptions;

        static {SerializerClassName}()
        {{{GenConverterStaticCtor()}
        }}
    }}
}}";
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
            var setPropertyCaseSource = string.Join(Environment.NewLine, classDef.Members.Values.Select(m => $@"
                            case ""{m.Name.ToLowerCamel()}"":
                                obj.{m.Name.ToUpperCamel()} = JsonSerializer.Deserialize<{m.GetCSharpMemberType()}>(ref reader, options);
                                break;"));

            var getPropertyClassSource = string.Join(Environment.NewLine, classDef.Members.Values.Select(m => $@"
                writer.WritePropertyName(""{m.Name.ToLowerCamel()}"");
                JsonSerializer.Serialize(writer, value.{m.Name.ToUpperCamel()});"));

            var converterSource = $@"
        internal class {GetClassConverterName(classDef)} : JsonConverter<{classDef.FullName}>
        {{
            public override {classDef.FullName} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {{
                var obj = new {classDef.FullName}();

                while (reader.Read())
                {{
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;
                    else if (reader.TokenType == JsonTokenType.PropertyName)
                    {{
                        var propertyName = reader.GetString();
                        reader.Read();
                        switch (propertyName)
                        {{{setPropertyCaseSource}
                            default:
                                break;
                        }}
                    }}
                    else
                        throw new JsonException($""Unknown Token: {{reader.TokenType}}"");
                }}
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
            return $"{classDef.FullName.ToUpperCamel().Replace('.', '_')}Converter";
        }
    }
}
