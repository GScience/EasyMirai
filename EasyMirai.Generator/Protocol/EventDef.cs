using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EasyMirai.Generator.Protocol
{
    /// <summary>
    /// Mirai 事件，继承自 ObjectDef
    /// </summary>
    public class EventDef : ObjectDef
    {
        /// <summary>
        /// 特殊处理消息对象的名称与描述
        /// </summary>
        /// <param name="version"></param>
        /// <param name="xml"></param>
        public override void FromXml(Version version, XmlElement xml)
        {
            if (xml == null)
                return;

            Name = xml.GetAttribute("name");
            Description = xml.GetAttribute("desc");

            base.FromXml(version, xml);
        }

        public override string ToString()
        {
            return "Event: " + Name;
        }
    }
}
