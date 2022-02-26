using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp.Generator
{
    internal class MessageGenerator : ObjectGenerator
    {
        /// <summary>
        /// 消息表，消息类型与对应序列化类型
        /// </summary>
        public Dictionary<string, string> MessageTable = new Dictionary<string, string>();

        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            return base.GenerateFrom(classDef, namespaceDef + ".Message");
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "Message";
        }


        /// <summary>
        /// 后处理负责序列化相关事项
        /// </summary>
        public override void PostProcessing()
        {

        }
    }
}
