#if UNITY_PS5 || UNITY_PS4
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace PSNTests
{
    [TestFixture, Description("Check all startup and initialization tests")]
    public class InitializationTests : BaseTests
    {

        //[PrebuildSetup("UnitTestsEditorSetup")]
        [UnityTest, Order(1), Description("Initialize PSN system")]
        public IEnumerator InitializePSN()
        {
            yield return new WaitForSeconds(0.2f);

            Debug.Log("s_InitOutput = " + TestFramework.s_InitOutput);

            Assert.AreEqual(TestFramework.IsInitialized(), true);

            Debug.Log("PSN initialization finished");
        }
    }
}
#endif
