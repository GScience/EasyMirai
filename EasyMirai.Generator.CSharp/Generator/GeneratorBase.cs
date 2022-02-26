using EasyMirai.Generator.Module;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using EasyMirai.Generator.CSharp.Extensions;

namespace EasyMirai.Generator.CSharp.Generator
{
    /// <summary>
    /// 基础的生成器
    /// </summary>
    internal abstract class GeneratorBase : ISourceGenerator
    {
        public static Dictionary<string, ISourceGenerator> SourceGeneratorTable { get; private set; }
            = new Dictionary<string, ISourceGenerator>();

        public virtual string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            return $@" // Auto-generated code
 // Generate at {DateTime.Now}";
        }

        public virtual void PreProcessing(ClassDef classDef)
        {
            classDef.Name = classDef.Name.ToUpperCamel();
        }

        public abstract string GetClassDir(ClassDef classDef);

        public virtual void PostProcessing()
        {

        }
    }
}
