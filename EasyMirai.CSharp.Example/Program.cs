using EasyMirai.CSharp.Adapter;
using EasyMirai.CSharp.Example;
using EasyMirai.CSharp.Message;
using System.Text.Json;

WsAdapter test = new();
var response = test.SendFriendMessageAsync(123, new List<IMessage>()).Result;

var testForward = JsonSerializer.Deserialize<SerializeTest>("{\"forward\":{\"nodeList\":[{\"senderName\":\"test\"}]},\"str\":\"456\"}");

var testForwardJson = JsonSerializer.Serialize(testForward);
return;