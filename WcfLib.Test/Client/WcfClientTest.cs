using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WcfLib.Client;
using WcfLib.Test.Service;
using WcfLib.Test.TestFramework;

namespace WcfLib.Test.Client
{
    [TestClass]
    public class WcfClientTest
    {
        private Mock<IMockService> _channelMock;
        private Mock<IWcfChannelPool> _channelPoolMock;
        private WcfClient<IMockService> _client;
        private Mock<RetryPolicy> _retryPolicyMock;

        [TestInitialize]
        public void Setup()
        {
            _channelMock = new Mock<IClientChannel>().As<IMockService>();
            _channelMock.Setup(m => m.EchoInt(It.IsAny<int>())).ReturnsAsync(1);
            _channelPoolMock = new Mock<IWcfChannelPool>();
            _channelPoolMock.Setup(m => m.GetChannel()).ReturnsAsync((IClientChannel) _channelMock.Object);
            _retryPolicyMock = new Mock<RetryPolicy>();
            _client = new WcfClient<IMockService>(_channelPoolMock.Object, _retryPolicyMock.Object);
        }

        [TestMethod]
        public async Task SimpleCallNoRetries()
        {
            int res = await _client.Call(s => s.EchoInt(1));
            Assert.AreEqual(1, res);
            _channelPoolMock.Verify(m => m.GetChannel(), Times.Once);
            _channelPoolMock.Verify(m => m.ReleaseChannel(It.IsAny<IClientChannel>()), Times.Once);
            _channelMock.Verify(m => m.EchoInt(1), Times.Once);
        }

        [TestMethod]
        public async Task FailedCallNoRetry()
        {
            _channelMock.Setup(m => m.EchoInt(It.IsAny<int>())).ThrowsAsync(new ApplicationException("Fail me"));

            await AssertEx.Throws<ApplicationException>(async () => await _client.Call(s => s.EchoInt(1)));

            _channelPoolMock.Verify(m => m.GetChannel(), Times.Once);
            _channelPoolMock.Verify(m => m.ReleaseChannel(It.IsAny<IClientChannel>()), Times.Never);
        }

        [TestMethod]
        public async Task FailFirstSucceedOnRetry()
        {
            _retryPolicyMock.Object.MaxRetryCount = 2;

            int count = 0;
            _channelMock.Setup(m => m.EchoInt(It.IsAny<int>())).Callback<int>(x => count++).Returns(() =>
            {
                if (count == 1)
                    throw new ApplicationException("First attempt failed");
                return Task.FromResult(1);
            });

            int res = await _client.Call(s => s.EchoInt(1));
            Assert.AreEqual(1, res);
            _channelPoolMock.Verify(m => m.GetChannel(), Times.Exactly(2));
            _channelPoolMock.Verify(m => m.ReleaseChannel(It.IsAny<IClientChannel>()), Times.Once);
            _channelMock.Verify(m => m.EchoInt(1), Times.Exactly(2));
        }

        [TestMethod]
        public async Task TransientFailuresAreReported()
        {
            // Setup to fail 3 attempts
            _retryPolicyMock.Object.MaxRetryCount = 3;
            _channelMock.Setup(m => m.EchoInt(It.IsAny<int>())).Throws(new ApplicationException("Attempt failed"));

            // Collect all retry reports into a list
            var attemptNumbers = new List<int>();
            _client.TransientFailure += (sender, args) => attemptNumbers.Add(args.AttemptNumber);

            await AssertEx.Throws<ApplicationException>(async () => await _client.Call(s => s.EchoInt(1)));
            
            //Verify that 3 retry attempts were reported
            Assert.AreEqual(2, attemptNumbers.Count);
            Assert.AreEqual(0, attemptNumbers[0]);
            Assert.AreEqual(1, attemptNumbers[1]);
        }

    }
}