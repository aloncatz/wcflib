using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WcfLib.Client;
using WcfLib.Test.Service;

namespace WcfLib.Test.Client
{
    using System;
    using TestFramework;

    [TestClass]
    public class WcfClientFactoryTest
    {
        private WcfClientFactory _factory;
        private ChannelFactory<IMockService> _channelFactory;

        [TestInitialize]
        public void Setup()
        {
            _factory = new WcfClientFactory();
            _channelFactory = new ChannelFactory<IMockService>(new NetTcpBinding(), "net.tcp://localhost:20001");
        }

        [TestMethod]
        public void RegisterDefaultAndGetClient()
        {
            _factory.Register(_channelFactory);
            var client1 = _factory.CreateClient<IMockService>();
            var client2 = _factory.CreateClient<IMockService>();
            
            Assert.IsNotNull(client1);
            Assert.IsNotNull(client2);
            Assert.AreNotSame(client1, client2);
        }

        [TestMethod]
        public void RegisterDefaultAndNamedGetClient()
        {
            var channelFactory1 = new ChannelFactory<IMockService>(new NetTcpBinding(), "net.tcp://localhost:20001");
            var channelFactory2 = new ChannelFactory<IMockService>(new NetTcpBinding(), "net.tcp://localhost:20002");
            var channelFactory3 = new ChannelFactory<IMockService>(new NetTcpBinding(), "net.tcp://localhost:20003");
            _factory.Register(channelFactory1);
            _factory.Register("A", channelFactory2);
            _factory.Register("B", channelFactory3);

            var clientDefault1 = (WcfClient<IMockService>)_factory.CreateClient<IMockService>();
            var clientA1 = (WcfClient<IMockService>)_factory.CreateClient<IMockService>("A");
            var clientB1 = (WcfClient<IMockService>)_factory.CreateClient<IMockService>("B");


            Assert.AreEqual(channelFactory1, ((WcfChannelPool<IMockService>)clientDefault1.ChannelPool).ChannelFactory);
            Assert.AreEqual(channelFactory2, ((WcfChannelPool<IMockService>)clientA1.ChannelPool).ChannelFactory);
            Assert.AreEqual(channelFactory3, ((WcfChannelPool<IMockService>)clientB1.ChannelPool).ChannelFactory);
        }

        [TestMethod]
        public void RegisterSameClientTwiceThrows()
        {
            _factory.Register(_channelFactory);

            AssertEx.Throws<Exception>(() => _factory.Register(_channelFactory));
        }

        [TestMethod]
        public void RegisterFactoryMethodTwice()
        {
            _factory.Register(() => new EnpointConfiguration<IMockService>(this._channelFactory, new NoRetryPolicy()));
            _factory.Register<IMockService>(() => { throw new Exception("We should never get here"); });

            var client1 = _factory.CreateClient<IMockService>();
            Assert.IsNotNull(client1);
        }

    }
}
