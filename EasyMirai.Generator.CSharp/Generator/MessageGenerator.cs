using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp.Generator
{
    internal class MessageGenerator : ObjectGenerator
    {
        public static string RootNamespace => $"{MiraiSource.RootNamespace}.Message";

        /// <summary>
        /// 消息表，消息类型与对应序列化类型
        /// </summary>
        public Dictionary<ClassDef, string> MessageTable = new Dictionary<ClassDef, string>();

        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            var type = classDef.ConstString["Type"].value;
            MessageTable[classDef] = type;
            return base.GenerateFrom(classDef, RootNamespace);
        }

        protected override string GenerateExtraCode(ClassDef classDef)
        {
            var type = classDef.ConstString["Type"].value;
            return $@"
        [JsonPropertyName(""Type"")] public string Type => ""{type}"";";
        }

        protected override string GenerateExtraInterface(ClassDef classDef)
        {
            return $@"MiraiJsonSerializers.ISerializableMessage";
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "Message";
        }
    }
}
