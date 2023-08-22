#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Ravity.Editor
{
    public static class ScreenshotUtils
    {
        [MenuItem("Ravity/Take Screenshot")]
        public static void TakeScreenshot()
        {
            DirectoryInfo projectDir = new DirectoryInfo(".");
            Assert.IsTrue(projectDir.Exists);
            Assert.IsTrue(projectDir.Parent != null);
            DirectoryInfo generalScreenshotsDir = projectDir.Parent.CreateSubdirectory("Screenshots");
            DirectoryInfo screenshotsDir = generalScreenshotsDir.CreateSubdirectory(Application.productName);
            string screenshotName = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".png";
            string path = Path.Combine(screenshotsDir.FullName, screenshotName);
            ScreenCapture.CaptureScreenshot(path);
        }
    }
}
#endif
