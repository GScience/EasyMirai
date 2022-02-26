using ProtocolGenerator.Module;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace SourceGenerator.CSharp.Generator
{
    internal class ObjectGenerator : GeneratorBase
    {
        /// <summary>
        /// 生成对象源码，忽略 namespace
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        public string GenObjectSource(ClassDef classDef, int depth = 0)
        {
            var newLine = Environment.NewLine + new string('\t', depth);

            // 成员定义
            var memberDefs = classDef.Members.Values.Select(memberDef =>
            {
                var memberTypeName = "";

                switch (memberDef.Type)
                {
                    case MemberType.String:
                        memberTypeName = "string";
                        break;
                    case MemberType.StringList:
                        memberTypeName = "List<string>";
                        break;
                    case MemberType.Boolean:
                        memberTypeName = "bool";
                        break;
                    case MemberType.BooleanList:
                        memberTypeName = "List<bool>";
                        break;
                    case MemberType.Int:
                        memberTypeName = "int";
                        break;
                    case MemberType.IntList:
                        memberTypeName = "List<int>";
                        break;
                    case MemberType.Long:
                        memberTypeName = "long";
                        break;
                    case MemberType.LongList:
                        memberTypeName = "List<long>";
                        break;
                    case MemberType.Object:
                        memberTypeName = memberDef.Reference.Name;
                        break;
                    case MemberType.ObjectList:
                        memberTypeName = $"List<{memberDef.Reference.Name}>";
                        break;
                    default:
                        throw new NotImplementedException($"Unknown type {memberDef.Type}");
                }

                var memberComment = $"/// <summary>{newLine}\t/// {memberDef.Description}{newLine}\t/// </summary>";
                var jsonPropertyName = $"[JsonPropertyName(\"{memberDef.Name}\")]";

                return 
                    $"{newLine}\t{memberComment}" +
                    $"{newLine}\t{jsonPropertyName}" +
                    $"{newLine}\tpublic {memberTypeName} {FormatName(memberDef.Name)} {{ get; set; }}";
            });

            // 内部类型定义
            var innerClassDefs = classDef.Classes.Select(innerClassDef =>
            {
                return GenObjectSource(innerClassDef, depth + 1);
            });

            var classComment = $"/// <summary>{newLine}/// {classDef.Description}{newLine}/// </summary>";
            var jsonSerializable = $"[JsonSerializable(typeof({classDef.Name}))]";

            string source =
                $"{newLine}{classComment}" +
                $"{newLine}{jsonSerializable}" +
                $"{newLine}public class {classDef.Name}" +
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

            return source;
        }

        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            string source = $@"{base.GenerateFrom(classDef, namespaceDef)}

using System;
using System.Text.Json.Serialization;

namespace {namespaceDef}
{{{GenObjectSource(classDef, 1)}
}}
";
            return source;
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "";
        }
    }
}
