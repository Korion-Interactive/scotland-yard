using System;
using NUnit.Framework;
using UnityEngine;

namespace Ravity
{
    public class TransformUtilsTests
    {
        [TestCaseSource(nameof(AllComponentCombinations))]
        public void ModifyLocalPosition_DifferentComponentCombinations_OnlyDesiredComponentsAreChanged(ComponentCombination combination)
        {
            (float changedX, float changedY, float changedZ) = (69f, 113f, -1004f);
            Transform transform = GetTransformWithParent();
            Vector3 initialPosition = transform.localPosition;

            Vector3 expectedPosition = Vector3.zero;
            switch (combination)
            {
                case ComponentCombination.None:
                    transform.ModifyLocalPosition();
                    expectedPosition = initialPosition;
                    break;
                case ComponentCombination.X:
                    transform.ModifyLocalPosition(x: changedX);
                    expectedPosition = new Vector3(changedX, initialPosition.y, initialPosition.z);
                    break;
                case ComponentCombination.Y:
                    transform.ModifyLocalPosition(y: changedY);
                    expectedPosition = new Vector3(initialPosition.x, changedY, initialPosition.z);
                    break;
                case ComponentCombination.Z:
                    transform.ModifyLocalPosition(z: changedZ);
                    expectedPosition = new Vector3(initialPosition.x, initialPosition.y, changedZ);
                    break;
                case ComponentCombination.XY:
                    transform.ModifyLocalPosition(x: changedX, y: changedY);
                    expectedPosition = new Vector3(changedX, changedY, initialPosition.z);
                    break;
                case ComponentCombination.XZ:
                    transform.ModifyLocalPosition(x: changedX, z: changedZ);
                    expectedPosition = new Vector3(changedX, initialPosition.y, changedZ);
                    break;
                case ComponentCombination.YZ:
                    transform.ModifyLocalPosition(y: changedY, z: changedZ);
                    expectedPosition = new Vector3(initialPosition.x, changedY, changedZ);
                    break;
                case ComponentCombination.XYZ:
                    transform.ModifyLocalPosition(x: changedX, y: changedY, z: changedZ);
                    expectedPosition = new Vector3(changedX, changedY, changedZ);
                    break;
            }

            Assert.That(transform.localPosition, Is.EqualTo(expectedPosition));
        }

        [TestCaseSource(nameof(AllComponentCombinations))]
        public void ModifyPosition_DifferentComponentCombinations_OnlyDesiredComponentsAreChanged(ComponentCombination combination)
        {
            (float changedX, float changedY, float changedZ) = (69f, 113f, -1004f);
            Transform transform = GetTransformWithParent();
            Vector3 initialPosition = transform.position;

            Vector3 expectedPosition = Vector3.zero;
            switch (combination)
            {
                case ComponentCombination.None:
                    transform.ModifyPosition();
                    expectedPosition = initialPosition;
                    break;
                case ComponentCombination.X:
                    transform.ModifyPosition(x: changedX);
                    expectedPosition = new Vector3(changedX, initialPosition.y, initialPosition.z);
                    break;
                case ComponentCombination.Y:
                    transform.ModifyPosition(y: changedY);
                    expectedPosition = new Vector3(initialPosition.x, changedY, initialPosition.z);
                    break;
                case ComponentCombination.Z:
                    transform.ModifyPosition(z: changedZ);
                    expectedPosition = new Vector3(initialPosition.x, initialPosition.y, changedZ);
                    break;
                case ComponentCombination.XY:
                    transform.ModifyPosition(x: changedX, y: changedY);
                    expectedPosition = new Vector3(changedX, changedY, initialPosition.z);
                    break;
                case ComponentCombination.XZ:
                    transform.ModifyPosition(x: changedX, z: changedZ);
                    expectedPosition = new Vector3(changedX, initialPosition.y, changedZ);
                    break;
                case ComponentCombination.YZ:
                    transform.ModifyPosition(y: changedY, z: changedZ);
                    expectedPosition = new Vector3(initialPosition.x, changedY, changedZ);
                    break;
                case ComponentCombination.XYZ:
                    transform.ModifyPosition(x: changedX, y: changedY, z: changedZ);
                    expectedPosition = new Vector3(changedX, changedY, changedZ);
                    break;
            }

            Assert.That(transform.position, Is.EqualTo(expectedPosition));
        }

        private static Transform GetTransformWithParent()
        {
            GameObject parent = new GameObject();
            parent.transform.position = new Vector3(2f, 100f, 9001f);
            GameObject gameObject = new GameObject();
            gameObject.transform.SetParent(parent.transform);
            Vector3 initialPosition = new Vector3(5f, -3f, 42f);
            gameObject.transform.position = initialPosition;
            return gameObject.transform;
        }

        private static Array AllComponentCombinations => Enum.GetValues(typeof(ComponentCombination));

        public enum ComponentCombination
        {
            None,
            X,
            Y,
            Z,
            XY,
            XZ,
            YZ,
            XYZ
        }
    }
}
