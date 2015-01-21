using System;
using System.Collections.Generic;
using Bond;

namespace WcfLib.Test.Service
{

    [Schema]
    public class MockRootDataObject
    {
        [Id(0)]
        public string String { get; set; }
        [Id(1)]
        public int Int { get; set; }
        [Id(3)]
        public Dictionary<string, MockChildObject> Dict { get; set; }
        [Id(4)]
        public List<MockChildObject> List { get; set; }
    }

    [Schema]
    public class MockChildObject : MockChildObjectBase
    {
        [Id(0)]
        public string String2 { get; set; }
        [Id(1)]
        public int Int2 { get; set; }
        [Id(3)]
        public List<string> List { get; set; }
    }

    [Schema]
    public class MockChildObjectBase
    {
        [Id(0)]
        public string String1 { get; set; }
        [Id(1)]
        public int Int1 { get; set; }
    }
}