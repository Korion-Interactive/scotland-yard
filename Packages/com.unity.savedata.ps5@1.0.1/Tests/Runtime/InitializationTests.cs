#if UNITY_PS5
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.SceneManagement;

namespace SaveDataTests
{
    [TestFixture, Description("Check all startup and initialization tests")]
    public partial class Tests : BaseTestFramework
    {
        //[PrebuildSetup("UnitTestsEditorSetup")]
        [UnityTest, Order(1), Description("Initialize Save data system")]
        public IEnumerator A_InitializeSaveData()
        {
            yield return new WaitUntil(IsInitialized);

            Assert.AreEqual(IsInitialized(), true);

            Debug.Log(s_InitOutput);
            Debug.Log("Save data initialization finished");

        }

    }
}
#endif

