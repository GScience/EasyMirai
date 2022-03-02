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
var session = await Session.CreateSessionAsync(config);
session.Start();


var startTime = DateTime.Now;

for (var i = 0; i < 10; ++i)
{
    var about = await session.WsAdapter.AboutAsync();
}

var endTime = DateTime.Now;
var deltaTime = endTime - startTime;

Console.WriteLine($"Do About Command 10 times in {deltaTime.TotalMilliseconds}ms");

while (true) ;

return;