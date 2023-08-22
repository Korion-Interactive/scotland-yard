using UnityEngine;

namespace Ravity
{
    public static class VectorUtils
    {
        public static float SquaredDistance(Vector2 a, Vector2 b) => (a - b).sqrMagnitude;
        public static float SquaredDistance(Vector3 a, Vector3 b) => (a - b).sqrMagnitude;
    }
}
