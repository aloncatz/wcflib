using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WcfLib.Serialization
{
    public class BondSerializerContractBehaviorAttribute : Attribute, IContractBehavior
    {
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
            ReplaceSerializerOperationBehavior(contractDescription);
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint,
            DispatchRuntime dispatchRuntime)
        {
            ReplaceSerializerOperationBehavior(contractDescription);
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        private void ReplaceSerializerOperationBehavior(ContractDescription contract)
        {
            foreach (OperationDescription od in contract.Operations)
            {
                BondSerializerOperationBehaviorAttribute.ReplaceSerializerOperationBehavior(od);
            }
        }
    }
}