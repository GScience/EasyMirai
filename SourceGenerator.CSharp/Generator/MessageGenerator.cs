using ProtocolGenerator.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGenerator.CSharp.Generator
{
    internal class MessageGenerator : ObjectGenerator
    {
        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            return base.GenerateFrom(classDef, namespaceDef + ".Message");
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "Message";
        }
    }
}
