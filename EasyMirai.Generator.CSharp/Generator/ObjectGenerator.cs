using EasyMirai.Generator.Module;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using EasyMirai.Generator.CSharp.Extensions;

namespace EasyMirai.Generator.CSharp.Generator
{
    internal class ObjectGenerator : GeneratorBase
    {
        public override void Init()
        {
            base.Init();
        }

        public override void PreProcessing(ClassDef classDef)
        {
            base.PreProcessing(classDef);
            classDef.Namespace = MiraiSource.RootNamespace;

            foreach (var innerClass in classDef.Classes)
                innerClass.Name = "Inner" + innerClass.Name;
        }

        /// <summary>
        /// 生成对象源码，忽略 namespace
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        public static string GenClassSource(ClassDef classDef, int depth = 0, string extraCode = "", string extraInterface = "", bool allowNull = false)
        {
            var newLine = Environment.NewLine + new string('\t', depth);

            // 内部类型定义
            var innerClassDefs = classDef.Classes.Select(innerClassDef =>
            {
                return GenClassSource(innerClassDef, depth + 1, allowNull: true);
            });

            // 成员定义
            var memberDefs = classDef.Members.Values.Select(memberDef =>
            {
                var memberComment = $"/// <summary>{newLine}\t/// {memberDef.Description}{newLine}\t/// </summary>";
                var jsonPropertyName = $"[global::System.Text.Json.Serialization.JsonPropertyName(\"{memberDef.Name}\")]";

                return
                    $"{newLine}\t{memberComment}" +
                    $"{newLine}\t{jsonPropertyName}" +
                    $"{newLine}\tpublic {memberDef.GetCSharpMemberDefine(allowNull:allowNull)} {{ get; set; }}";
            });

            var classComment = $"/// <summary>{newLine}/// {classDef.Description}{newLine}/// </summary>";
            var converterFullName = SerializeGenerator.GetFullNameOf($"{SerializeGenerator.GetClassConverterName(classDef)}");

            // 是否继承其他可序列化对象
            var baseOtherSerializableObj = classDef.Base != null && classDef.Base.Namespace == MiraiSource.RootNamespace;
            string classConverterDefineSource, classConverterGetterSource;

            if (baseOtherSerializableObj)
            {
                classConverterDefineSource
                   = SerializeGenerator.GenerateSerializeSource
                       ? $"[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)] " +
                       $"public static new {SerializeGenerator.GetFullNameOf("ConverterWrapper")}<{classDef.FullName}> defaultConverter " +
                       $"= new ({converterFullName}.Read, {converterFullName}.Write);"
                       : "";
                classConverterGetterSource
                    = SerializeGenerator.GenerateSerializeSource
                        ? $"[global::System.Text.Json.Serialization.JsonIgnore] [global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)] " +
                        $"public new {SerializeGenerator.GetFullNameOf("ConverterWrapper")}<{classDef.FullName}> DefaultConverter " +
                        $"=> {classDef.Name}.defaultConverter;"
                        : "";
            }
            else
            {
                classConverterDefineSource
                    = SerializeGenerator.GenerateSerializeSource
                        ? $"[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)] " +
                        $"public static {SerializeGenerator.GetFullNameOf("ConverterWrapper")}<{classDef.FullName}> defaultConverter " +
                        $"= new ({converterFullName}.Read, {converterFullName}.Write);"
                        : "";
                classConverterGetterSource
                    = SerializeGenerator.GenerateSerializeSource
                        ? $"[global::System.Text.Json.Serialization.JsonIgnore] [global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)] " +
                        $"public {SerializeGenerator.GetFullNameOf("ConverterWrapper")}<{classDef.FullName}> DefaultConverter " +
                        $"=> {classDef.Name}.defaultConverter;"
                        : "";
            }
            var baseClassSource = classDef.Base == null ? "" : $" : {classDef.Base.Name}";

            var classSerializable
                = SerializeGenerator.GenerateSerializeSource
                    ? $"{(string.IsNullOrEmpty(baseClassSource) ? " :" : ",")} {SerializeGenerator.GetFullNameOf("ISerializable")}<{classDef.FullName}>"
                    : "";

            if (!string.IsNullOrEmpty(extraInterface))
                extraInterface = " , " + extraInterface;

            string source =
                $"{newLine}{classComment}" +
                $"{newLine}public class {classDef.Name}{baseClassSource}{classSerializable}{extraInterface}" +
                $"{newLine}{{" +
                // Converter
                $"{newLine}\t{classConverterDefineSource}" +
                $"{newLine}\t{classConverterGetterSource}" +
                $"{newLine}" +
                // Extra
                $"{newLine}{extraCode}" +
                // 成员定义
                $"{newLine}#region Members" +
                $"{newLine}{string.Join(Environment.NewLine, memberDefs)}" + 
                $"{newLine}#endregion" +
                $"{newLine}" +
                // 内部类型定义
                $"{newLine}#region Inner classes" +
                $"{newLine}{string.Join(Environment.NewLine, innerClassDefs)}" +
                $"{newLine}#endregion" +
                $"{newLine}}}";

            SerializeGenerator.SerializableClasses.Add(classDef);

            return source;
        }
        protected virtual string GenerateExtraCode(ClassDef classDef)
        {
            return "";
        }
        protected virtual string GenerateExtraInterface(ClassDef classDef)
        {
            return "";
        }

        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            string source = $@"{base.GenerateFrom(classDef, namespaceDef)}
#nullable enable
namespace {namespaceDef}
{{
    /// <remarks>
    /// Version: {classDef.Version}
    /// </remarks> { GenClassSource(classDef, 1, GenerateExtraCode(classDef), GenerateExtraInterface(classDef), true) }
}}
#nullable restore";
            return source;
        }

    }
}
