using Microsoft.CodeAnalysis;
using System;

namespace SourceGenerator.CSharp
{
    [Generator]
    public class EasyMiraiSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
           string source = $@" // Auto-generated code
using System;

namespace EasyMirai
{{
    public static class Hello
    {{
        public static void HelloWorld(string name) =>
            Console.WriteLine($""Generator says: His from '{{name}}'"");
    }}
}}
";
            context.AddSource($"Hello.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
            Console.WriteLine("Generate mirai code...");
        }
    }
}
