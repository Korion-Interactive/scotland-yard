using NUnit.Framework;
using UnityEngine;

namespace Ravity
{
    public class VectorUtilsTests
    {
        [Test]
        public void Vector2_SquaredDistance()
        {
            Vector2 a = new Vector2(x: 2f, y: 3f);
            Vector2 b = new Vector2(x: 10f, y: -3f);
            float expected = Mathf.Pow(Vector2.Distance(a, b), 2);
            float result = VectorUtils.SquaredDistance(a, b);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Vector3_SquaredDistance()
        {
            Vector3 a = new Vector3(x: 2f, y: 3f, z: 5f);
            Vector3 b = new Vector3(x: 10f, y: -3f, z: -35f);
            float expected = Mathf.Pow(Vector3.Distance(a, b), 2);
            float result = VectorUtils.SquaredDistance(a, b);
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
