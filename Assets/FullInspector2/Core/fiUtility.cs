using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Internal {
    public static class fiUtility {
        public static string CombinePaths(string a, string b) {
            return Path.Combine(a, b).Replace('\\', '/');
        }
        public static string CombinePaths(string a, string b, string c) {
            return Path.Combine(Path.Combine(a, b), c).Replace('\\', '/');
        }
        public static string CombinePaths(string a, string b, string c, string d) {
            return Path.Combine(Path.Combine(Path.Combine(a, b), c), d).Replace('\\', '/');
        }

        /// <summary>
        /// Destroys the given object using the proper destroy function. If the game is in edit
        /// mode, then DestroyImmedate is used. Otherwise, Destroy is used.
        /// </summary>
        public static void DestroyObject(UnityObject obj) {
            if (Application.isPlaying) {
                UnityObject.Destroy(obj);
            }
            else {
                UnityObject.DestroyImmediate(obj);
            }
        }

        /// <summary>
        /// Removes leading whitespace after newlines from a string. This is extremely useful when
        /// using the C# multiline @ string.
        /// </summary>
        public static string StripLeadingWhitespace(this string s) {
            // source: http://stackoverflow.com/a/7178336
            Regex r = new Regex(@"^\s+", RegexOptions.Multiline);
            return r.Replace(s, string.Empty);
        }

        /// <summary>
        /// This is equivalent to Application.isEditor except that it can be called off of
        /// the main thread.
        /// </summary>
        public static bool IsEditor {
            get {
                if (_cachedIsEditor.HasValue == false) {
                    _cachedIsEditor = Type.GetType("UnityEditor.Editor, UnityEditor", /*throwOnError:*/false) != null;
                }

                return _cachedIsEditor.Value;
            }
        }
        private static bool? _cachedIsEditor;

        /// <summary>
        /// Creates a dictionary from the given keys and given values.
        /// </summary>
        /// <typeparam name="TKey">The key type of the dictionary.</typeparam>
        /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
        /// <param name="keys">The keys in the dictionary. A null key will be skipped.</param>
        /// <param name="values">The values in the dictionary.</param>
        /// <returns>A dictionary that contains the given key to value mappings.</returns>
        public static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>(IList<TKey> keys, IList<TValue> values) {
            var dict = new Dictionary<TKey, TValue>();

            if (keys != null && values != null) {
                for (int i = 0; i < Mathf.Min(keys.Count, values.Count); ++i) {
                    if (ReferenceEquals(keys[i], null)) continue;

                    dict[keys[i]] = values[i];
                }
            }

            return dict;
        }

        /// <summary>
        /// Swaps two items.
        /// </summary>
        public static void Swap<T>(ref T a, ref T b) {
            T tmp = a;
            a = b;
            b = tmp;
        }
    }
}