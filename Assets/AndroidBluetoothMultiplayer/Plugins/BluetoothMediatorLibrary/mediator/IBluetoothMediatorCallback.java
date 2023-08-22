/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.mediator;

import android.bluetooth.BluetoothAdapter;

/**
 * The Interface IBluetoothMediatorCallback.
 */
interface IBluetoothMediatorCallback {

    /**
     * Gets the mediator settings.
     *
     * @return the settings
     */
    BluetoothMediator.MediatorSettings getSettings();

    /**
     * Gets the BluetoothAdapter instance.
     *
     * @return the adapter
     */
    BluetoothAdapter getAdapter();

    /**
     * Called from within client/host to notify that connection has been stopped.
     */
    void onMediatorStopped();

}
