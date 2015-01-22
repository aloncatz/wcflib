using System;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Xml;

namespace WcfLib.Serialization
{
    public class WcfBondSerializer : XmlObjectSerializer
    {
        private const int MB = 1024*1024;
        private static readonly CachingBondSerializer BondSerializer = new CachingBondSerializer();
        private static BufferManager _bufferManager = BufferManager.CreateBufferManager(100 * MB, MB);
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
            var buffer = _bufferManager.TakeBuffer(MB);
            
            int pos = 0;
            int bytesRead;
            do
            {
                bytesRead = reader.ReadElementContentAsBase64(buffer, pos, buffer.Length - pos);
                pos += bytesRead;
            } while (bytesRead > 0);

            var obj = BondSerializer.Deserialize(this.type, new ArraySegment<byte>(buffer, 0, pos));
            _bufferManager.ReturnBuffer(buffer);
            return obj;
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            writer.WriteEndElement();
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            var buffer = _bufferManager.TakeBuffer(MB);
            int size = (int)BondSerializer.Serialize(this.type, graph, buffer);
            writer.WriteBase64(buffer, 0, size);
            _bufferManager.ReturnBuffer(buffer);
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            writer.WriteStartElement(LocalName);
        }
    }
}