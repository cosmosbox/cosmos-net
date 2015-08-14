using System;
using System.Threading.Tasks;
using Cosmos.Framework;
using ExampleProjectLib.clients;

namespace ExampleProject
{
    public class ExampleServerApp : AppDirector
    {
        public static ExampleServerApp Instance = new ExampleServerApp();  // ����

        private ExampleServerApp()
        {
            var clientScript = new ExampleClientScript();
        }
    }
}

