using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp.Generator
{
    internal class IMessageGenerator : GeneratorBase
    {
        public static string RootNamespace => $"{MiraiSource.RootNamespace}.Message";

        public ClassDef InterfaceDef { get; private set; }

        public override void PreProcessing(ClassDef classDef)
        {
            base.PreProcessing(classDef);
            classDef.Namespace = RootNamespace;
            InterfaceDef = classDef;
        }

        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            string source = $@"{base.GenerateFrom(classDef, namespaceDef + ".Message")}\n
#nullable enable
namespace {RootNamespace}
{{
    public interface {classDef.Name}
    {{
    
    }}
}}
#nullable restore";
            return source;
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "Message";
        }
    }
}
