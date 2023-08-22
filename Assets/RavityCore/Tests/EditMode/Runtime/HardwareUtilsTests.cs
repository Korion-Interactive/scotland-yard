using NUnit.Framework;
using Ravity;
using UnityEngine;

namespace Ravity
{
    public class HardwareUtilsTests
    {
        [Test]
        public void TestPlatforms()
        {
            #if UNITY_EDITOR 
                Assert.That(HardwareUtils.IsEditor, Is.True);
                Assert.That(HardwareUtils.IsDesktop, Is.True);
            #else
                Assert.That(HardwareUtils.IsEditor, Is.False);
            #endif
            
            #if UNITY_IOS
                Assert.That(HardwareUtils.IsiOS, Is.True);
            #else
                Assert.That(HardwareUtils.IsiOS, Is.False);
            #endif
            
            #if UNITY_ANDROID
                Assert.That(HardwareUtils.IsAndroid, Is.True);
            #else
                Assert.That(HardwareUtils.IsAndroid, Is.False);
            #endif
            
            #if UNITY_WEBGL
                Assert.That(HardwareUtils.IsDesktop, Is.True);
                Assert.That(HardwareUtils.IsWebGL, Is.True);
            #else
                Assert.That(HardwareUtils.IsWebGL, Is.False);
            #endif
        }
    }
}
