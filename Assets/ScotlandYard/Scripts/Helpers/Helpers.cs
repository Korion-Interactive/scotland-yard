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

    public static string TransferMrXTextToMrsX(string textID)
    {
        switch(textID)
        {
            case "MrX_turn":
                return "MsX_turn";
            case "player_selection_misterx":
                return "player_selection_msx";
            case "pass_device":
                return "pass_device_msx";
            case "game_end_text_MrXCaught":
                return "game_end_text_MsXCaught";
            case "game_end_title_MrXCaught":
                return "game_end_title_MsXCaught";
            case "game_end_text_EscapeOfMrX":
                return "game_end_text_EscapeOfMsX";
            case "game_end_title_EscapeOfMrX":
                return "game_end_title_EscapeOfMsX";
            case "game_end_text_MrXSurrounded":
                return "game_end_text_MsXSurrounded";
            case "game_end_title_MrXSurrounded":
                return "game_end_title_MsXSurrounded";

        }
        return textID;
    }
}
