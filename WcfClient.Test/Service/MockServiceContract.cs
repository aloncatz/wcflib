using System;
using System.Collections.Generic;

namespace WcfLib.Test.Service
{
    public class MockRootDataObject
    {
        public string String { get; set; }
        public int Int { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, MockChildObject> Dict { get; set; }
        public List<MockChildObject> List { get; set; }
    }

    public class MockChildObject : MockChildObjectBase
    {
        public string String2 { get; set; }
        public int Int2 { get; set; }
        public DateTime Date2 { get; set; }
        public List<string> List { get; set; }
    }

    public class MockChildObjectBase
    {
        public string String1 { get; set; }
        public int Int1 { get; set; }
        public DateTime Date1 { get; set; }
    }
}