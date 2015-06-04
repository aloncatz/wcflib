using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WcfLib.Client;
using WcfLib.Test.Service;

namespace WcfLib.Test.Performance
{
    [TestClass]
    [Ignore]
    public class WcfClientPerformanceTest
    {
        private const int IterationCount = 10000;
        private ServiceHost _host;
        private WcfClientFactory _wcfClientFactory;

        [TestInitialize]
        public void Setup()
        {
            _host = new ServiceHost(typeof (MockService));
            _host.AddServiceEndpoint(typeof (IMockService), new NetTcpBinding(SecurityMode.None),
                "net.tcp://0.0.0.0:20001");
            _host.Open();

            _wcfClientFactory = new WcfClientFactory();
            _wcfClientFactory.Register(new ChannelFactory<IMockService>(new NetTcpBinding(SecurityMode.None),
                "net.tcp://localhost:20001"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _host.Close();
        }

        [TestMethod]
        [TestCategory("Build.Skip")]
        public async Task WcfClient()
        {
            await Measure(async () =>
            {
                WcfClient<IMockService> client = _wcfClientFactory.CreateClient<IMockService>();
                await client.Call(s => s.EchoInt(1));
            });
        }

        [TestMethod]
        [TestCategory("Build.Skip")]
        public async Task CachedChannelFactoryNonCachedChannel()
        {
            var clientBinding = new NetTcpBinding(SecurityMode.None);
            var channelFactory = new ChannelFactory<IMockService>(clientBinding, "net.tcp://localhost:20001");

            await Measure(async () =>
            {
                IMockService proxy = channelFactory.CreateChannel();
                var channel = (IServiceChannel) proxy;
                await proxy.EchoInt(1);
                channel.Close();
            });
        }

        [TestMethod]
        [TestCategory("Build.Skip")]
        public async Task NonCachedChannelFactoryNonCachedChannel()
        {
            await Measure(async () =>
            {
                var clientBinding = new NetTcpBinding(SecurityMode.None);
                var channelFactory = new ChannelFactory<IMockService>(clientBinding, "net.tcp://localhost:20001");
                IMockService proxy = channelFactory.CreateChannel();
                var channel = (IServiceChannel) proxy;
                await proxy.EchoInt(1);
                channel.Close();
            });
        }

        [TestMethod]
        [TestCategory("Build.Skip")]
        public async Task AllTests()
        {
            await WcfClient();
            await CachedChannelFactoryNonCachedChannel();
            await NonCachedChannelFactoryNonCachedChannel();
        }

        private async Task Measure(Func<Task> action, [CallerMemberName] string name = null)
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

            Console.WriteLine("{0}: Average: {1:0.000}, 99%: {2:0.000}", name, average, p99);
        }
    }
}