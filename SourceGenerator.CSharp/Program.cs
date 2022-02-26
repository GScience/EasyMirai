using ProtocolGenerator;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGenerator.CSharp
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var protocol = new MiraiProtocol();
            var module = new MiraiModule(protocol);
            var source = new MiraiSource(module, "EasyMirai");

            foreach (var src in source.SourceCodeDict)
            {
                Console.WriteLine(src.Key);
                Console.WriteLine(src.Value);
            }
        }
    }
}
