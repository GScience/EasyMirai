using EasyMirai.Generator.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMirai.Generator.CSharp.Generator
{
    internal class EventGenerator : ObjectGenerator
    {
        public static string RootNamespace => $"{MiraiSource.RootNamespace}.Event";

        /// <summary>
        /// 事件表，消息类型与对应序列化类型
        /// </summary>
        public Dictionary<ClassDef, string> EventTable = new Dictionary<ClassDef, string>();
        public override void PreProcessing(ClassDef classDef)
        {
            base.PreProcessing(classDef);
            classDef.Namespace = RootNamespace;
        }
        public override string GenerateFrom(ClassDef classDef, string namespaceDef)
        {
            var type = classDef.ConstString["Type"].value;
            EventTable[classDef] = type;
            return base.GenerateFrom(classDef, namespaceDef + ".Event");
        }

        protected override string GenerateExtraCode(ClassDef classDef)
        {
            var type = classDef.ConstString["Type"].value;
            return $@"
        [global::System.Text.Json.Serialization.JsonPropertyName(""Type"")] public string Type => ""{type}"";";
        }

        protected override string GenerateExtraInterface(ClassDef classDef)
        {
            return SerializeGenerator.GetFullNameOf("ISerializableEvent");
        }

        public override string GetClassDir(ClassDef classDef)
        {
            return "Event";
        }
    }
}
