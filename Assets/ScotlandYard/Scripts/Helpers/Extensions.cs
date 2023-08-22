using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Extensions
{
    public static void LogError(this object self, System.Exception ex) { Log.error(self, ex); }
    public static void LogError(this object self, string msg, System.Exception ex) { Log.error(self, msg, ex); }
    public static void LogError(this object self, string msg) { Log.error(self, msg); }
    public static void LogError(this object self, string msg, Color color) { Log.error(self, string.Format("<color=#{0}>{1}</color>", color.GetRgbHexString(), msg)); }

    public static void LogWarn(this object self, string msg) { Log.warn(self, msg); }
    public static void LogWarn(this object self, string msg, Color color) { Log.warn(self, string.Format("<color=#{0}>{1}</color>", color.GetRgbHexString(), msg)); }

    public static void LogInfo(this object self, string msg) { Log.info(self, msg); }
    public static void LogInfo(this object self, string msg, Color color) { Log.info(self, string.Format("<color=#{0}>{1}</color>", color.GetRgbHexString(), msg)); }

    public static void LogDebug(this object self, string msg) { Log.debug(self, msg); }
    public static void LogDebug(this object self, string msg, Color color) { Log.debug(self, string.Format("<color=#{0}>{1}</color>", color.GetRgbHexString(), msg)); }

    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Assert(this object self, bool assertion)
    {
        self.Assert(assertion, "Assertion Failed!");
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Assert(this object self, bool assertion, string errorMessage)
    {
        if (!assertion)
            throw new Exception(self.GetClassName() + ": " + errorMessage);
    }

    public static string GetRgbHexString(this Color self)
    {
        var rgb = self.GetRgbBytes();
        return string.Format("{0:X2}{1:X2}{2:X2}", rgb[0], rgb[1], rgb[2]);
    }

    public static byte[] GetRgbBytes(this Color self)
    {
        byte[] result = new byte[3];
        result[0] = (byte)Mathf.Round(self.r * 255);
        result[1] = (byte)Mathf.Round(self.g * 255);
        result[2] = (byte)Mathf.Round(self.b * 255);
        return result;
    }

    public static float LengthSquared(this Vector3 self)
    {
        return self.x * self.x + self.y * self.y + self.z * self.z;
    }

    public static float Length(this Vector3 self)
    {
        return Mathf.Sqrt(self.LengthSquared());
    }

    public static IEnumerable<T> Concat<T>(this IEnumerable<T> self, T obj)
    {
        foreach (var item in self)
            yield return item;

        yield return obj;
    }

    public static Transform[] GetAllChilds(this Transform self)
    {
        Transform[] result = new Transform[self.childCount];
        for (int i = 0; i < self.childCount; i++)
        {
            result[i] = self.GetChild(i);
        }

        return result;
    }


    public static Transform GetChildByName(this Transform self, string name)
    {
        foreach (Transform child in self)
        {
            if (child.name == name)
                return child;
        }
        return null;
    }
    public static IEnumerable<Transform> GetChildsByName(this Transform self, string name)
    {
        foreach (Transform child in self)
        {
            if (child.name == name)
                yield return child;
        }
    }

    public static IEnumerable<Transform> GetChildsByTag(this Transform self, string tag)
    {
        foreach (Transform child in self)
        {
            if (child.tag == tag)
                yield return child;
        }
    }

    public static void ApplyToSelfAndAllChildren(this Transform self, Action<GameObject> applyMethod)
    {
        if (applyMethod != null)
            applyMethod(self.gameObject);

        for (int i = 0; i < self.childCount; i++)
        {
            var child = self.GetChild(i);
            child.ApplyToSelfAndAllChildren(applyMethod);
        }

    }

    public static T GetComponentInThisOrParent<T>(this GameObject self) where T : class
    {
        Type t = typeof(T);

        T result = self.GetComponent(t) as T;

        if (result == null)
            result = self.transform.parent.GetComponent(t) as T;

        return result;
    }

    public static IEnumerable<T> GetComponentsInThisAndParent<T>(this GameObject self) where T : Component
    {
        foreach (var comp in self.GetComponents<T>())
            yield return comp;

        foreach (var comp in self.transform.parent.GetComponents<T>())
            yield return comp;
    }
    public static T GetComponentWithInterface<T>(this GameObject self) where T : class
    {
        return self.GetComponents<Component>().OfType<T>().FirstOrDefault();
    }

    public static IEnumerable<T> GetComponentsWithInterface<T>(this GameObject self) where T : class
    {
        return self.GetComponents<Component>().OfType<T>();
    }

    public static void ForEach<T>(this IEnumerable<T> self, Action<T> callback)
    {
        if(callback == null)
            return;

        foreach (T obj in self)
            callback(obj);
    }

    public static string GetClassName(this object self)
    {
        return self.GetType().Name.Split('.').Last().Split('`').First();
    }

    public static void ResetToIdentity(this Transform self)
    {
        self.localPosition = new Vector3();
        self.localRotation = Quaternion.identity;
        self.localScale = Vector3.one;
    }

    public static Texture2D GetCopy(this Texture2D self)
    {
        bool useMipMaps = self.mipmapCount > 1;

        Texture2D result = new Texture2D(self.width, self.height, self.format, useMipMaps);
        //result.alphaIsTransparency = self.alphaIsTransparency;
        result.anisoLevel = self.anisoLevel;
        result.filterMode = self.filterMode;
        result.hideFlags = self.hideFlags;
        result.mipMapBias = self.mipMapBias;
        result.name = self.name + "_copy";
        result.wrapMode = self.wrapMode;

        if (useMipMaps)
        {
            for (int mip = 0; mip < self.mipmapCount; mip++)
            {
                result.SetPixels(self.GetPixels(mip), mip);
            }
        }
        else
        {
            result.SetPixels(self.GetPixels());
        }

        result.Apply();

        return result;
    }

    public static bool AlmostEquals(this Color self, Color other, float maxByteDivergence = 6)
    {
        int divR = (int)Mathf.Abs(255 * (self.r - other.r));
        int divG = (int)Mathf.Abs(255 * (self.g - other.g));
        int divB = (int)Mathf.Abs(255 * (self.b - other.b));

        return divR + divG + divB <= maxByteDivergence;
    }


    public static string ToFileName(this DateTime self)
    {
        StringBuilder b = new StringBuilder();
        b.Append(self.Year).Append(self.Month).Append(self.Day);
        b.Append("_");
        b.Append(self.Hour).Append(self.Minute).Append(self.Second);

        return b.ToString();
    }

    public static void AddRange<T>(this List<T> self, params T[] items)
    {
        self.AddRange(items);
    }

    public static int CompareKeys<TKey, TValue>(this KeyValuePair<TKey, TValue> self, KeyValuePair<TKey, TValue> other)
    where TKey : IComparable<TKey>
    {
        return self.Key.CompareTo(other.Key);
    }
    public static int CompareValues<TKey, TValue>(this KeyValuePair<TKey, TValue> self, KeyValuePair<TKey, TValue> other)
        where TValue : IComparable<TValue>
    {
        return self.Value.CompareTo(other.Value);
    }

    public static bool IsInRange<T>(this T value, T a, T b)
        where T : IComparable<T>
    {
        if (a.CompareTo(b) == 1)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        return value.CompareTo(a) >= 0 && value.CompareTo(b) <= 0;
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> self)
    {
        HashSet<T> result = new HashSet<T>();
        foreach (var val in self)
            result.Add(val);

        return result;
    }

    /// <summary>
    /// Removes and returns the object at the top of the Queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static T Pop<T>(this Queue<T> self)
    {
        Queue<T> tmp = new Queue<T>();

        while (self.Count > 1)
            tmp.Enqueue(self.Dequeue());

        T result = self.Dequeue();

        while (tmp.Count > 0)
            self.Enqueue(tmp.Dequeue());

        return result;
    }

    public static string GetSimpleName(this object self)
    {
        return self.ToString().Split('.').Last();
    }

    public static void SetComponentsEnabled<T>(this GameObject self, bool enabled)
        where T : MonoBehaviour
    {
        foreach (T comp in self.GetComponents<T>())
            comp.enabled = enabled;
    }
    
    public static void SetComponentsInChildrenEnabled<T>(this GameObject self, bool enabled)
        where T : MonoBehaviour
    {
        foreach (T comp in self.GetComponentsInChildren<T>())
        {
            comp.enabled = enabled;
        }
    }
    

    public static string ToString<T>(this IEnumerable<T> self, Func<T, string> toStringMethod)
    {
        if(toStringMethod == null)
            return self.ToString();

        StringBuilder sb = new StringBuilder("[ ");

        foreach (T item in self)
            sb.Append(toStringMethod(item)).Append(", ");

        sb.Append(" ]");

        return sb.ToString();
    }

    public static T PickRandom<T>(this T[] self)
    {
        return self[UnityEngine.Random.Range(0, self.Length)];
    }

    public static void WaitAndDo(this MonoBehaviour self, YieldInstruction waitingTime, Func<bool> predicate, Action action)
    {
        self.StartCoroutine(CoWaitAndDo(waitingTime, predicate, action));
    }

    static IEnumerator CoWaitAndDo(YieldInstruction waitingTime, Func<bool> predicate, Action action)
    {
        do
        {
            yield return waitingTime;
        }
        while (predicate != null && !predicate());

        if (action != null)
            action();
    }

    public static void RepeatDo(this MonoBehaviour self, YieldInstruction intervalTime, Func<bool> terminationPredicate, Action action)
    {
        self.StartCoroutine(CoRepeatDo(intervalTime, terminationPredicate, action));
    }

    static IEnumerator CoRepeatDo(YieldInstruction intervalTime, Func<bool> terminationPredicate, Action action)
    {
        while(terminationPredicate == null || !terminationPredicate())
        {
            if (action != null)
                action();

            yield return intervalTime;
        }
    }
    
}
