using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reeb.Wcf.Test.Service;
using Reeb.Wcf.Test.TestFramework;

namespace Reeb.Wcf.Test
{
    /// <summary>
    ///     Test the WCF client with an actual WCF service in different scenarios
    /// </summary>
    [TestClass]
    public class WcfClientIntegrationTest
    {
        private ChannelFactory<IMockService> _channelFactory;
        private ServiceHost _host;
        private Mock<WcfChannelPool<IMockService>> _wcfChannelPoolMock;
        private WcfClient<IMockService> _wcfClient;

        [TestInitialize]
        public void Setup()
        {
            StartWcfService();

            var clientBinding = new NetTcpBinding(SecurityMode.None);
            _channelFactory = new ChannelFactory<IMockService>(clientBinding, "net.tcp://localhost:20001");

            _wcfChannelPoolMock = new Mock<WcfChannelPool<IMockService>>(_channelFactory);
            _wcfChannelPoolMock.CallBase = true;
            _wcfClient = new WcfClient<IMockService>(_wcfChannelPoolMock.Object);
        }

        private void StartWcfService()
        {
            _host = new ServiceHost(typeof (MockService));
            var serverBinding = new NetTcpBinding(SecurityMode.None);
            _host.AddServiceEndpoint(typeof (IMockService), serverBinding, "net.tcp://0.0.0.0:20001");
            _host.Open();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _host.Close();
        }

        [TestMethod]
        public async Task SingleCall()
        {
            int r = await _wcfClient.Call(s => s.Echo(42));

            Assert.AreEqual(42, r);
            _wcfChannelPoolMock.Verify(m => m.GetChannel(), Times.Exactly(1));
            _wcfChannelPoolMock.Verify(m => m.ReleaseChannel(MoqExtensions.AnyObject<IClientChannel>()),
                Times.Exactly(1));
        }

        [TestMethod]
        public async Task SequentialSeriesOfCalls()
        {
            const int count = 100;
            for (int i = 0; i < count; i++)
            {
                int req = i;
                int rep = await _wcfClient.Call(s => s.Echo(req));
                Assert.AreEqual(i, rep);
            }

            _wcfChannelPoolMock.Verify(m => m.GetChannel(), Times.Exactly(count));
            _wcfChannelPoolMock.Verify(m => m.ReleaseChannel(MoqExtensions.AnyObject<IClientChannel>()),
                Times.Exactly(count));
        }

        [TestMethod]
        public async Task UserCodeFailureDoesntRecycleChannel()
        {
            //Make a normal call,
            //Make a call which generates a user code exception
            //Make another normal call 
            await _wcfClient.Call(s => s.Echo(1));
            await AssertEx.Throws<Exception>(() => _wcfClient.Call(s => s.Fail()));
            await _wcfClient.Call(s => s.Echo(2));

            //Verify that the channel was released to the pool 3 times
            //This means that WcfClient didn't recycle the channel after user code failure
            _wcfChannelPoolMock.Verify(m => m.GetChannel(), Times.Exactly(3));
            _wcfChannelPoolMock.Verify(m => m.ReleaseChannel(MoqExtensions.AnyObject<IClientChannel>()),
                Times.Exactly(3));
        }

        [TestMethod]
        public async Task CommunicationFailureRecyclesChannel()
        {
            //Make a call,
            //Restart the WCF service to break the existing channel
            //Make a call, expect it to fail and recycle the channel
            //Make a call, expect it to succeed
            await _wcfClient.Call(s => s.Echo(1));
            _host.Close();
            StartWcfService();
            await AssertEx.Throws<CommunicationException>(() => _wcfClient.Call(s => s.Echo(2)));
            await _wcfClient.Call(s => s.Echo(3));

            //Verify that the channel was released to the pool only twice
            _wcfChannelPoolMock.Verify(m => m.GetChannel(), Times.Exactly(3));
            _wcfChannelPoolMock.Verify(m => m.ReleaseChannel(MoqExtensions.AnyObject<IClientChannel>()),
                Times.Exactly(2));
        }

        [TestMethod]
        public async Task OpenBadAddress()
        {
            var clientBinding = new NetTcpBinding(SecurityMode.None);
            var channelFactory = new ChannelFactory<IMockService>(clientBinding, "net.tcp://localhost:20002");
            var wcfClient = new WcfClient<IMockService>(channelFactory);
            await AssertEx.Throws<EndpointNotFoundException>(() => wcfClient.Call(s => s.Echo(42)));
        }

        [TestMethod]
        public void MultithreadedStressTest()
        {
            const int count = 10000;
            Parallel.For(0, count, async index =>
            {
                var rep = await _wcfClient.Call(s => s.Echo(index));
                Assert.AreEqual(index, rep);
            });
            Console.WriteLine("Pool size: " + _wcfChannelPoolMock.Object.PoolSize);
        }
    }
}