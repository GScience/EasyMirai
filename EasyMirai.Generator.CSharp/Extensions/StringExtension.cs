using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp.Extensions
{
    internal static class StringExtension
    {
        /// <summary>
        /// 大驼峰
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        internal static string ToUpperCamel(this string className)
        {
            return className.Remove(1).ToUpper() + className.Substring(1);
        }

        /// <summary>
        /// 小驼峰
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        internal static string ToLowerCamel(this string className)
        {
            return className.Remove(1).ToLower() + className.Substring(1);
        }
    }
}
