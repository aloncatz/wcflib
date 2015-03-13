using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WcfLib.Serialization
{
    public class BondSerializerOperationBehaviorAttribute : Attribute, IOperationBehavior
    {
        public void Validate(OperationDescription operationDescription)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            ReplaceSerializerOperationBehavior(operationDescription);
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            ReplaceSerializerOperationBehavior(operationDescription);
        }

        public void AddBindingParameters(OperationDescription operationDescription,
            BindingParameterCollection bindingParameters)
        {
        }

        internal static void ReplaceSerializerOperationBehavior(OperationDescription od)
        {
            var dcsob = od.Behaviors.Find<DataContractSerializerOperationBehavior>();
            if (dcsob == null)
                throw new Exception("Couldn't find existing DataContractSerializerOperationBehavior");

            int index = od.Behaviors.IndexOf(dcsob);
            od.Behaviors[index] = new BondSerializerOperationBehavior(od);
        }
    }
}