using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bond;
using Bond.IO.Unsafe;
using Bond.Protocols;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WcfLib.Serialization;
using WcfLib.Test.Service;

namespace WcfLib.Test.Serialization
{
    [TestClass]
    public class CachingBondSerializerTest
    {
        [TestMethod]
        public void Serialize()
        {
            var ser = new CachingBondSerializer();
            ser.Serialize(new MockChildObjectBase());
        }
    }
}
