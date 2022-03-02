using EasyMirai.CSharp.Adapter;
using EasyMirai.CSharp.Message;
using EasyMirai.CSharp.Util;
using EasyMirai.CSharp.Event;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using EasyMirai.CSharp;

var config = MiraiConfig.FromFile("config.json");
var session = Session.CreateSession(config);
await session.Start();

var aboutWs = await session.WsAdapter.AboutAsync();
var aboutHttp = await session.HttpAdapter.AboutAsync();
var groupListWs = await session.WsAdapter.GroupListAsync();
var groupListHttp = await session.HttpAdapter.GroupListAsync();

session.WsAdapter.EventHookTable.GroupMessage = async (groupMessage) =>
{
    foreach (var msg in groupMessage.MessageChain)
        if (msg is PlainMessage plainMessage &&
            plainMessage.Text == "犊子测试机呢")
            await session.WsAdapter.SendGroupMessageAsync(groupMessage.Sender.Group.Id, new List<IMessage>() { new PlainMessage { Text = "在这呢~" } });
};

while (true)
{
    Console.ReadLine();
}
