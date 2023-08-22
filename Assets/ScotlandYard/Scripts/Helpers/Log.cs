using System;
using System.Collections;
using System.Collections.Generic;

public static class Log
{

    public enum Level { DEBUG = 0, INFO = 1, WARN = 2, ERROR = 3 };
	
	public static Level showThisAndMoreGeneral = Level.WARN;
	
	public static List<string> forceDebugDisplayOfThisFacilities = new List<string>(){  };
	
	public static event Action<string, string> evError;
	public static event Action<string, string> evWarn;
	public static event Action<string, string> evInfo;
	public static event Action<string, string> evDebug;
	public static event Action<string> evAnyLine;

    public static void error(object caller, System.Exception e) { error(caller.GetClassName(), e); }
    public static void error(object caller, string message, System.Exception e) { error(caller.GetClassName(), message, e); }
    public static void error(string facility, System.Exception e) { error(facility, "", e); }
	public static void error(string facility, string message, System.Exception e)
	{
        System.Exception cur = e;

        while (cur != null)
        {
            error(facility, string.Format("{0} - {1}: {2}\n<color=grey>{3}</color>", message, cur.GetSimpleName(), cur.Message, cur.StackTrace));
            cur = cur.InnerException;
            message = "Inner Exception";
        }
	}

    public static void error(object caller, string text) { error(caller.GetClassName(), text); }
	public static void error(string facility, string text)
	{
		if (showThisAndMoreGeneral > Level.ERROR)return;
		
		if (evError != null)evError(facility, text);

        string line = string.Format("<color=grey>{0}</color> {1}", facility, text);
		
		if (evAnyLine != null)evAnyLine(line);
		
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_WII || UNITY_IPHONE || UNITY_ANDROID || UNITY_PS3 || UNITY_XBOX360 || UNITY_NACL || UNITY_FLASH || UNITY_DASHBOARD_WIDGET
		UnityEngine.Debug.LogError(line);
#else
		System.Console.WriteLine(line);
#endif	
	}

    public static void warn(object caller, string text) { warn(caller.GetClassName(), text); }
	public static void warn(string facility, string text)
	{
		if (showThisAndMoreGeneral > Level.WARN)return;
		
		if (evWarn != null)evWarn(facility, text);

        string line = string.Format("<color=grey>{0}</color> {1}", facility, text);
		
		if (evAnyLine != null)evAnyLine(line);
		
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_WII || UNITY_IPHONE || UNITY_ANDROID || UNITY_PS3 || UNITY_XBOX360 || UNITY_NACL || UNITY_FLASH || UNITY_DASHBOARD_WIDGET
		UnityEngine.Debug.LogWarning(line);
#else
		System.Console.WriteLine(line);
#endif	
	}

    public static void info(object caller, string text) { info(caller.GetClassName(), text); }
	public static void info(string facility, string text)
	{
		if (showThisAndMoreGeneral > Level.INFO)return;
		
		if (evInfo != null)evInfo(facility, text);

        string line = string.Format("<color=grey>{0}</color> {1}", facility, text);
		
		if (evAnyLine != null)evAnyLine(line);
		
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_WII || UNITY_IPHONE || UNITY_ANDROID || UNITY_PS3 || UNITY_XBOX360 || UNITY_NACL || UNITY_FLASH || UNITY_DASHBOARD_WIDGET
		UnityEngine.Debug.Log(line);
#else
		System.Console.WriteLine(line);
#endif	
	}

    public static void debug(object caller, string text) { debug(caller.GetClassName(), text); }
	public static void debug(string facility, string text)
	{
		bool forceDisplay = false;
		
		if (forceDebugDisplayOfThisFacilities != null && forceDebugDisplayOfThisFacilities.Contains(facility))
			forceDisplay = true;
		
		if (forceDisplay == false && showThisAndMoreGeneral > Level.DEBUG)return;
		
		if (evDebug != null)evDebug(facility, text);

        string line = string.Format("( <color=grey>{0}</color> {1} )", facility, text);
		
		if (evAnyLine != null)evAnyLine(line);
		
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_WII || UNITY_IPHONE || UNITY_ANDROID || UNITY_PS3 || UNITY_XBOX360 || UNITY_NACL || UNITY_FLASH || UNITY_DASHBOARD_WIDGET
		UnityEngine.Debug.Log(line);
#else
		System.Console.WriteLine(line);
#endif	
	}
}
