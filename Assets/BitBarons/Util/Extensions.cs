using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitBarons.Util
{
    public static class Extensions
    {
        public static string GetShortcutName(this Type t)
        {
            if (t == null)
                return "< null >";

            string result = t.ToString();

            switch(result)
            {
                case "System.Byte":
                    result = "byte";
                    break;
                case "System.Int16":
                    result = "short";
                    break;
                case "System.Int32":
                    result = "int";
                    break;
                case "System.Int64":
                    result = "long";
                    break;
                case "System.UInt16":
                    result = "ushort";
                    break;
                case "System.UInt32":
                    result = "uint";
                    break;
                case "System.UInt64":
                    result = "ulong";
                    break;
                case "System.Boolean":
                    result = "bool";
                    break;
                case "System.Single":
                    result = "float";
                    break;
                case "System.Double":
                    result = "double";
                    break;
                case "System.String":
                    result = "string";
                    break;
                case "System.Char":
                    result = "char";
                    break;
                case "System.Decimal":
                    result = "decimal";
                    break;
                case "System.SByte":
                    result = "sbyte";
                    break;
            }

            return result;
        }

    }
}
