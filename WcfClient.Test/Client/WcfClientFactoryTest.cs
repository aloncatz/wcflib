using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WcfLib.Client;
using WcfLib.Test.Service;

namespace WcfLib.Test.Client
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

            var clientDefault1 = _factory.GetClient<IMockService>();
            var clientA1 = _factory.GetClient<IMockService>("A");
            var clientB1 = _factory.GetClient<IMockService>("B");


            Assert.AreEqual("net.tcp://localhost:20001/", clientDefault1.ChannelFactory.Endpoint.Address.ToString());
            Assert.AreEqual("net.tcp://localhost:20002/", clientA1.ChannelFactory.Endpoint.Address.Uri.ToString());
            Assert.AreEqual("net.tcp://localhost:20003/", clientB1.ChannelFactory.Endpoint.Address.Uri.ToString());
        }

    }
}
