using EasyMirai.Generator.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EasyMirai.Generator.Protocol
{
    /// <summary>
    /// WS Adapter
    /// </summary>
    public class WsAdapterDef
    {
        public string Description { get; private set; }

        public string Command { get; private set; }

        public string SubCommand { get; private set; }

        public WsAdapterDef(string command, string subCommand, string description)
        {
            Command = command;
            SubCommand = subCommand;
            Description = description;
        }
    }

    public enum HttpAdapterMethod
    {
        Get, Post
    }

    /// <summary>
    /// Http Adapter
    /// </summary>
    public class HttpAdapterDef
    {
        public string Description { get; private set; }

        public string Command { get; private set; }
        public string ContentType { get; private set; }

        public HttpAdapterMethod Method { get; private set; }

        public HttpAdapterDef(string command, string description, string contentType, HttpAdapterMethod method)
        {
            Command = command;
            Description = description;
            if (method == HttpAdapterMethod.Get)
                ContentType = "";
            else
                ContentType = string.IsNullOrEmpty(contentType) ? "application/json" : contentType;
            Method = method;
        }
    }

    /// <summary>
    /// Mirai Api
    /// </summary>
    public class ApiDef : ProtocolComponent
    {
        /// <summary>
        /// 请求
        /// </summary>
        public ObjectDef Request { get; private set; } = new ObjectDef();

        /// <summary>
        /// 响应
        /// </summary>
        public ObjectDef Response { get; private set; } = new ObjectDef();

        /// <summary>
        /// Websock Adapter
        /// </summary>
        public WsAdapterDef WsAdapter { get; private set; }

        /// <summary>
        /// Http Adapter
        /// </summary>
        public HttpAdapterDef HttpAdapter { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; } = "";

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; private set; } = "";

        public override string ToString()
        {
            return "Api: " + Name;
        }

        /// <summary>
        /// 加载 Api
        /// </summary>
        /// <param name="version"></param>
        /// <param name="xml"></param>
        public override void FromXml(Version version, XmlElement xml)
        {
            if (xml == null)
                return;

            Name = xml.GetAttribute("name");
            Description = xml.GetAttribute("desc");

            Request.LoadFieldDefs(xml["request"].ChildNodes);
            Response.LoadFieldDefs(xml["response"].ChildNodes);

            var requestRefClassName = xml["request"].GetAttribute("ref");
            var responseRefClassName = xml["response"].GetAttribute("ref");

            if (!string.IsNullOrEmpty(requestRefClassName))
                Request.Base = new ObjectRef(requestRefClassName);

            if (!string.IsNullOrEmpty(responseRefClassName))
                Response.Base = new ObjectRef(responseRefClassName);

            foreach (XmlElement element in xml["adapter"])
            {
                switch (element.Name)
                {
                    case "http":
                        var method = (HttpAdapterMethod)Enum.Parse(typeof(HttpAdapterMethod), element.GetAttribute("method"));
                        HttpAdapter = new HttpAdapterDef(
                            element.GetAttributeValue("cmd"),
                            element.GetAttributeValue("desc", true),
                            element.GetAttributeValue("content", true),
                            method);
                        break;
                    case "ws":
                        WsAdapter = new WsAdapterDef(
                            element.GetAttributeValue("cmd"),
                            element.GetAttributeValue("subcmd", true),
                            element.GetAttributeValue("desc", true));
                        break;
                    default:
                        throw new NotImplementedException($"Unknown adapter {element.Name}");
                }
            }

            base.FromXml(version, xml);
        }
    }
}
