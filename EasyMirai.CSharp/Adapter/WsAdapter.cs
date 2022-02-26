using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyMirai.CSharp.Adapter
{
    public partial class WsAdapter
    {
        /// <summary>
        /// 发送请求
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="response"></param>
        partial void Send<TRequest, TResponse>(TRequest request, string cmd, TResponse response)
        {

        }
    }
}
