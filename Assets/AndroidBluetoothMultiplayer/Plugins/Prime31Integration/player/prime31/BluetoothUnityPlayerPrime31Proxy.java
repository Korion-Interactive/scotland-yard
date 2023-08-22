/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.player.prime31;

import android.bluetooth.BluetoothAdapter;
import android.content.Intent;
import android.os.Bundle;

import com.lostpolygon.unity.bluetoothmediator.mediator.BluetoothMediator;

/**
 * This class is a proxy for Prime31UnityActivity used to provide onActivityResult() method to the
 * BluetoothMediator.
 */
public class BluetoothUnityPlayerPrime31Proxy {

    public static void onCreate(Bundle savedInstanceState) {
        // Workaround for Android pre-4.1.
        // BluetoothAdapter.getDefaultAdapter() returns null
        // before it was called on UI thread
        BluetoothAdapter.getDefaultAdapter();
    }

    public static void onActivityResult(int requestCode, int resultCode, Intent data) {
        BluetoothMediator.onActivityResult(requestCode, resultCode, data);
    }

}
