using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMirai.CSharp.Adapter
{
    /// <summary>
    /// Adapter 异常
    /// </summary>
    public class AdapterException : Exception
    {
        public Exception Reason { get; private set; }

        public AdapterException(Exception reason, string message)
        {
            Reason = reason;
        }

        public override string ToString()
        {
            return $"Error: {Message}\nDue to: {Reason}\n{base.ToString()}";
        }
    }
}
