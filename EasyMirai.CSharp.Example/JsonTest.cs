using EasyMirai.CSharp.Message;
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
            /*groupMessage.MessageChain = new List<IMessage>()
            {
                new Source(){ Id=123 },
                new Plain(){ Msg = "456"},
            };*/
            //testJson = JsonSerializer.Serialize(groupMessage, MiraiJsonConverters.DefaultOptions);
            //groupMessage = JsonSerializer.Deserialize<GroupMessage>(testJson, MiraiJsonConverters.DefaultOptions);

            using var memoryStream = new MemoryStream();
            var writer = new Utf8JsonWriter(memoryStream);

            MiraiJsonSerializers.GroupMessageConverter.Write(writer, groupMessage);
            writer.Flush();
            testJson = Encoding.UTF8.GetString(memoryStream.ToArray());
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

                GC.Collect();
                GC.WaitForPendingFinalizers();
                results[3].Add(Test3());
                results[4].Add(Test4());
            }

            for (var i = 1; i <= 4; ++i)
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
                var jsonReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(testJson));
                jsonReader.Read();
                var obj = MiraiJsonSerializers.GroupMessageConverter.Read(ref jsonReader);
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

            using var memoryStream = new MemoryStream();
            for (var i = 0; i < 100000; ++i)
            {
                var writer = new Utf8JsonWriter(memoryStream);

                MiraiJsonSerializers.GroupMessageConverter.Write(writer, groupMessage);
                memoryStream.Seek(0, SeekOrigin.Begin);
            }
            var endTime = DateTime.Now;
            var deltaTime = endTime - startTime;
            var ms = deltaTime.TotalMilliseconds;

            Console.WriteLine($"Default Serialize time {ms}ms");
            return ms;
        }

        static double Test3()
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

        static double Test4()
        {
            Console.WriteLine($"Running Default Serialize");

            var startTime = DateTime.Now;

            using var memoryStream = new MemoryStream();
            for (var i = 0; i < 100000; ++i)
            {
                var writer = new Utf8JsonWriter(memoryStream);
                JsonSerializer.Serialize(writer, groupMessage);
                memoryStream.Seek(0, SeekOrigin.Begin);
            }
            var endTime = DateTime.Now;
            var deltaTime = endTime - startTime;
            var ms = deltaTime.TotalMilliseconds;

            Console.WriteLine($"Default Serialize time {ms}ms");
            return ms;
        }
    }
}
