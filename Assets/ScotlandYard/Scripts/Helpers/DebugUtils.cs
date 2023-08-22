#define DEBUG

using System;
using System.Diagnostics;

public static class DebugUtils
{
    [Conditional("DEBUG")]
    public static void Assert(bool condition)
    {
        if (!condition) {
			Log.error("ASSERT", "ASSERT");
		}
    }

	[Conditional("DEBUG")]
    public static void Assert(bool condition, string text)
    {
        if (!condition) {
			Log.error("ASSERT", text);
		}
    }
}