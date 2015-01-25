﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WcfLib.Client;
using WcfLib.Test.Service;

namespace WcfLib.Test.Performance
{
    [TestClass]
    public class TransportSecurityPerformanceTest
    {
        private ServiceHost _host;
        private WcfClientFactory _wcfClientFactory;
        private const int IterationCount = 10000;

        [TestInitialize]
        public void Setup()
        {
            _host = new ServiceHost(typeof(MockService));
            _host.AddServiceEndpoint(typeof(IMockService), CreateBinding(SecurityMode.None), "net.tcp://0.0.0.0:20001");
            _host.AddServiceEndpoint(typeof(IMockService), CreateBinding(SecurityMode.Transport), "net.tcp://0.0.0.0:20002");
            _host.Open();

            _wcfClientFactory = new WcfClientFactory();
            _wcfClientFactory.Register("NoSecurity", new ChannelFactory<IMockService>(CreateBinding(SecurityMode.None), "net.tcp://localhost:20001"));
            _wcfClientFactory.Register("TransportSecurity", new ChannelFactory<IMockService>(CreateBinding(SecurityMode.Transport), "net.tcp://localhost:20002"));
        }

        NetTcpBinding CreateBinding(SecurityMode securityMode)
        {
            return new NetTcpBinding(securityMode) {MaxReceivedMessageSize = 2147483647};
        }

        [TestCleanup]
        public void Cleanup()
        {
            _host.Close();
        }
        
        [TestMethod]
        public async Task MeasureAll()
        {
            var sizes = Enumerable.Range(0, 10).Select(i => Math.Pow(2, i) * 1024);

            foreach (int size in sizes)
            {
                await Measure("NoSecurity", size);
            }

            foreach (int size in sizes)
            {
                await Measure("TransportSecurity", size);
            }
        }

        [TestMethod]
        public async Task Measure(string channelName, int size)
        {
            var bytes = new byte[size];
            await Measure(channelName, size, async () =>
            {
                var client = _wcfClientFactory.GetClient<IMockService>(channelName);
                await client.Call(s => s.EchoBytes(bytes));
            });
        }

        async Task Measure(string name, int size, Func<Task> action)
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
            double p99 = latencies[(int)(latencies.Count * 0.99)];

            Console.WriteLine("{0}, {1}, {2:0.000}, {3:0.000}", name, size, average, p99);
        }


    }
}
