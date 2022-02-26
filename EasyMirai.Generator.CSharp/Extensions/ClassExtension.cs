using EasyMirai.Generator.CSharp.Generator;
using EasyMirai.Generator.Module;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp.Extensions
{
    internal static class ClassExtension
    {
        /// <summary>
        /// 展开参数
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        public static string ExpandArgs(this ClassDef classDef)
        {
            var members = classDef.Members.Values;
            return string.Join(", ", members.Select(memberDef => ObjectGenerator.GetMemberSource(memberDef, true)));
        }

        /// <summary>
        /// 展开构造
        /// </summary>
        /// <param name="classDef"></param>
        /// <returns></returns>
        public static string ExpandCtor(this ClassDef classDef, int depth = 0)
        {
            var members = classDef.Members.Values;
            var newLine = Environment.NewLine + new string('\t', depth);
            return 
                string.Join(
                    $",{newLine}", 
                    members.Select(memberDef => 
                        $"{GeneratorBase.FormatNameToUpperCamel(memberDef.Name)} = {GeneratorBase.FormatNameToLowerCamel(memberDef.Name)}"
                    ));
        }
    }
}
