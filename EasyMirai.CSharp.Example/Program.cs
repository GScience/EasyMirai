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

var startTime = DateTime.Now;


var aboutWs = await session.WsAdapter!.AboutAsync();
var aboutHttp = await session.HttpAdapter!.AboutAsync();
var groupListWs = await session.WsAdapter!.GroupListAsync();
var groupListHttp = await session.HttpAdapter!.GroupListAsync();

var uploadImage1 = await session.HttpAdapter.UploadImageAsync(type: "group");
var uploadImage2 = await session.HttpAdapter.UploadImageAsync(type: "group", img: File.OpenRead("Test.png"), uploadName: "test.png");

session.WsAdapter.EventHookTable.NudgeEvent = async (nudgeEvent) =>
{
    if (nudgeEvent.Subject!.Id != config.Id && nudgeEvent.Subject!.Kind == "Group")
        await session.WsAdapter.SendGroupMessageAsync(group: nudgeEvent.Subject!.Id, messageChain: new[] { new PlainMessage { Text = "戳尼玛" } });
};

session.WsAdapter.EventHookTable.GroupMessage = async (groupMessage) =>
{
    foreach (var msg in groupMessage!.MessageChain!)
        if (msg is PlainMessage plainMessage &&
            plainMessage.Text == "犊子测试机呢")
        {
            await session.WsAdapter.SendGroupMessageAsync(group: groupMessage.Sender!.Group!.Id, messageChain: new[] { new PlainMessage { Text = "在这呢" } });
        }
};

session.WsAdapter.EventHookTable.FriendMessage = async (friendMessage) =>
{
    await session.WsAdapter.SendFriendMessageAsync(qq: friendMessage.Sender!.Id, messageChain:friendMessage.MessageChain!);
};

while (true)
{
    Console.ReadLine();
}
