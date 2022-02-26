using EasyMirai.CSharp.Adapter;
using EasyMirai.CSharp.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMirai.CSharp
{
    internal class Class1
    {
        public void test()
        {
            WsAdapter test = new();
            test.SendFriendMessage("123", 123, new List<IMessage>());
            return;
        }
    }
}
