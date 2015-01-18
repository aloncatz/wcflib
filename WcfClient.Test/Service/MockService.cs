using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace WcfLib.Test.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true)]
    public class MockService : IMockService
    {
        public Task<int> Echo(int x)
        {
            return Task.FromResult(x);
        }

        public Task<MockResponse> GenerateResponse(MockRequest request)
        {
            return Task.FromResult(new MockResponse());
        }

        public Task Fail()
        {
            throw new Exception("You asked for it");
        }
    }
}