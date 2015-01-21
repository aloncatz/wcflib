using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WcfLib.Client;
using WcfLib.Test.Service;

namespace WcfLib.Test.Performance
{
    [TestClass]
    public class SerializationPerformanceTest
    {
        private const int IterationCount = 1000;
        private ServiceHost _host;
        private WcfClientFactory _wcfClientFactory;

        [TestInitialize]
        public void Setup()
        {
            _host = new ServiceHost(typeof (MockService));
            var serverBinding = new NetTcpBinding(SecurityMode.None);
            serverBinding.MaxReceivedMessageSize = 2147483647;
            serverBinding.MaxBufferSize = 2147483647;
            _host.AddServiceEndpoint(typeof (IMockService), serverBinding, "net.tcp://0.0.0.0:20001");
            _host.Open();

            _wcfClientFactory = new WcfClientFactory();
            _wcfClientFactory.Register(new ChannelFactory<IMockService>(new NetTcpBinding(SecurityMode.None),
                "net.tcp://localhost:20001"));
        }

        private MockRootDataObject CreateRequestObject(int size)
        {
            var req = new MockRootDataObject();
            req.Int = 10;
            req.String = "This is a test request object";
            req.Dict = new Dictionary<string, MockChildObject>();

            for (int i = 0; i < size; i++)
            {
                var child = new MockChildObject();
                child.Int1 = child.Int2 = req.Int;
                child.String1 = child.String2 = req.String;
                child.List = Enumerable.Range(1, 10).Select(j => "Some text + " + j).ToList();
                req.Dict["ItemABCDEF" + i] = child;
            }

            req.List = req.Dict.Values.ToList();
            return req;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _host.Close();
        }

        public async Task Measure(int size)
        {
            MockRootDataObject requestObject = CreateRequestObject(size);

            await Measure(size, "bond", async () =>
            {
                WcfClient<IMockService> client = _wcfClientFactory.GetClient<IMockService>();
                await client.Call(s => s.EchoComplexBond(requestObject));
            });

            await Measure(size, "dcs", async () =>
            {
                WcfClient<IMockService> client = _wcfClientFactory.GetClient<IMockService>();
                await client.Call(s => s.EchoComplex(requestObject));
            });
        }

        [TestMethod]
        public async Task AllTests()
        {
            var sizes = new List<int> {0, 10, 20, 40, 80, 160, 320};

            foreach (int size in sizes)
            {
                await Measure(size);
                Console.WriteLine();
            }
        }

        private async Task Measure(int size, string name, Func<Task> action)
        {
            //Make one warmup call
            await action();

            var latencies = new List<double>(IterationCount);

            var sw = new Stopwatch();
            for (int i = 0; i < IterationCount; i++)
            {
                sw.Restart();
                await action();
                sw.Stop();
                latencies.Add(sw.Elapsed.TotalMilliseconds);
            }

            latencies.Sort();
            double average = latencies.Average();
            double p99 = latencies[(int) (latencies.Count*0.99)];

            Console.WriteLine("{0}@{1}: Average: {2:0.000}, 99%: {3:0.000}", name, size, average, p99);
        }
    }
}