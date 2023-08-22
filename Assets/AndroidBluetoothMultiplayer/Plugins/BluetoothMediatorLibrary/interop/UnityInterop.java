/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.interop;

import com.lostpolygon.unity.bluetoothmediator.mediator.BluetoothMediator;
import com.unity3d.player.UnityPlayer;

/**
 * Wrapper class for sending string messages to Unity via UnityPlayer.UnitySendMessage().
 */
public class UnityInterop {
    private static final String LOG_TITLE = "Interop";

    /** The name of GameObject that will receive the callbacks. */
    private static final String UNITY_OBJECT = "_AndroidBluetoothMultiplayer";

    /** Value indicating whether the Unity Activity presence test has already been done. */
    private static boolean mIsUnityPresenceChecked = false;

    /**
     * Value indicating whether the Unity Activity is present and UnitySendMessage can be called.
     */
    private static boolean mIsUnityPresent;

    /**
     * Sends message to UNITY_OBJECT GameObject.
     *
     * @param methodName
     *            Method to call in UNITY_OBJECT.
     * @param data
     *            String data to be passed.
     */
    public static void UnitySendMessage(String methodName, String data) {
        UnitySendMessage(UNITY_OBJECT, methodName, data);
    }

    /**
     * Sends message to Unity GameObject objName.
     *
     * @param objName
     *            Name of GameObject which will receive the message.
     * @param methodName
     *            Method to call.
     * @param data
     *            String data to be passed.
     */
    private static void UnitySendMessage(String objName, String methodName, String data) {
        // Run the test
        if (!mIsUnityPresenceChecked) {
            try {
                mIsUnityPresent = true;
                UnityPlayer.UnitySendMessage("", "", "");
            } catch (NoClassDefFoundError e) {
                mIsUnityPresent = false;
            }
            if (BluetoothMediator.isVerboseLog())
                LogHelper.log("Running under Unity player: " + (mIsUnityPresent ? "True" : "False"), LOG_TITLE);

            mIsUnityPresenceChecked = true;
        }

        // Send the message if running under Unity Activity
        if (!mIsUnityPresent)
            return;

        if (methodName == null) {
            LogHelper.logWarning("An attempt to call UnitySendMessage with null 'methodName' was detected. This should not happen, please report this.", LOG_TITLE);
            return;
        }

        if (data == null) {
            LogHelper.logWarning("An attempt to call UnitySendMessage with null 'data' was detected. This should not happen, please report this.", LOG_TITLE);
            return;
        }

        if (BluetoothMediator.isVerboseLog())
            LogHelper.log("Call event '" + methodName + "'", LOG_TITLE);

        UnityPlayer.UnitySendMessage(objName, methodName, data);
    }
}
