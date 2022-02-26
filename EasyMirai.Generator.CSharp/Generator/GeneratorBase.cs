using EasyMirai.Generator.Module;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

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
            classDef.Name = FormatNameToUpperCamel(classDef.Name);
        }

        /// <summary>
        /// 格式化名称
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        protected static string FormatNameToUpperCamel(string className)
        {
            return className.Remove(1).ToUpper() + className.Substring(1);
        }
        protected static string FormatNameToLowerCamel(string className)
        {
            return className.Remove(1).ToLower() + className.Substring(1);
        }

        public abstract string GetClassDir(ClassDef classDef);

        public virtual void PostProcessing()
        {

        }
    }
}
