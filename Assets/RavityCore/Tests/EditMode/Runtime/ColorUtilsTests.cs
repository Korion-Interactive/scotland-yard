using NUnit.Framework;
using UnityEngine;

namespace Ravity
{
    public class ColorUtilsTests
    {
        [Test]
        public void CopyWithAlpha()
        {
            Color transparent = new Color(1, 1, 1, 0.5f);
            Color opaque = transparent.CopyWithAlpha(1.0f);
            Assert.That(transparent.a,Is.EqualTo(0.5f));
            Assert.That(opaque.a,Is.EqualTo(1.0f));
        }

        [Test]
        public void CreateColor()
        {
            Color red = ColorUtils.CreateColor(0xff, 0x00, 0x80);
            Assert.That(red.r, Is.EqualTo(1.0f));
            Assert.That(red.g, Is.EqualTo(0.0f));
            Assert.AreEqual(0.502f, red.b, 0.0001f);
        }
        
        [Test]
        public void CreateColorWithAlphaAsByte()
        {
            Color transparentBlue = ColorUtils.CreateColor(0x00, 0x00, 0xff, 0x7f);
            Assert.AreEqual(0.498f, transparentBlue.a, delta:0.0001f);
        }

        [Test]
        public void CreateGray()
        {
            float grayValue = 0.2f;
            Color gray = ColorUtils.CreateGray(grayValue, alpha: 0.5f);
            Assert.That(gray.r,Is.EqualTo(grayValue));
            Assert.That(gray.g,Is.EqualTo(grayValue));
            Assert.That(gray.b,Is.EqualTo(grayValue));
            Assert.That(gray.a,Is.EqualTo(0.5f));
        }
        
        [Test]
        public void CreateWhite()
        {
            Color white = ColorUtils.CreateWhite(alpha: 0.7f);
            Assert.That(white.r,Is.EqualTo(1.0f));
            Assert.That(white.g,Is.EqualTo(1.0f));
            Assert.That(white.b,Is.EqualTo(1.0f));
            Assert.That(white.a,Is.EqualTo(0.7f));
        }
    }
}
