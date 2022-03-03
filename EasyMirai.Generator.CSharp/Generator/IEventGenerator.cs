﻿using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp.Generator
{
    internal class IEventGenerator : GeneratorBase
    {
        public static string RootNamespace => $"{MiraiSource.RootNamespace}.Event";

        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            string source = $@"{base.GenerateFrom(classDef, namespaceDef + ".Event")}\n
#nullable enable
using System;
using System.Text.Json.Serialization;

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
            return "Event";
        }
    }
}
