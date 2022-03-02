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
        public async static Task<Session> CreateSessionAsync(MiraiConfig config)
        {
            var session = new Session();
            // 创建Adapter
            foreach (var adapter in config.Adapters)
            {
                switch (adapter)
                {
                    case AdapterTypes.Ws:
                        if (session.HttpAdapter == null)
                            session.WsAdapter = await WsAdapter.CreateAsync(config, CancellationToken.None);
                        else
                            session.WsAdapter = await WsAdapter.CreateAsync(config, session.HttpAdapter.sessionKey, CancellationToken.None);
                        break;
                    case AdapterTypes.Http:
                        if (session.WsAdapter == null)
                            session.HttpAdapter = new HttpAdapter(config);
                        else
                            session.HttpAdapter = new HttpAdapter(session.WsAdapter.sessionKey);
                        break;
                }
            }
            return session;
        }

        public void Start()
        {
            WsAdapter?.Start(CancellationToken.None);
        }

        public void Dispose()
        {
            WsAdapter?.Dispose();
            HttpAdapter?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
