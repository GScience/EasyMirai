using EasyMirai.Generator;
using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp.Generator
{
    internal class WsAdapterGenerator : GeneratorBase
    {
        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            string source = $@"{base.GenerateFrom(classDef, namespaceDef + ".Adapter")}

using System;
using System.Text.Json.Serialization;

namespace {namespaceDef}.Adapter
{{
    public class WsAdapter
    {{
        
    }}
}}
";
            return source;
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "Adapter";
        }
    }
}
