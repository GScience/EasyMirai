﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ProtocolGenerator;
using System;
using System.IO;
using System.Reflection;

namespace SourceGenerator.CSharp.CodeAnalysis
{
    [Generator]
    public class EasyMiraiSourceGenerator : Microsoft.CodeAnalysis.ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var protocol = new MiraiProtocol();
            var module = new MiraiModule(protocol);
            var source = new MiraiSource(module, "EasyMirai");

            foreach (var src in source.SourceCodeDict)
                context.AddSource(src.Key, src.Value);
        }

        public void Initialize(GeneratorInitializationContext context)
        {

        }
    }
}
