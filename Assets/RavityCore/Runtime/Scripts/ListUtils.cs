using System.Collections.Generic;
using UnityEngine;

namespace Ravity
{
    public class ListUtils : MonoBehaviour
    {
        public static void LimitSize<T>(List<T> list, int limit)
        {
            if (list.Count > limit)
            {
                list.RemoveRange(
                    index: limit,
                    count: list.Count - limit
                );
            }
        }
    }
}