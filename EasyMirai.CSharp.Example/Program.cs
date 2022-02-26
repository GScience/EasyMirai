using EasyMirai.CSharp.Adapter;
using EasyMirai.CSharp.Message;

WsAdapter test = new();
var response = test.SendFriendMessageAsync(123, new List<IMessage>()).Result;
return;