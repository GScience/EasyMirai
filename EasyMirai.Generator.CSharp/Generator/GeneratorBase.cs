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

        /// <summary>
        /// 生成代码通用头部
        /// </summary>
        /// <returns></returns>
        protected string GenerateSourceHead()
        {
            return $@" // Auto-generated code
 // Generate at {DateTime.Now}";
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// 从类型定义中生成代码
        /// </summary>
        /// <param name="classDef"></param>
        /// <param name="namespaceDef"></param>
        /// <returns></returns>
        public virtual string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            return GenerateSourceHead();
        }

        /// <summary>
        /// 预处理，处理类型
        /// </summary>
        /// <param name="classDef"></param>
        public virtual void PreProcessing(ClassDef classDef)
        {
            classDef.Name = classDef.Name.ToUpperCamel();
        }

        /// <summary>
        /// 获取生成类型目录
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        public virtual string GetClassDir(ClassDef classDef)
        {
            return "";
        }

        /// <summary>
        /// 后处理
        /// </summary>
        /// <param name="source"></param>
        public virtual void PostProcessing(Dictionary<string, string> source)
        {

        }
    }
}
