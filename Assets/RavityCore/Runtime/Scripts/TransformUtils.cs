using UnityEngine;

namespace Ravity
{
    public static class TransformUtils
    {
        public static void ModifyPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            Vector3 position = transform.position;
            if(x.HasValue) position.x = x.Value;
            if(y.HasValue) position.y = y.Value;
            if(z.HasValue) position.z = z.Value;
            transform.position = position;
        }

        public static void ModifyLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            Vector3 position = transform.localPosition;
            if(x.HasValue) position.x = x.Value;
            if(y.HasValue) position.y = y.Value;
            if(z.HasValue) position.z = z.Value;
            transform.localPosition = position;
        }
    }
}
