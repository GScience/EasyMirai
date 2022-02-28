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
        }

        /// <summary>
        /// 生成对象源码，忽略 namespace
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        public static string GenClassSource(ClassDef classDef, int depth = 0)
        {
            var newLine = Environment.NewLine + new string('\t', depth);

            foreach (var innerClass in classDef.Classes)
                innerClass.Name = "Inner" + innerClass.Name;

            // 内部类型定义
            var innerClassDefs = classDef.Classes.Select(innerClassDef =>
            {
                return GenClassSource(innerClassDef, depth + 1);
            });

            // 成员定义
            var memberDefs = classDef.Members.Values.Select(memberDef =>
            {
                // 移除JsonPropertyName因为已经引入静态序列化代码生成
                var memberComment = $"/// <summary>{newLine}\t/// {memberDef.Description}{newLine}\t/// </summary>";
                //var jsonPropertyName = $"[JsonPropertyName(\"{memberDef.Name}\")]";
                return
                    $"{newLine}\t{memberComment}" +
                    //$"{newLine}\t{jsonPropertyName}" +
                    $"{newLine}\tpublic {memberDef.GetCSharpMemberDefine()} {{ get; set; }}";
            });

            var classComment = $"/// <summary>{newLine}/// {classDef.Description}{newLine}/// </summary>";
            var classConverter 
                = $"[JsonConverter(typeof({SerializeGenerator.SerializerClassName}.{SerializeGenerator.GetClassConverterName(classDef)}))]";

            var baseClassSource = classDef.Base == null ? "" : $" : {classDef.Base.Name}";

            string source =
                $"{newLine}{classComment}" +
                $"{newLine}{classConverter}" +
                $"{newLine}public class {classDef.Name}{baseClassSource}" +
                $"{newLine}{{" +
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

        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            string source = $@"{base.GenerateFrom(classDef, namespaceDef)}

using System;
using System.Collections;
using System.Text.Json.Serialization;
using {EventGenerator.RootNamespace};
using {MessageGenerator.RootNamespace};
using {ApiGenerator.RootNamespace};
using {SerializeGenerator.RootNamespace};

namespace {namespaceDef}
{{
    /// <remarks>
    /// Version: {classDef.Version}
    /// </remarks> { GenClassSource(classDef, 1) }
}}
";
            return source;
        }

    }
}
