﻿using System.ServiceModel;
using System.Threading.Tasks;

namespace Reeb.Wcf.Test.Service
{
    [ServiceContract]
    public interface IMockService
    {
        [OperationContract]
        Task<int> Echo(int x);

        [OperationContract]
        Task<MockResponse> GenerateResponse(MockRequest request);

        [OperationContract]
        Task Fail();
    }
}