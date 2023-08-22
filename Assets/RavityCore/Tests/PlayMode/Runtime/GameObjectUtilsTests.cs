using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ravity.GameObjectUtilsTests
{
    #region TransformHierchy Helpers

    public class GetTransformHierarchy
    {
        [Test]
        public void ByComponent_SimpleTransformHierarchy_ReturnsCorrectListForEachElement()
        {
            SimpleTransformHierarchy hierarchy = SimpleTransformHierarchy.Get();
            Assert.That(hierarchy.Root.transform.GetTransformHierarchy(),        Is.EqualTo(new[] { hierarchy.Root }),                                     nameof(hierarchy.Root));
            Assert.That(hierarchy.ChildOfRoot.transform.GetTransformHierarchy(), Is.EqualTo(new[] { hierarchy.Root, hierarchy.ChildOfRoot }),              nameof(hierarchy.ChildOfRoot));
            Assert.That(hierarchy.Parent.transform.GetTransformHierarchy(),      Is.EqualTo(new[] { hierarchy.Root, hierarchy.Parent }),                   nameof(hierarchy.Parent));
            Assert.That(hierarchy.Child1.transform.GetTransformHierarchy(),      Is.EqualTo(new[] { hierarchy.Root, hierarchy.Parent, hierarchy.Child1 }), nameof(hierarchy.Child1));
            Assert.That(hierarchy.Child2.transform.GetTransformHierarchy(),      Is.EqualTo(new[] { hierarchy.Root, hierarchy.Parent, hierarchy.Child2 }), nameof(hierarchy.Child2));
        }

        [Test]
        public void ByGameObject_SimpleTransformHierarchy_ReturnsCorrectListForEachElement()
        {
            SimpleTransformHierarchy hierarchy = SimpleTransformHierarchy.Get();
            Assert.That(hierarchy.Root.gameObject.GetTransformHierarchy(),        Is.EqualTo(new[] { hierarchy.Root }),                                     nameof(hierarchy.Root));
            Assert.That(hierarchy.ChildOfRoot.gameObject.GetTransformHierarchy(), Is.EqualTo(new[] { hierarchy.Root, hierarchy.ChildOfRoot }),              nameof(hierarchy.ChildOfRoot));
            Assert.That(hierarchy.Parent.gameObject.GetTransformHierarchy(),      Is.EqualTo(new[] { hierarchy.Root, hierarchy.Parent }),                   nameof(hierarchy.Parent));
            Assert.That(hierarchy.Child1.gameObject.GetTransformHierarchy(),      Is.EqualTo(new[] { hierarchy.Root, hierarchy.Parent, hierarchy.Child1 }), nameof(hierarchy.Child1));
            Assert.That(hierarchy.Child2.gameObject.GetTransformHierarchy(),      Is.EqualTo(new[] { hierarchy.Root, hierarchy.Parent, hierarchy.Child2 }), nameof(hierarchy.Child2));
        }
    }

    public class GetTransformHierarchyString
    {
        [Test]
        public void ByComponent_SimpleTransformHierarchy_ReturnsCorrectStringForEachElement()
        {
            SimpleTransformHierarchy hierarchy = SimpleTransformHierarchy.Get();
            Assert.That(hierarchy.Root.transform.GetTransformHierarchyString(),        Is.EqualTo($"{hierarchy.Root.name}"));
            Assert.That(hierarchy.ChildOfRoot.transform.GetTransformHierarchyString(), Is.EqualTo($"{hierarchy.Root.name}/{hierarchy.ChildOfRoot.name}"));
            Assert.That(hierarchy.Parent.transform.GetTransformHierarchyString(),      Is.EqualTo($"{hierarchy.Root.name}/{hierarchy.Parent.name}"));
            Assert.That(hierarchy.Child1.transform.GetTransformHierarchyString(),      Is.EqualTo($"{hierarchy.Root.name}/{hierarchy.Parent.name}/{hierarchy.Child1.name}"));
            Assert.That(hierarchy.Child2.transform.GetTransformHierarchyString(),      Is.EqualTo($"{hierarchy.Root.name}/{hierarchy.Parent.name}/{hierarchy.Child2.name}"));
        }

        [Test]
        public void ByGameObject_SimpleTransformHierarchy_ReturnsCorrectStringForEachElement()
        {
            SimpleTransformHierarchy hierarchy = SimpleTransformHierarchy.Get();
            Assert.That(hierarchy.Root.gameObject.GetTransformHierarchyString(),        Is.EqualTo($"{hierarchy.Root.name}"));
            Assert.That(hierarchy.ChildOfRoot.gameObject.GetTransformHierarchyString(), Is.EqualTo($"{hierarchy.Root.name}/{hierarchy.ChildOfRoot.name}"));
            Assert.That(hierarchy.Parent.gameObject.GetTransformHierarchyString(),      Is.EqualTo($"{hierarchy.Root.name}/{hierarchy.Parent.name}"));
            Assert.That(hierarchy.Child1.gameObject.GetTransformHierarchyString(),      Is.EqualTo($"{hierarchy.Root.name}/{hierarchy.Parent.name}/{hierarchy.Child1.name}"));
            Assert.That(hierarchy.Child2.gameObject.GetTransformHierarchyString(),      Is.EqualTo($"{hierarchy.Root.name}/{hierarchy.Parent.name}/{hierarchy.Child2.name}"));
        }
    }

    public struct SimpleTransformHierarchy
    {
        //                     Root
        //                    /    \
        //              Parent      ChildOfRoot
        //             /     \
        //       Child1      Child2
        public static SimpleTransformHierarchy Get()
        {
            GameObject root = new GameObject(nameof(Root));
            GameObject childOfRoot = new GameObject(nameof(ChildOfRoot));
            GameObject parent = new GameObject(nameof(Parent));
            GameObject child1 = new GameObject(nameof(Child1));
            GameObject child2 = new GameObject(nameof(Child2));

            childOfRoot.transform.SetParent(root.transform);
            parent.transform.SetParent(root.transform);
            child1.transform.SetParent(parent.transform);
            child2.transform.SetParent(parent.transform);

            return new SimpleTransformHierarchy
            {
                Root = root.transform,
                ChildOfRoot = childOfRoot.transform,
                Parent = parent.transform,
                Child1 = child1.transform,
                Child2 = child2.transform
            };
        }

        public Transform Root, ChildOfRoot, Parent, Child1, Child2;
    }

    #endregion

    #region Null Checks

    public class IsNullRef
    {
        [Test]
        public void AliveGameObject_ReturnsFalse()
        {
            GameObject alive = new GameObject();
            Assert.That(alive.IsNullRef(), Is.False);
        }

        [Test]
        public void DestroyedGameObject_ReturnsFalse()
        {
            GameObject destroyed = new GameObject();
            Object.DestroyImmediate(destroyed);
            Assert.That(destroyed.IsNullRef(), Is.False);
        }

        [Test]
        public void NullReference_ReturnsTrue()
        {
            GameObject nullRef = null;
            Assert.That(nullRef.IsNullRef(), Is.True);
        }
    }

    public class IsUnityNull
    {
        [Test]
        public void AliveGameObject_ReturnsFalse()
        {
            GameObject alive = new GameObject();
            Assert.That(alive.IsUnityNull(), Is.False);
        }

        [Test]
        public void DestroyedGameObject_ReturnsTrue()
        {
            GameObject destroyed = new GameObject();
            Object.DestroyImmediate(destroyed);
            Assert.That(destroyed.IsUnityNull(), Is.True);
        }

        [Test]
        public void NullReference_ReturnsTrue()
        {
            GameObject nullRef = null;
            Assert.That(nullRef.IsUnityNull(), Is.True);
        }
    }

    #endregion
}
