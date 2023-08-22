using System.Collections;
using System.IO;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using static Ravity.RuntimeTestUtils;

namespace Ravity
{
    public class SingletonTests
    {
        [UnitySetUp, UsedImplicitly]
        public IEnumerator Setup()
        {
            if (TestSingleton.Instance != null)
            {
                Object.Destroy(TestSingleton.Instance.gameObject);
            }

            LoadTestScene();
            yield return null;
        }

        [Test]
        public void InstanceIsSet()
        {
            TestSingleton singleton = new GameObject("singleton").AddComponent<TestSingleton>();
            Assert.That(TestSingleton.Instance, Is.EqualTo(singleton), "Singleton instance is null.");
        }

        [Test]
        public void SecondInstanceDoesNotOverrideFirst()
        {
            ExecutePreCondition(InstanceIsSet);

            TestSingleton secondSingleton = new GameObject("second singleton").AddComponent<TestSingleton>();
            Assert.That(TestSingleton.Instance, Is.Not.EqualTo(secondSingleton), "Singleton instance was set to second singleton.");
        }

        [UnityTest]
        public IEnumerator SecondInstanceIsDestroyed()
        {
            ExecutePreCondition(InstanceIsSet);

            TestSingleton secondSingleton = new GameObject("second singleton").AddComponent<TestSingleton>();
            yield return null;
            Assert.That(secondSingleton.IsUnityNull(), "Second singleton was not destroyed.");
        }

        [UnityTest]
        public IEnumerator SurvivesSceneSwitch()
        {
            ExecutePreCondition(InstanceIsSet);

            LoadTestScene();
            yield return null;

            Assert.That(TestSingleton.Instance.IsUnityNull(), Is.False, "Singleton is no longer valid after scene change.");
        }

        private static void LoadTestScene()
        {
            string testScene = Path.Combine("Assets", "RavityCore", "Tests", "PlayMode", "RavityTests.unity");
            EditorSceneManager.LoadSceneInPlayMode(testScene, new LoadSceneParameters(LoadSceneMode.Single));
        }
    }

    public class TestSingleton : Singleton<TestSingleton> { }
}
