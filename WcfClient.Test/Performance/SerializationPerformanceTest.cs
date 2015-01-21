using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WcfLib.Client;
using WcfLib.Test.Service;

namespace WcfLib.Test.Performance
{
    [TestClass]
    public class SerializationPerformanceTest
    {
        private ServiceHost _host;
        private WcfClientFactory _wcfClientFactory;
        private const int IterationCount = 1000;
        [TestInitialize]
        public void Setup()
        {
            _host = new ServiceHost(typeof(MockService));
            _host.AddServiceEndpoint(typeof(IMockService), new NetTcpBinding(SecurityMode.None), "net.tcp://0.0.0.0:20001");
            _host.Open();

            _wcfClientFactory = new WcfClientFactory();
            _wcfClientFactory.Register(new ChannelFactory<IMockService>(new NetTcpBinding(SecurityMode.None), "net.tcp://localhost:20001"));
        }

        private MockRootDataObject CreateRequestObject(int size)
        {
            MockRootDataObject req = new MockRootDataObject();
            req.Date = DateTime.Now;
            req.Int = 10;
            req.String = "This is a test request object";
            req.Dict = new Dictionary<string, MockChildObject>();

            for (int i = 0; i < size; i++)
            {
                var child = new MockChildObject();
                child.Date1 = child.Date2 = req.Date;
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

        [TestMethod]
        public async Task DateContractSerializer(int size)
        {
            var requestObject = CreateRequestObject(size);

            await Measure(size, async () =>
            {
                var client = _wcfClientFactory.GetClient<IMockService>();
                await client.Call(s => s.EchoComplex(requestObject));
            });

        }
        
        [TestMethod]
        public async Task AllTests()
        {
            List<int> sizes = new List<int> {0, 10, 20, 40, 80};

            foreach (var size in sizes)
            {
                await DateContractSerializer(size);
            }
        }

        async Task Measure(int size, Func<Task> action, [CallerMemberName] string name = null)
        {
            //Make one warmup call
            await action();

            List<double> latencies = new List<double>(IterationCount);

            Stopwatch sw = new Stopwatch();
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
