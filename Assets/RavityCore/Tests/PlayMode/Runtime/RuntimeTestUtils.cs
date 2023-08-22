using System;
using NUnit.Framework;

namespace Ravity
{
    public static class RuntimeTestUtils
    {
        public static void ExecutePreCondition(Action preCondition)
        {
            try
            {
                preCondition();
            }
            catch (Exception e)
            {
                Assert.Inconclusive($"Precondition failed: {e}");
            }
        }
    }
}
