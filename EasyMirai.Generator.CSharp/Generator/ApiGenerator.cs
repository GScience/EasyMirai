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

        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            var source = base.GenerateFrom(classDef, namespaceDef);

            if (classDef.Classes.Count < 2 ||
                !classDef.Classes.Exists(c => c.Name == "Request") ||
                !classDef.Classes.Exists(c => c.Name == "Response"))
                source += $"#error class Request and class Response is required but not found in {classDef.Name}";

            source += $@"

using System;
using System.Text.Json.Serialization;
using {EventGenerator.RootNamespace};
using {MessageGenerator.RootNamespace};

namespace {RootNamespace}
{{
    /// <summary>
    /// {classDef.Description}
    /// </summary>
    /// <remarks>
    /// Version: {classDef.Version}
    /// </remarks>
    public class {classDef.Name}
    {{{ObjectGenerator.GenClassSource(classDef.Classes[0], 2)}
{ObjectGenerator.GenClassSource(classDef.Classes[1], 2)}
    }}
}}
";
            return source;
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "Api";
        }

        public override void PostProcessing()
        {
            
        }
    }
}
