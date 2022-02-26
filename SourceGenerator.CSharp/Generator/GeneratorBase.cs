using ProtocolGenerator.Module;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace SourceGenerator.CSharp.Generator
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

        public void PreProcessing(ClassDef classDef)
        {
            classDef.Name = FormatName(classDef.Name);
        }

        /// <summary>
        /// 格式化名称
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        protected static string FormatName(string className)
        {
            return className.Remove(1).ToUpper() + className.Substring(1);
        }

        public abstract string GetClassDir(ClassDef classDef);
    }
}
