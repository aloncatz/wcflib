using System;
using System.Collections.Concurrent;
using Bond;
using Bond.IO.Unsafe;
using Bond.Protocols;

namespace WcfLib.Serialization
{
    public class CachingBondSerializer
    {
        private readonly ConcurrentDictionary<Type, Serializer<CompactBinaryWriter<OutputBuffer>>> serializers =
            new ConcurrentDictionary<Type, Serializer<CompactBinaryWriter<OutputBuffer>>>();

        private readonly ConcurrentDictionary<Type, Deserializer<CompactBinaryReader<InputBuffer>>> deserializers =
            new ConcurrentDictionary<Type, Deserializer<CompactBinaryReader<InputBuffer>>>();

        public ArraySegment<byte> Serialize<T>(T obj)
        {
            return Serialize(typeof(T), obj);
        }

        public T Deserialize<T>(byte[] data)
        {
            return (T)Deserialize(typeof(T), data);
        }

        public ArraySegment<byte> Serialize(Type type, object obj)
        {
            OutputBuffer buffer = new OutputBuffer();
            CompactBinaryWriter<OutputBuffer> writer = new CompactBinaryWriter<OutputBuffer>(buffer);
            Serializer<CompactBinaryWriter<OutputBuffer>> ser = this.serializers.GetOrAdd(type, t => new Serializer<CompactBinaryWriter<OutputBuffer>>(type));
            ser.Serialize(obj, writer);
            return buffer.Data;
        }

        public long Serialize(Type type, object obj, byte[] data)
        {
            OutputBuffer buffer = new OutputBuffer(data);
            CompactBinaryWriter<OutputBuffer> writer = new CompactBinaryWriter<OutputBuffer>(buffer);
            Serializer<CompactBinaryWriter<OutputBuffer>> ser = this.serializers.GetOrAdd(type, t => new Serializer<CompactBinaryWriter<OutputBuffer>>(type));
            ser.Serialize(obj, writer);
            return buffer.Position;
        }

        public object Deserialize(Type type, byte[] data)
        {
            return Deserialize(type, new ArraySegment<byte>(data));
        }

        public object Deserialize(Type type, ArraySegment<byte> data)
        {
            InputBuffer buffer = new InputBuffer(data);
            CompactBinaryReader<InputBuffer> reader = new CompactBinaryReader<InputBuffer>(buffer);
            Deserializer<CompactBinaryReader<InputBuffer>> ser = this.deserializers.GetOrAdd(type, t => new Deserializer<CompactBinaryReader<InputBuffer>>(type));

            return ser.Deserialize(reader);
        }

    }
}