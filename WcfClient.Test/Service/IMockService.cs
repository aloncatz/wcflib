using System.ServiceModel;
using System.Threading.Tasks;
using WcfLib.Serialization;

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
        [BondSerializerOperationBehavior]
        Task<MockRootDataObject> EchoComplexBond(MockRootDataObject request);
        
        [OperationContract]
        Task Fail();
    }
}