/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.player;

import android.bluetooth.BluetoothAdapter;
import android.content.Intent;
import android.os.Bundle;

import com.lostpolygon.unity.bluetoothmediator.mediator.BluetoothMediator;
import com.unity3d.player.UnityPlayerActivity;

/**
 * This class extends UnityPlayerActivity in order to provide onActivityResult() method to the
 * BluetoothMediator.
 */
public class BluetoothUnityPlayerActivity extends UnityPlayerActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // Workaround for Android pre-4.1.
        // BluetoothAdapter.getDefaultAdapter() returns null
        // before it was called on UI thread
        BluetoothAdapter.getDefaultAdapter();
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        BluetoothMediator.onActivityResult(requestCode, resultCode, data);
    }

}
