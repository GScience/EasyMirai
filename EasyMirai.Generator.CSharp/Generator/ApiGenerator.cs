using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace EasyMirai.Generator.CSharp.Generator
{
    internal class ApiGenerator : GeneratorBase
    {
        public static string RootNamespace => $"{MiraiSource.RootNamespace}.Api";

        public override void PreProcessing(ClassDef classDef)
        {
            base.PreProcessing(classDef);
            classDef.Namespace = RootNamespace;
        }
        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            var source = base.GenerateFrom(classDef, namespaceDef);

            if (classDef.Classes.Count < 2 ||
                !classDef.Classes.Exists(c => c.Name == "Request") ||
                !classDef.Classes.Exists(c => c.Name == "Response"))
                source += $"#error class Request and class Response is required but not found in {classDef.Name}";

            source += $@"
#nullable enable
namespace {RootNamespace}
{{
    /// <summary>
    /// {classDef.Description}
    /// </summary>
    /// <remarks>
    /// Version: {classDef.Version}
    /// </remarks>
    public sealed class {classDef.Name}
    {{{ObjectGenerator.GenClassSource(classDef.Classes[0], 2, allowNull:true)}
{ObjectGenerator.GenClassSource(classDef.Classes[1], 2, allowNull: true)}
    }}
}}
#nullable restore";
            return source;
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "Api";
        }
    }
}
