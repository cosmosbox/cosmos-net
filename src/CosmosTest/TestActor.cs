﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Actor;
using Cosmos.Framework.Components;
using Cosmos.Rpc;
using Cosmos.Utils;
using NUnit.Framework;

namespace CosmosTest
{
    public class SampleRpcCaller : IActorService
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

    public class SampleHandler : IHandler
    {
        public string Test()
        {
            return "TestString";
        }
    }

    public class SampleActor : FrontendActor
    {
        public override IActorService NewRpcCaller()
        {
            return new SampleRpcCaller();
        }

        public override IHandler GetHandler()
        {
            return new SampleHandler();
        }
    }


    [TestFixture]
    class TestActor
    {
        private ActorRunner _actorA;
        private ActorRunner _actorB;

        public TestActor()
        {
            // A server
            var discoverServers = new string[] {"http://127.0.0.1:4001"};
            var actorConf = new ActorNodeConfig()
            {
                Name = "Actor-Test-A",
                DiscoveryParam = discoverServers,
                ActorClass = "CosmosTest.SampleActor, Cosmos.Test",
            };
            _actorA = ActorRunner.Run(actorConf);
            Assert.AreEqual(_actorA.State, ActorRunState.Running);

            // B Server
            var actorConfB = new ActorNodeConfig()
            {
                Name = "Actor-Test-B",
                DiscoveryParam = discoverServers,
                ActorClass = "CosmosTest.SampleActor, Cosmos.Test",
            };
            _actorB = ActorRunner.Run(actorConfB);
            Assert.AreEqual(_actorB.State, ActorRunState.Running);
        }


        /// <summary>
        /// 创建一个Actor，并且使用客户端联系之
        /// </summary>
        //[Test]
        //public void TestFrontendActor()
        //{
        //    var co = Coroutine<object>.Start(CoTestFrontendActor());
        //    while (!co.IsFinished)
        //    {
        //        Thread.Sleep(1);
        //    }
        //}

        //IEnumerator<object> CoTestFrontendActor()
        //{

        //    var actorConf = new ActorNodeConfig
        //    {
        //        AppToken = "TestApp",
        //        Name = "Actor-Test-1",
        //        ActorClass = "CosmosTest.SampleActor, CosmosTest",
        //        Host = "*",
        //        RpcPort = 12300,

        //        DiscoveryMode = "Json",
        //        DiscoveryParam = "config/actors.json",


        //        ResponsePort = 12311,

        //    };
        //    ActorRunner.Run(actorConf);

        //    // Handler
        //    var client = new HandlerClient("127.0.0.1", 12311);
        //    var resulter = new CoroutineResult<string>();
        //    var result = Coroutine2.Start(client.Call<string>(resulter, "Test"));
        //    while (!result.IsFinished)
        //    {
        //        yield return null;
        //    }
        //    Assert.AreEqual(resulter.Result, "TestString");
        //    Assert.Pass();
        //}

        [Test]
        public void TestTypeStringName()
        {
            var typeName = typeof (TestType).AssemblyQualifiedName;
            Assert.IsTrue(!string.IsNullOrEmpty(typeName));
            Console.Write(string.Format("TestType Name: {0}", typeName));
            Assert.IsTrue(typeName.StartsWith("CosmosTest.TestType, Cosmos.Test"));

            var getType = Type.GetType("CosmosTest.TestType, Cosmos.Test");
            Assert.AreEqual(typeof(TestType), getType);
            var getType2 = Type.GetType("CosmosTest.TestType");
            Assert.AreEqual(typeof(TestType), getType2);
        }

        [Test]
        public void CreateActorByCode()
        {
            var co = Coroutine<object>.Start(WaitRunner());
            while (!co.IsFinished)
            {
                Thread.Sleep(1);
            }

            Assert.AreEqual(1, 1);
        }

        IEnumerator<object> WaitRunner()
        {
            var actorConf = new ActorNodeConfig
            {
                Name = "Actor-Test-1",
                ActorClass = "CosmosTest.SampleActor, CosmosTest",
            };
            var runner1 = ActorRunner.Run(actorConf);
            Assert.AreEqual(runner1.SecondsTick, 0);

            while (runner1.State != ActorRunState.Running)
                yield return null;

            Assert.AreEqual(runner1.State, ActorRunState.Running);
            var runner2 = ActorRunner.GetActorStateByName("Actor-Test-1");
            Assert.AreEqual(runner2.State, ActorRunState.Running);
            Assert.AreEqual(runner2.ActorName, "Actor-Test-1");
        }
    }

    public class TestType
    {

    }

}
