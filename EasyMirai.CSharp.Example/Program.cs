﻿using EasyMirai.CSharp.Adapter;
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


while (true) ;

return;