using System;
using System.Runtime.Serialization;
using System.Xml;

namespace WcfLib.Serialization
{
    public class WcfBondSerializer : XmlObjectSerializer
    {
        private static readonly CachingBondSerializer BondSerializer = new CachingBondSerializer();
        private const string LocalName = "Bond";
        private readonly Type type;

        public WcfBondSerializer(Type type)
        {
            this.type = type;
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            return reader.LocalName == LocalName;
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            byte[] bytes = reader.ReadElementContentAsBase64();
            return BondSerializer.Deserialize(this.type, bytes);
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            writer.WriteEndElement();
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            ArraySegment<byte> bytes = BondSerializer.Serialize(this.type, graph);
            writer.WriteBase64(bytes.Array, bytes.Offset, bytes.Count);
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            writer.WriteStartElement(LocalName);
        }
    }
}