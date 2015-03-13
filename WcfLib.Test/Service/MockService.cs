using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace WcfLib.Test.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true)]
    public class MockService : IMockService
    {
        public Task<int> EchoInt(int x)
        {
            return Task.FromResult(x);
        }

        public Task<int> Add(int x, int y)
        {
            return Task.FromResult(x + y);
        }

        public Task<MockRootDataObject> EchoComplex(MockRootDataObject request)
        {
            return Task.FromResult(request);

        }

        public Task<MockRootDataObject> EchoComplexBond(MockRootDataObject request)
        {
            return Task.FromResult(request);
        }

        public Task<byte[]> EchoBytes(byte[] bytes)
        {
            return Task.FromResult(bytes);
        }

        public Task Fail()
        {
            throw new ApplicationException("You asked for it");
        }
    }
}