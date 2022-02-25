using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProtocolGenerator
{
    /// <summary>
    /// 协议组件接口
    /// </summary>
    public class ProtocolComponent
    {
        /// <summary>
        /// 协议版本
        /// </summary>
        public Version? ProtocolVersion { get; private set; }

        /// <summary>
        /// 从Xml中加载协议组件
        /// </summary>
        /// <param name="version"></param>
        /// <param name="xml"></param>
        public virtual void FromXml(Version version, XmlElement? xml)
        {
            ProtocolVersion = version;
        }
    }
}
