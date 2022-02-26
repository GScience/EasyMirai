using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp
{
    /// <summary>
    /// 源生成器接口
    /// </summary>
    public interface ISourceGenerator
    {
        /// <summary>
        /// 生成源码
        /// </summary>
        /// <param name="classDef"></param>
        /// <param name="namespaceDef"></param>
        /// <returns></returns>
        string GenerateFrom(ClassDef classDef, string namespaceDef);

        /// <summary>
        /// 对类型进行预处理
        /// </summary>
        /// <param name="classDef"></param>
        void PreProcessing(ClassDef classDef);

        /// <summary>
        /// 获取类型路径
        /// </summary>
        /// <returns></returns>
        string GetClassDir(ClassDef classDef);
    }
}
