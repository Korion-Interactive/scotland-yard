using System.Globalization;
using UnityEngine;

namespace BitBarons.Util
{
    public static partial class ParseMethodHelper
    {
        public static void RegisterMyParseMethods()
        {
            Parser.RegisterTryParseMethod<Vector2>(TryParseVector2, true);
            Parser.RegisterTryParseMethod<Vector3>(TryParseVector3, true);
            Parser.RegisterTryParseMethod<Vector4>(TryParseVector4, true);
            Parser.RegisterTryParseMethod<Color>(TryParseColor, true);
        }

        static bool TryParseVector2(string input, out Vector2 result)
        {
            var parts = GetMultiComponentData(input, "Vector2");
            
            float x, y;
            if (parts.Length == 2
                && TryParseFloat(parts[0], out x)
                && TryParseFloat(parts[1], out y))
            {
                result = new Vector2(x, y);
                return true;
            }
            
            return DefaultFalse<Vector2>(out result);
        }

        static bool TryParseVector3(string input, out Vector3 result)
        {
            var parts = GetMultiComponentData(input, "Vector3");

            float x, y, z;
            if (parts.Length == 3
                && TryParseFloat(parts[0], out x)
                && TryParseFloat(parts[1], out y)
                && TryParseFloat(parts[2], out z))
            {
                result = new Vector3(x, y, z);
                return true;
            }
            
            return DefaultFalse<Vector3>(out result);
        }
        static bool TryParseVector4(string input, out Vector4 result)
        {
            var parts = GetMultiComponentData(input, "Vector4");

            float x, y, z, w;
            if (parts.Length == 4
                && TryParseFloat(parts[0], out x)
                && TryParseFloat(parts[1], out y)
                && TryParseFloat(parts[2], out z)
                && TryParseFloat(parts[3], out w))
            {
                result = new Vector4(x, y, z, w);
                return true;
            }
            
            return DefaultFalse<Vector4>(out result);
        }

        static bool TryParseColor(string input, out Color result)
        {
            var parts = GetMultiComponentData(input, "RGBA");

            float r, g, b, a;
            if ((parts.Length == 3 || parts.Length == 4)
                && TryParseFloat(parts[0], out r)
                && TryParseFloat(parts[1], out g)
                && TryParseFloat(parts[2], out b))
            {
                if (parts.Length == 4 && TryParseFloat(parts[3], out a))
                    result = new Color(r, g, b, a);
                else
                    result = new Color(r, g, b);
                
                return true;
            }

            return DefaultFalse<Color>(out result);
        }

        
        
        private static bool TryParseFloat(string input, out float value)
        {
            return float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
