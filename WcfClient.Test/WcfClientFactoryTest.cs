using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reeb.Wcf.Test.Service;

namespace Reeb.Wcf.Test
{
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
            var client1 = _factory.GetClient<IMockService>();
            var client2 = _factory.GetClient<IMockService>();
            
            Assert.IsNotNull(client1);
            Assert.AreSame(client1, client2);
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

            var clientDefault1 = _factory.GetClient<IMockService>();
            var clientDefault2 = _factory.GetClient<IMockService>();
            var clientA1 = _factory.GetClient<IMockService>("A");
            var clientA2 = _factory.GetClient<IMockService>("A");
            var clientB1 = _factory.GetClient<IMockService>("B");
            var clientB2 = _factory.GetClient<IMockService>("B");

            Assert.AreSame(clientDefault1, clientDefault2);
            Assert.AreSame(clientA1, clientA2);
            Assert.AreSame(clientB1, clientB2);

            Assert.AreNotSame(clientDefault1, clientA1);
            Assert.AreNotSame(clientA1, clientB2);

            Assert.AreEqual("net.tcp://localhost:20001/", clientDefault1.ChannelFactory.Endpoint.Address.ToString());
            Assert.AreEqual("net.tcp://localhost:20002/", clientA1.ChannelFactory.Endpoint.Address.Uri.ToString());
            Assert.AreEqual("net.tcp://localhost:20003/", clientB1.ChannelFactory.Endpoint.Address.Uri.ToString());
        }

    }
}
