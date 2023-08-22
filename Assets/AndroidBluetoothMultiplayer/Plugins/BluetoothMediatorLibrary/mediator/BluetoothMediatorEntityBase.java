/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.mediator;

abstract class BluetoothMediatorEntityBase {
    /**
     * The BluetoothMediator interface.
     */
    protected final IBluetoothMediatorCallback mMediatorCallback;

    /**
     * Whether the entity was stopped.
     */
    protected boolean mIsStopped = false;

    /**
     * Instantiates a new Bluetooth base instance.
     *
     * @param bluetoothMediator the IBluetoothMediatorCallback interface instance
     */
    protected BluetoothMediatorEntityBase(IBluetoothMediatorCallback bluetoothMediator) {
        mMediatorCallback = bluetoothMediator;
    }

    /**
     * Gets the BluetoothMediator interface.
     *
     * @return the IBluetoothMediatorCallback instance
     */
    public IBluetoothMediatorCallback getMediatorCallback() {
        return mMediatorCallback;
    }
}
