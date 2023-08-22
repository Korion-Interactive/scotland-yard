using System;

namespace BitBarons.Util
{
    /// <summary>
    /// some parse methods
    /// </summary>
    public static partial class ParseMethodHelper
    {

        /// <summary>
        /// Registers Parse and try parse methods for the following types:
        /// <list type="bullet">
        /// <item>Char</item> <item>Byte</item> <item>Int16</item> <item>Int32</item> <item>Int64</item> <item>Single</item>
        /// <item>Boolean</item> <item>UInt16</item> <item>UInt32</item> <item>UInt64</item> <item>Double</item> <item>String</item>
        /// </list>
        /// </summary>
        internal static void RegisterCommonSystemTypeParseMethods()
        {
            
            Parser.RegisterTryParseMethod<Byte>(Byte.TryParse);
            Parser.RegisterTryParseMethod<Int32>(Int32.TryParse);
            Parser.RegisterTryParseMethod<Single>(TryParseSingle, true);
            Parser.RegisterTryParseMethod<Boolean>(TryParseBoolean, true);
            Parser.RegisterTryParseMethod<String>(TryParseString);

            //Parser.RegisterTryParseMethod<Char>(Char.TryParse, true);
            //Parser.RegisterTryParseMethod<Int16>(Int16.TryParse);
            //Parser.RegisterTryParseMethod<Int64>(Int64.TryParse);
            //Parser.RegisterTryParseMethod<Double>(TryParseDouble, true);
            //Parser.RegisterTryParseMethod<UInt16>(UInt16.TryParse);
            //Parser.RegisterTryParseMethod<UInt32>(UInt32.TryParse);
            //Parser.RegisterTryParseMethod<UInt64>(UInt64.TryParse);

            Parser.RegisterParseMethod<Byte>(Byte.Parse);
            Parser.RegisterParseMethod<Int32>(Int32.Parse);
            Parser.RegisterParseMethod<String>(ParseString);

            //Parser.RegisterParseMethod<Int16>(Int16.Parse);
            //Parser.RegisterParseMethod<Int64>(Int64.Parse);
            //Parser.RegisterParseMethod<UInt16>(UInt16.Parse);
            //Parser.RegisterParseMethod<UInt32>(UInt32.Parse);
            //Parser.RegisterParseMethod<UInt64>(UInt64.Parse);

            RegisterMyParseMethods(); // points to other part of this class
        }

        /// <summary>
        /// Splits a string with some data and returns only the relevant data.
        /// eg. if you pass "Point(3, 5)" with the prefix "Point" the result will be { 3, 5 }.
        /// if you pass just "(3, 5)", "3, 5" or "{ 3, 5 }" the result will be the same. a semicolon ';' is also allowed instead of a comma ','.
        /// </summary>
        /// <param name="s">the full string you want to parse</param>
        /// <param name="prefixes">the prefix which will be cut away if there is such a prefix (pass as many as allowed)</param>
        /// <returns>the data of interest</returns>
        public static string[] GetMultiComponentData(string s, params string[] prefixes)
        {
            foreach (string prefix in prefixes)
            {
                if (s.StartsWith(prefix))
                {
                    s = s.Remove(0, prefix.Length);
                    break;
                }
            }
            s = s.TrimStart('(', '{');
            s = s.TrimEnd(')', '}');

            string[] parts = s.Split(new char[]{',', ';', ' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim();

            return parts;
        }

        /// <summary>
        /// This method sets the passed parameter to the default value and returns false. (it is a helper method to bind two lines into one)
        /// </summary>
        /// <typeparam name="T">the type of the variable</typeparam>
        /// <param name="result">the result which gets the default value</param>
        /// <returns>false</returns>
        public static bool DefaultFalse<T>(out T result)
        {
            result = default(T);
            return false;
        }


        public static bool TryParseBoolean(string s, out Boolean result)
        {
            string str = s.ToLower();
            if (str == "true" || str == "on" || str == "yes" || str == "1")
            {
                result = true;
                return true;
            }

            if (str == "false" || str == "off" || str == "no" || str == "0")
            {
                result = false;
                return true;
            }


            return DefaultFalse<Boolean>(out result);
        }

        public static bool TryParseSingle(string s, out Single result)
        {
            s = s.Replace(',', '.');
            return Single.TryParse(s, out result);
        }

        public static bool TryParseDouble(string s, out Double result)
        {
            s = s.Replace(',', '.');
            return Double.TryParse(s, out result);
        }

        public static string ParseString(string s)
        {
            return s;
        }
        public static bool TryParseString(string s, out string result)
        {
            result = s;
            return true;
        }

        //public static bool TryParseInt32(string s, out Int32 result)
        //{
        //    return Int32.TryParse(s.Replace(" ", ""), out result);
        //}

     
    }
}