﻿using PostalConventions.NET;
using PostalRX.NET;
using System;
using System.Reactive.Linq;
using System.Threading;
using PostalWhen.NET;

namespace Postal.NET.Test
{
    class Program
    {
        static void TestMultipleSubscriptions()
        {
            using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine("Got: " + env.Data)))
            using (Postal.Box.Subscribe("channel2", "topic2", (env) => Console.WriteLine("Didn't got: " + env.Data)))
            {
                Postal.Box.Publish("channel", "topic", "Hello, World!");
            }
       }

        static void TestDisposition()
        {
            using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
            {
                Postal.Box.Publish("channel", "topic", "Hello, World!");
            }

            Postal.Box.Publish("channel", "topic", "Does not appear!");
        }

        static void TestCatchAll()
        {
            using (Postal.Box.Subscribe("*", "*", (env) => Console.WriteLine("Catch all!")))
            {
                using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
                {
                    Postal.Box.Publish("channel", "topic", "Hello, World!");
                }
            }
        }

        static void TestCatchSome()
        {
            using (Postal.Box.Subscribe("c*", "t*", (env) => Console.WriteLine("Catch some!")))
            {
                using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
                {
                    Postal.Box.Publish("channel", "topic", "Hello, World!");
                }
            }
        }

        static void TestFilter()
        {
            using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data), (env) => env.Data is int))
            {
                Postal.Box.Publish("channel", "topic", "Does not show!");
                Postal.Box.Publish("channel", "topic", 12345);
            }
        }

        static void TestAsync()
        {
            using (var evt = new ManualResetEvent(false))
            using (Postal.Box.Subscribe("channel", "topic", (env) => {
                Console.WriteLine(env.Data);
                evt.Set();
            }))
            {
                Postal.Box.PublishAsync("channel", "topic", "Hello, World!");

                evt.WaitOne();
            }
        }

        static void TestFluent()
        {
            using (Postal.Box.AnyChannelAndTopic().Subscribe((env) => Console.WriteLine("Catch all!")))
            {
                using (Postal.Box.Channel("channel").Topic("topic").Subscribe((env) => Console.WriteLine(env.Data)))
                {
                    Postal.Box.Channel("channel").Topic("topic").Publish("Hello, World!");
                }
            }
        }

        static void TestExtensions()
        {
            using (Postal.Box.Subscribe<string>("channel", "topic", data => Console.WriteLine(data)))
            {
                Postal.Box.Publish("channel", "topic", "Hello, World!");
            }
        }

        static void TestReactive()
        {
            var observable = Postal.Box.Observe("channel", "topic");

            using (observable as IDisposable)
            {
                using (observable.Subscribe((env) => Console.WriteLine(env.Data), (ex) => { }, () => {}))
                {
                    Postal.Box.Publish("channel", "topic", "Hello, World!");
                }
            }
        }

        static void TestReactiveBuffered()
        {
            var observable = Postal.Box.Observe("channel", "topic");

            using (observable as IDisposable)
            {
                using (observable.Buffer(5).Subscribe((env) => Console.WriteLine("Got: " + env.Count), (ex) => { }, () => { }))
                {
                    Postal.Box.Publish("channel", "topic", "Hello, World 1!");
                    Postal.Box.Publish("channel", "topic", "Hello, World 2!");
                    Postal.Box.Publish("channel", "topic", "Hello, World 3!");
                    Postal.Box.Publish("channel", "topic", "Hello, World 4!");
                    Postal.Box.Publish("channel", "topic", "Hello, World 5!");
                }
            }
        }

        static void TestConventions()
        {
            var conventionsBox = Postal
                .Box
                .WithConventions()
                .AddChannelConvention<string>(x => x)
                .AddTopicConvention<string>(x => x);

            using (conventionsBox.Subscribe<string>(x => Console.WriteLine(x)))
            {
                conventionsBox.Publish<string>("Hello, World!");
            }
        }

        static void TestComposition()
        {
            using (Postal
                .Box
                .When("channel1", "topic1")
                .And("channel2", "topic2")
                .Subscribe(env => Console.WriteLine(env.Data)))
            {
                Postal.Box.Publish("channel1", "topic1", "Will not show");
                Postal.Box.Publish("channel2", "topic2", "Hello, World!");
            }
        }

        static void TestTimedComposition()
        {
            using (Postal
                .Box
                .When("channel1", "topic1")
                .And("channel2", "topic2")
                .InTime(TimeSpan.FromSeconds(5))
                .Subscribe(env => Console.WriteLine(env.Data)))
            {
                Postal.Box.Publish("channel1", "topic1", "Will not show");

                Thread.Sleep(6 * 1000);

                Postal.Box.Publish("channel2", "topic2", "Will not show too");
            }
        }

        static void TestInterruptedComposition()
        {
            using (Postal
                .Box
                .When("channel1", "topic1")
                .And("channel2", "topic2")
                .Subscribe(env => Console.WriteLine(env.Data)))
            {
                Postal.Box.Publish("channel1", "topic1", "Will not show");
                Postal.Box.Publish("channel3", "topic3", "Will not show");
                Postal.Box.Publish("channel2", "topic2", "Will not show");
            }
        }

        static void TestConditionalComposition()
        {
            using (Postal
                .Box
                .When("channel1", "topic1", env => env.Data is int)
                .And("channel2", "topic2")
                .Subscribe(env => Console.WriteLine(env.Data)))
            {
                Postal.Box.Publish("channel1", "topic1", 1);
                Postal.Box.Publish("channel2", "topic2", "Hello, World!");
            }
        }

        static void Main(string[] args)
        {
            //These are not unit tests, just samples of how to use Postal.NET
            TestConditionalComposition();
            TestInterruptedComposition();
            TestTimedComposition();
            TestComposition();
            TestMultipleSubscriptions();
            TestExtensions();
            TestConventions();
            TestFluent();
            TestReactive();
            TestReactiveBuffered();
            TestAsync();
            TestFilter();
            TestDisposition();
            TestCatchAll();
            TestCatchSome();

            Console.ReadLine();
        }
    }
}
