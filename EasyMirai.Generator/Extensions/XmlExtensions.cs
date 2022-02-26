using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EasyMirai.Generator.Extensions
{
    internal static class XmlExtensions
    {
        public static string GetAttributeValue(this XmlElement element, string attr, bool canBeNull = false)
        {
            var valNameAttr = element.Attributes[attr];
            if (valNameAttr == null)
            {
                if (canBeNull)
                    return "";
                throw new Exception("Name cannot be null");
            }
            return valNameAttr.Value;
        }
    }
}
