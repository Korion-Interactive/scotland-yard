using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ravity
{
    public static class GameObjectUtils
    {
        public static string GetTransformHierarchyString(this Component component) => GetTransformHierarchyString(component.gameObject);
        public static string GetTransformHierarchyString(this GameObject gameObject)
        {
            IEnumerable<string> hierarchy = GetTransformHierarchy(gameObject).Select(t => t.name);
            string hierarchyString = string.Join("/", hierarchy);
            return hierarchyString;
        }

        public static IEnumerable<Transform> GetTransformHierarchy(this Component component) => GetTransformHierarchy(component.gameObject);
        public static IEnumerable<Transform> GetTransformHierarchy(this GameObject gameObject)
        {
            List<Transform> hierarchy = new List<Transform>();
            for(Transform current = gameObject.transform; IsNullRef(current) == false; current = current.parent)
            {
                hierarchy.Add(current);
            }
            hierarchy.Reverse();
            return hierarchy;
        }

        public static bool IsNullRef(this Object obj) => ReferenceEquals(obj, null);
        public static bool IsUnityNull(this Object obj) => obj == null; // this is not a normal null check!
    }
}
