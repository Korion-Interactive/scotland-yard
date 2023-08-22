using System;
using NUnit.Framework;
using UnityEngine;

namespace Ravity
{
    public class RectTransformUtilsTests
    {
        private GameObject CreateGameObjectWithRectTransform(out RectTransform rectTransform)
        {
            GameObject gameObject = new GameObject("Test GameObject");
            rectTransform = gameObject.AddComponent<RectTransform>();
            Assert.That(rectTransform,Is.Not.Null);
            return gameObject;
        }

        [Test]
        public void GetRectTransform_FromGameObjectWithoutRectTransform_Throws()
        {
            GameObject gameObject = new GameObject("HasNoRect");
            Assert.That(() => gameObject.GetRectTransform(), Throws.TypeOf<InvalidCastException>());
        }

        [Test]
        public void GetRectTransform_FromComponentWithoutRectTransform_Throws()
        {
            GameObject gameObject = new GameObject("HasNoRect");
            Assert.That(() => gameObject.transform.GetRectTransform(), Throws.TypeOf<InvalidCastException>());
        }

        [Test]
        public void GetRectTransform_FromGameObjectWithRectTransform_ReturnsCorrect()
        {
            GameObject gameObject = CreateGameObjectWithRectTransform(out RectTransform expectedRectTransform);
            RectTransform rectTransform = gameObject.GetRectTransform();
            Assert.That(rectTransform, Is.EqualTo(expectedRectTransform));
        }

        [Test]
        public void GetRectTransform_FromComponentWithRectTransform_ReturnsCorrect()
        {
            GameObject gameObject = CreateGameObjectWithRectTransform(out RectTransform expectedRectTransform);
            RectTransform rectTransform = gameObject.transform.GetRectTransform();
            Assert.That(rectTransform, Is.EqualTo(expectedRectTransform));
        }
        
        #region Anchored Position
        [Test]
        public void SetAnchoredPositionX()
        {
            CreateGameObjectWithRectTransform(out RectTransform rectTransform);
            rectTransform.anchoredPosition = new Vector2(100, 200);
            RectTransformUtils.SetAnchoredPositionX(rectTransform, 300);
            Assert.That(rectTransform.anchoredPosition,Is.EqualTo(new Vector2(300,200)));
        }
        
        [Test]
        public void SetAnchoredPositionY()
        {
            CreateGameObjectWithRectTransform(out RectTransform rectTransform);
            rectTransform.anchoredPosition = new Vector2(100, 200);
            RectTransformUtils.SetAnchoredPositionY(rectTransform, 300);
            Assert.That(rectTransform.anchoredPosition,Is.EqualTo(new Vector2(100,300)));
        }
        #endregion
        
        #region Left, Top, Right, Bottom
        [Test]
        public void SetLeftRightTopBottom()
        {
            CreateGameObjectWithRectTransform(out RectTransform rectTransform);
            rectTransform.offsetMin = new Vector2(20, 20);
            rectTransform.offsetMax = new Vector2(20, 20);
            RectTransformUtils.SetLeftRightTopBottom(rectTransform,1,2,3,4);
            Assert.That(rectTransform.offsetMin.x, Is.EqualTo(1));
            Assert.That(rectTransform.offsetMin.y, Is.EqualTo(4));
            Assert.That(rectTransform.offsetMax.x, Is.EqualTo(-2));
            Assert.That(rectTransform.offsetMax.y, Is.EqualTo(-3));
        }
        
        [Test]
        public void SetLeftRight()
        {
            CreateGameObjectWithRectTransform(out RectTransform rectTransform);
            rectTransform.offsetMin = new Vector2(20, 30);
            rectTransform.offsetMax = new Vector2(40, 50);
            RectTransformUtils.SetLeftRight(rectTransform,1,2);
            Assert.That(rectTransform.offsetMin.x, Is.EqualTo(1));
            Assert.That(rectTransform.offsetMin.y, Is.EqualTo(30));
            Assert.That(rectTransform.offsetMax.x, Is.EqualTo(-2));
            Assert.That(rectTransform.offsetMax.y, Is.EqualTo(50));
        }
        
        [Test]
        public void SetTopBottom()
        {
            CreateGameObjectWithRectTransform(out RectTransform rectTransform);
            rectTransform.offsetMin = new Vector2(20, 30);
            rectTransform.offsetMax = new Vector2(40, 50);
            RectTransformUtils.SetTopBottom(rectTransform,1,2);
            Assert.That(rectTransform.offsetMin.x, Is.EqualTo(20));
            Assert.That(rectTransform.offsetMin.y, Is.EqualTo(2));
            Assert.That(rectTransform.offsetMax.x, Is.EqualTo(40));
            Assert.That(rectTransform.offsetMax.y, Is.EqualTo(-1));
        }
        
        [Test]
        public void SetLeft()
        {
            CreateGameObjectWithRectTransform(out RectTransform rectTransform);
            rectTransform.anchorMin = new Vector2(20, 30);
            rectTransform.anchorMax = new Vector2(40, 50);
            RectTransformUtils.SetLeft(rectTransform,500f);
            Assert.That(rectTransform.offsetMin.x, Is.EqualTo(500f));
        }
        
        [Test]
        public void SetRight()
        {
            CreateGameObjectWithRectTransform(out RectTransform rectTransform);
            rectTransform.anchorMin = new Vector2(20, 30);
            rectTransform.anchorMax = new Vector2(40, 50);
            RectTransformUtils.SetRight(rectTransform,500f);
            Assert.That(rectTransform.offsetMax.x, Is.EqualTo(-500f));
        }
        
        [Test]
        public void SetBottom()
        {
            CreateGameObjectWithRectTransform(out RectTransform rectTransform);
            rectTransform.anchorMin = new Vector2(20, 30);
            rectTransform.anchorMax = new Vector2(40, 50);
            RectTransformUtils.SetBottom(rectTransform,500f);
            Assert.That(rectTransform.offsetMin.y, Is.EqualTo(500f));
        }
        
        [Test]
        public void SetTop()
        {
            CreateGameObjectWithRectTransform(out RectTransform rectTransform);
            rectTransform.anchorMin = new Vector2(20, 30);
            rectTransform.anchorMax = new Vector2(40, 50);
            RectTransformUtils.SetTop(rectTransform,500f);
            Assert.That(rectTransform.offsetMax.y, Is.EqualTo(-500f));
        }
        #endregion
        
        #region Size Delta
        [Test]
        public void SetSizeDeltaWidth()
        {
            CreateGameObjectWithRectTransform(out RectTransform rectTransform);
            rectTransform.sizeDelta = new Vector2(50, 50);
            RectTransformUtils.SetSizeDeltaWidth(rectTransform,100f);
            Assert.That(rectTransform.sizeDelta,Is.EqualTo(new Vector2(100,50)));
        }

        [Test]
        public void SetSizeDeltaHeight()
        {
            CreateGameObjectWithRectTransform(out RectTransform rectTransform);
            rectTransform.sizeDelta = new Vector2(20, 20);
            RectTransformUtils.SetSizeDeltaHeight(rectTransform,5000f);
            Assert.That(rectTransform.sizeDelta,Is.EqualTo(new Vector2(20,5000)));
        }
        #endregion
    }
}
