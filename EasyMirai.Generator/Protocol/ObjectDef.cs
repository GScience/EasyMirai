using EasyMirai.Generator.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EasyMirai.Generator.Protocol
{
    public enum ValDef
    {
        Boolean, Int, Long, String, File
    };

    /// <summary>
    /// 类型引用
    /// </summary>
    public struct ObjectRef
    {
        private readonly string _refName;

        public ObjectRef(string refName)
        {
            _refName = refName;
        }

        public ObjectDef TypeDef
        {
            get
            {
                if (ObjectDef.TypeTable.TryGetValue(_refName, out var type))
                    return type;
                return null;
            }
        }

        public override string ToString()
        {
            return $"ref({_refName})";
        }
    }

    /// <summary>
    /// Mirai 内置类型
    /// </summary>
    public class ObjectDef : ProtocolComponent
    {
        /// <summary>
        /// 全局类型定义，负责类型引用查找
        /// </summary>
        public static readonly Dictionary<string, ObjectDef> TypeTable 
            = new Dictionary<string, ObjectDef>();

        public string Name { get; protected set; } = "";
        public string Description { get; protected set; } = "";

        /// <summary>
        /// 值
        /// </summary>
        public Dictionary<string, (ValDef valDef, string description)> Values { get; private set; } 
            = new Dictionary<string, (ValDef valDef, string description)>();
        
        /// <summary>
        /// 对象
        /// </summary>
        public Dictionary<string, (ObjectDef objectDef, string description)> Objects { get; private set; } 
            = new Dictionary<string, (ObjectDef objectDef, string description)>();

        /// <summary>
        /// 引用
        /// </summary>
        public Dictionary<string, (ObjectRef objectRef, string description)> References { get; private set; } 
            = new Dictionary<string, (ObjectRef objectRef, string description)> ();

        /// <summary>
        /// 值列表
        /// </summary>
        public Dictionary<string, (ValDef valDef, string description)> ValueList { get; private set; }
            = new Dictionary<string, (ValDef valDef, string description)>();

        /// <summary>
        /// 对象列表
        /// </summary>
        public Dictionary<string, (ObjectDef objectDef, string description)> ObjectList { get; private set; } 
            = new Dictionary<string, (ObjectDef objectDef, string description)>();

        /// <summary>
        /// 引用列表
        /// </summary>
        public Dictionary<string, (ObjectRef objectRef, string description)> ReferenceList { get; private set; } 
            = new Dictionary<string, (ObjectRef objectRef, string description)>();

        public ObjectDef() { }

        /// <summary>
        /// 创建一个简单的对象定义
        /// </summary>
        /// <param name="name">对象名称</param>
        /// <param name="description">对象描述</param>
        /// <param name="autoRegister">是否自动注册到字典</param>
        public ObjectDef(string name, string description, bool autoRegister = false)
        {
            Name = name;
            Description = description;

            if (autoRegister)
                TypeTable[Name] = this;
        }

        /// <summary>
        /// 从Xml中加载
        /// </summary>
        /// <param name="version"></param>
        /// <param name="xml"></param>
        public override void FromXml(Version version, XmlElement xml)
        {
            if (xml == null)
                return;

            // 可能会被其他协议组件引用，只有当其为Type的时候才拥有 name desc 等信息
            if (xml.Name == "object")
            {
                Name = xml.GetAttribute("name");
                Description = xml.GetAttribute("desc");

                // 注册到全局的类型定义字典中
                TypeTable[Name] = this;
            }

            // 通过 LoadFieldDefs 加载具体的类型信息
            LoadFieldDefs(xml.ChildNodes);

            base.FromXml(version, xml);
        }

        /// <summary>
        /// 加载类型
        /// </summary>
        /// <param name="fieldList"></param>
        public void LoadFieldDefs(XmlNodeList fieldList)
        {
            foreach (XmlElement element in fieldList)
            {
                switch (element.Name)
                {
                    case "val":
                        LoadFieldVal(element);
                        break;
                    case "obj":
                        LoadFieldObj(element);
                        break;
                    case "ref":
                        LoadFieldRef(element);
                        break;
                    case "valList":
                        LoadFieldValList(element);
                        break;
                    case "objList":
                        LoadFieldObjList(element);
                        break;
                    case "refList":
                        LoadFieldRefList(element);
                        break;
                    default:
                        throw new NotImplementedException($"Unknown element {element.Name}");
                }
            }
        }

        /// <summary>
        /// 加载 val
        /// </summary>
        /// <param name="element"></param>
        private void LoadFieldVal(XmlElement element)
        {
            var valName = element.GetAttributeValue("name");
            var valType = (ValDef)Enum.Parse(typeof(ValDef), element.GetAttribute("type"));

            Values[valName] = (valType, element.GetAttributeValue("desc", true));
        }

        /// <summary>
        /// 加载 obj
        /// </summary>
        /// <param name="element"></param>
        private void LoadFieldObj(XmlElement element)
        {
            var objName = element.GetAttributeValue("name");
            var objDef = new ObjectDef();
            objDef.LoadFieldDefs(element.ChildNodes);
            Objects[objName] = (objDef, element.GetAttributeValue("desc", true));
        }

        /// <summary>
        /// 加载 ref
        /// </summary>
        /// <param name="element"></param>
        private void LoadFieldRef(XmlElement element)
        {
            var refName = element.GetAttributeValue("name");
            var objRef = element.GetAttributeValue("ref");

            References[refName] = (new ObjectRef(objRef), element.GetAttributeValue("desc", true));
        }

        /// <summary>
        /// 加载 valList
        /// </summary>
        /// <param name="element"></param>
        private void LoadFieldValList(XmlElement element)
        {
            var valListName = element.GetAttributeValue("name");
            var valListType = (ValDef)Enum.Parse(typeof(ValDef), element.GetAttribute("type"));

            ValueList[valListName] = (valListType, element.GetAttributeValue("desc", true));
        }

        /// <summary>
        /// 加载 obj
        /// </summary>
        /// <param name="element"></param>
        private void LoadFieldObjList(XmlElement element)
        {
            var objName = element.GetAttributeValue("name");
            var objDef = new ObjectDef();
            objDef.LoadFieldDefs(element.ChildNodes);
            ObjectList[objName] = (objDef, element.GetAttributeValue("desc", true));
        }

        /// <summary>
        /// 加载 refList
        /// </summary>
        /// <param name="element"></param>
        private void LoadFieldRefList(XmlElement element)
        {
            var refListName = element.GetAttributeValue("name");
            var refListRef = element.GetAttributeValue("ref");

            ReferenceList[refListName] = (new ObjectRef(refListRef), element.GetAttributeValue("desc", true));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
