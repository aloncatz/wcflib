using System.ServiceModel;
using System.Threading.Tasks;

namespace WcfLib.Test.Service
{
    [ServiceContract]
    public interface IMockService
    {
        [OperationContract]
        Task<int> EchoInt(int x);

        [OperationContract]
        Task<MockRootDataObject> EchoComplex(MockRootDataObject request);

        [OperationContract]
        Task Fail();
    }
}