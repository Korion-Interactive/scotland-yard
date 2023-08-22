using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;

namespace Ravity
{
    public class ListUtilsTests
    {
        [Test]
        public void LimitSize()
        {
            string[] array = {"1", "2", "3", "4", "5"};
            List<string> list = new List<string>(array);
            ListUtils.LimitSize(list, 10);
            Assert.That(list.Count,Is.EqualTo(5));
            
            ListUtils.LimitSize(list, 4);
            Assert.That(list.Count,Is.EqualTo(4));
        }
    }
}