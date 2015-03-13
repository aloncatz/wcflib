using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;

namespace WcfLib.Serialization
{
    public class BondSerializerOperationBehavior : DataContractSerializerOperationBehavior
    {
        public BondSerializerOperationBehavior(OperationDescription operation) : base(operation)
        {
        }

        public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
        {
            return new WcfBondSerializer(type);
        }

        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns,
            IList<Type> knownTypes)
        {
            return new WcfBondSerializer(type);
        }
    }


}