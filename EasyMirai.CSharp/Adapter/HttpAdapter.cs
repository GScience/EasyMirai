using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMirai.CSharp.Adapter
{
    public partial class HttpAdapter
    {
        partial void Send<TRequest, TResponse>(TRequest request, string cmd, string method, string contentType, TResponse response)
        {

        }
    }
}
