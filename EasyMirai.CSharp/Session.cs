using EasyMirai.CSharp.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMirai.CSharp
{
    /// <summary>
    /// Mirai Session
    /// </summary>
    public class Session : IDisposable
    {
        public WsAdapter? WsAdapter { get; private set; }
        public HttpAdapter? HttpAdapter { get; private set; }

        internal Session() { }

        /// <summary>
        /// 从配置中创建Session
        /// </summary>
        /// <param name="config"></param>
        public static Session CreateSession(MiraiConfig config)
        {
            var session = new Session();
            // 创建Adapter
            foreach (var adapter in config.Adapters)
            {
                switch (adapter)
                {
                    case AdapterTypes.Ws:
                        session.WsAdapter = new WsAdapter(config);
                        break;
                    case AdapterTypes.Http:
                        session.HttpAdapter = new HttpAdapter(config);
                        break;
                }
            }
            return session;
        }

        public async Task Start()
        {
            // 如果有WebSocket，则从WebSocket创建Session
            if (WsAdapter != null)
            {
                await WsAdapter.StartAsync(CancellationToken.None);
                if (HttpAdapter != null)
                    HttpAdapter.sessionKey = WsAdapter.sessionKey;
            }
            else if (HttpAdapter != null)
            {
                await HttpAdapter.StartAsync(CancellationToken.None);
            }
        }

        public void Dispose()
        {
            WsAdapter?.Dispose();
            HttpAdapter?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
