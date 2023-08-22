/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.interop;

import java.text.SimpleDateFormat;
import java.util.Locale;

import android.util.Log;

/**
 * Helper class for logcat logs.
 */
public class LogHelper {
    private final static String LOG_TAG = "BluetoothMultiplayer";
    private final static Boolean LOG_THREAD_ID = true;
    private static boolean sStartupMessageLogged;

    public static void log(String text, String title) {
        Log.d(LOG_TAG, formatMessage(text, title));
    }

    public static void logWarning(String text, String title) {
        Log.w(LOG_TAG, formatMessage(text, title));
    }

    public static void logError(String text, String title, Throwable throwable) {
        text = formatMessage(text, title);
        if (throwable == null) {
            Log.e(LOG_TAG, text);
        } else {
            Log.e(LOG_TAG, text, throwable);
        }
    }

    public static void logStartupMessage() {
        
    }

    private static String formatMessage(String text, String title) {
        if (text == null)
            text = "null";

        text = title != null ? title + " - " + text : text;
        if (LOG_THREAD_ID) {
            text = "{" + Long.toString(Thread.currentThread().getId()) + "} " + text;
        }
        return text;
    }
}
