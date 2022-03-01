using EasyMirai.CSharp.Adapter;
using EasyMirai.CSharp.Example;
using EasyMirai.CSharp.Message;
using EasyMirai.CSharp.Util;
using EasyMirai.CSharp.Event;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Text;
using System.Text.Json.Serialization.Metadata;

WsAdapter test = new();
var response = test.SendFriendMessageAsync(123, new List<IMessage>()).Result;

JsonTest.TestMain();

Utf8JsonReader reader = new Utf8JsonReader();

return;