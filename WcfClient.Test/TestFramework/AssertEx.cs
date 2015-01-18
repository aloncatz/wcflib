using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WcfLib.Test.TestFramework
{
    public static class AssertEx
    {
        public static async Task Throws<TException>(Func<Task> func) where TException : Exception
        {
            try
            {
                await func();
                Assert.Fail("There was no exception. Expected exception of type {0}", typeof (TException).Name);
            }
            catch (TException)
            {
            }
            catch (Exception ex)
            {
                Assert.Fail("Received exception of type {0}. Expected exception of type {1}", ex.GetType().Name,
                    typeof (TException).Name);
            }
        }
    }
}