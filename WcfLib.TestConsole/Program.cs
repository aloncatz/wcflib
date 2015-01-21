using WcfLib.Test.Performance;

namespace WcfLib.TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var test = new SerializationPerformanceTest();
            test.Setup();
            test.AllTests().Wait();
            test.Cleanup();
        }
    }
}