using ProtocolGenerator.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGenerator.CSharp.Generator
{
    internal class IMessageGenerator : GeneratorBase
    {
        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            string source = $@"{base.GenerateFrom(classDef, namespaceDef)}\n
using System;
using System.Text.Json.Serialization;

namespace {namespaceDef}.Messages
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
            return "Messages";
        }
    }
}
