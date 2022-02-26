using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp.Generator
{
    internal class IMessageGenerator : GeneratorBase
    {
        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            string source = $@"{base.GenerateFrom(classDef, namespaceDef + ".Message")}\n
using System;
using System.Text.Json.Serialization;

namespace {namespaceDef}.Message
{{
    public interface {classDef.Name}
    {{
    
    }}
}}
";
            return source;
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "Message";
        }
    }
}
