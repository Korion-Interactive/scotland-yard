using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using UnityEngine;


namespace BitBarons.Util
{
    /// <summary>
    /// A Delegation for a TryParse method.
    /// </summary>
    /// <typeparam name="T">the type to parse with the method</typeparam>
    /// <param name="s">A string containing a value which can be parsed to the given type</param>
    /// <param name="result">the result which is filled by the parsing process</param>
    /// <returns>true, if the parse process was successfull, otherwise false.</returns>
    public delegate bool TryParse<T>(string s, out T result);

    public static class Parser
    {
        static Dictionary<Type, Delegate> parseMethods = new Dictionary<Type,Delegate>();
        static Dictionary<Type, Delegate> tryParseMethods = new Dictionary<Type,Delegate>();

        static Parser()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            ParseMethodHelper.RegisterCommonSystemTypeParseMethods();
        }
        /// <summary>
        /// Registers a parse method. You don't need to pass the parse method to <code>myTable.Get</code> anymore when the method for the corresponding type has been registered here.
        /// </summary>
        /// <typeparam name="T">the Type which is returned by the parse method</typeparam>
        /// <param name="parseMethod">the parse method to register (in common <code>MyType.Parse</code>)</param>
        public static void RegisterParseMethod<T>(Func<string, T> parseMethod)
        {
            Type type = typeof(T);

            if (parseMethods == null)
                parseMethods = new Dictionary<Type, Delegate>();

            if (parseMethods.ContainsKey(type))
                Warn("Trying to add a parse method for tables. There already is a parse method for type \"" + type.ToString() + "\". only one parseMethod per type allowed");
            else
                parseMethods.Add(type, parseMethod);
        }
        /// <summary>
        /// Unregisters the parse method of the given type.
        /// </summary>
        /// <typeparam name="T">the type to unregister</typeparam>
        public static void UnregisterParseMethod<T>()
        {
            Type type = typeof(T);
            if (!parseMethods.ContainsKey(type))
            {
                Warn("tried to unregister parseMethod of type \"" + type.ToString() + "\". This type was not registered.");
                return;
            }
            parseMethods.Remove(type);
        }
        /// <summary>
        /// Registers a try-parse-method. You don't need to pass the try-parse-method to <code>myTable.TryGet</code> anymore when the method for the corresponding type has been registered here.
        /// </summary>
        /// <typeparam name="T">the Type which is returned by the try-parse-method</typeparam>
        /// <param name="tryParseMethod">the try-parse-method to register (in common <code>MyType.TryParse</code>)</param>
        public static void RegisterTryParseMethod<T>(TryParse<T> tryParseMethod)
        {
            RegisterTryParseMethod(tryParseMethod, false);
        }
        /// <summary>
        /// Registers a try-parse-method. You don't need to pass the try-parse-method to <code>myTable.TryGet</code> anymore when the method for the corresponding type has been registered here.
        /// </summary>
        /// <typeparam name="T">the Type which is returned by the try-parse-method</typeparam>
        /// <param name="tryParseMethod">the try-parse-method to register (in common <code>MyType.TryParse</code>)</param>
        /// <param name="generateParseMethod">if true, a parse method will be generated and must not be added seperately.</param>
        public static void RegisterTryParseMethod<T>(TryParse<T> tryParseMethod, bool generateParseMethod)
        {
            Type type = typeof(T);

            if (tryParseMethods == null)
                tryParseMethods = new Dictionary<Type, Delegate>();

            if (tryParseMethods.ContainsKey(type))
			{
                Warn("Trying to add a try-parse-method for tables. There already is a try-parse-method for type \"" + type.ToString() + "\". only one tryParseMethod per type allowed");
				return;
			}			
            else
                tryParseMethods.Add(type, tryParseMethod);


            if (generateParseMethod)
            {
                parseMethods.Add(type, (Func<string, T>)((s) =>
                    {
                        T result;
                        if (tryParseMethod(s, out result))
                            return result;
                        else
                            throw new ArgumentException("the given string couldn't be parsed to " + type.ToString());
                    })
                );
            }
        }
        /// <summary>
        /// Unregisters the try-parse-method of the given type.
        /// </summary>
        /// <typeparam name="T">the type to unregister</typeparam>
        public static void UnregisterTryParseMethod<T>()
        {
            Type type = typeof(T);
            if (!tryParseMethods.ContainsKey(type))
            {
                Warn("tried to unregister tryParseMethod of type \"" + type.ToString() + "\". This type was not registered.");
                return;
            }
            tryParseMethods.Remove(type);
        }



        /// <summary>
        /// Parses the passed string as the given type. returns the default value, if it doesn't work.
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="s">the string to parse</param>
        /// <returns>the parsed result</returns>
        public static T Parse<T>(string s)
        {
            Type type = typeof(T);

            if (type.IsEnum)
                return (T)Enum.Parse(type, s, false);

            Assert(parseMethods.ContainsKey(type), "Type \"" + type.ToString() + "\" has no parse method registered. default value will be returned.");
            if (!parseMethods.ContainsKey(type))
            {
                if (tryParseMethods.ContainsKey(type))
                {
                    T result;
                    if (((TryParse<T>)tryParseMethods[type])(s, out result))
                    {
                        return result;
                    }
                    else
                    {
                        Error("No Parse method and try parse failed for type: " + type.ToString() + " with value: " + s);
                        return default(T);
                    }
                }
                else
                {
                    Error("No Parse method for type: " + type.ToString());
                    return default(T);
                }
            }

            return ((Func<string, T>)parseMethods[type])(s);
        }

        /// <summary>
        /// tries to parse a value from a type by using a corresponding registered try-parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="s">the string to parse</param>
        /// <param name="result">the parsed result</param>
        /// <returns>true, if successful, otherwise false</returns>
        public static bool TryParse<T>(string s, out T result)
        {
            Type type = typeof(T);

            if (type.IsEnum)
            {
                try
                {
                    result = (T)Enum.Parse(type, s, false);
                    return true;
                }
                catch (Exception)
                {
                    result = default(T);
                    return false;
                }
            } 

            Assert(tryParseMethods != null, "no parse methods registered.");
            Assert(tryParseMethods.ContainsKey(type), "Type \"" + type.ToString() + "\" has no tryParseMethod registered. Default value will be used.");

            if (tryParseMethods.ContainsKey(type))
            {
                if (((TryParse<T>)tryParseMethods[type])(s, out result))
                    return true;
            }

            result = default(T);
            return false;
        }


        public static object Parse(Type type, string s)
        {
            if (type.IsEnum)
                return Enum.Parse(type, s, false);

#if !UNITY_IPHONE
            if (!parseMethods.ContainsKey(type))
            {
                Error("No Parse Method registered for type: " + type.ToString());
                return null;
            }

            return parseMethods[type].DynamicInvoke(s);
#else
            Error("[iOS problem] No Parse Method registered for type: " + type.ToString());
            return null;
#endif

        }

        /// <summary>
        /// Iterates through all types which have a Parse method registered
        /// </summary>
        public static IEnumerable<Type> GetAllRegisteredParseTypes()
        {
            foreach (var t in parseMethods.Keys)
                yield return t;
        }
        /// <summary>
        /// Iterates through all types which have a TryParse method registered
        /// </summary>
        public static IEnumerable<Type> GetAllRegisteredTryParseTypes()
        {
            foreach (var t in tryParseMethods.Keys)
                yield return t;
        }

        private static void Warn(string message)
        {
            Debug.LogWarning("Batch.Util.ParseManager: " + message);
        }

        private static void Error(string message)
        {
            Debug.LogError("Batch.Util.ParseManager: " + message);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                Error(message);
        }
    }
}
