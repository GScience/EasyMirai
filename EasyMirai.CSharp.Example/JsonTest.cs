﻿using EasyMirai.CSharp.Message;
using EasyMirai.CSharp.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasyMirai.CSharp.Util;

namespace EasyMirai.CSharp.Example
{
    static class JsonTest
    {
        static GroupMessage groupMessage = new GroupMessage();
        static string testJson;

        public static void TestMain()
        {
            groupMessage.Sender = new Member();
            groupMessage.Sender.Group = new Group();
            groupMessage.Sender.Group.Id = 12345;

            testJson = JsonSerializer.Serialize(groupMessage, MiraiJsonConverters.DefaultOptions);
            groupMessage = JsonSerializer.Deserialize<GroupMessage>(testJson, MiraiJsonConverters.DefaultOptions);

            var results = new Dictionary<int, List<double>>()
            {
                {1, new List<double>() },
                {2, new List<double>() },
                {3, new List<double>() },
                {4, new List<double>() },
                {5, new List<double>() },
                {6, new List<double>() }
            };

            for (var i = 0; i < 15; ++i)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                results[1].Add(Test1());
                results[2].Add(Test2());
            }

            for (var i = 1; i <= 2; ++i)
            {
                var times = results[i];
                var meanTime = times.Min();
                Console.WriteLine($"Test{i}: {meanTime}ms");
            }
        }

        static double Test1()
        {
            Console.WriteLine($"Running Default Deserialize");

            var startTime = DateTime.Now;

            for (var i = 0; i < 100000; ++i)
            {
                var obj = JsonSerializer.Deserialize<GroupMessage>(testJson);
            }
            var endTime = DateTime.Now;
            var deltaTime = endTime - startTime;
            var ms = deltaTime.TotalMilliseconds;

            Console.WriteLine($"Default Serialize time {ms}ms");
            return ms;
        }

        static double Test2()
        {
            Console.WriteLine($"Running Default Serialize");

            var startTime = DateTime.Now;

            for (var i = 0; i < 100000; ++i)
            {
                var result = JsonSerializer.Serialize(groupMessage);
            }
            var endTime = DateTime.Now;
            var deltaTime = endTime - startTime;
            var ms = deltaTime.TotalMilliseconds;

            Console.WriteLine($"Default Serialize time {ms}ms");
            return ms;
        }
    }
}