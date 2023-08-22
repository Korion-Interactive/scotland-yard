using UnityEngine;
using System.Collections;
using System;

public static class Helpers
{

    public static Color ColorFromRGBA(byte r, byte g, byte b, byte a)
    {
        return new Color(
            r/255f,
            g/255f,
            b/255f,
            a/255f
            );
    }

    public static Color ColorFromRGB(byte r, byte g, byte b)
    {
        return new Color(
            r / 255f,
            g / 255f,
            b / 255f
            );
    }

    internal static T Clamp<T>(T val, T min, T max)
        where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0)
            return min;
        if (val.CompareTo(max) > 0)
            return max;

        return val;
    }

    public static IEnumerator CoFadeAudio(AudioSource audio, float targetVolume, float duration, Action finishedCallback)
    {
        targetVolume = Clamp(targetVolume, 0, 1);

        float startVolume = audio.volume;
        float timePassed = 0;

        while((targetVolume > startVolume && audio.volume < targetVolume) || (targetVolume < startVolume && audio.volume > targetVolume))
        {
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;

            float t = Math.Min(1, timePassed / duration);
            audio.volume = Mathf.Lerp(startVolume, targetVolume, t);
        }

        if (finishedCallback != null)
            finishedCallback();
    }
}
