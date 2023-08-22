/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.mediator;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;

/**
 * Represents a connected Bluetooth client device.
 */
class BluetoothMediatorConnection {

    /**
     * The socket.
     */
    private final BluetoothSocket mSocket;

    /**
     * The device.
     */
    private final BluetoothDevice mDevice;

    /**
     * Bluetooth connection UDP tunneling thread.
     */
    private BluetoothMediatorConnectedThread mBluetoothConnectedThread;

    /**
     * Instantiates a new Bluetooth client.
     *
     * @param socket the client BluetoothSocket
     * @param device the client BluetoothDevice
     */
    public BluetoothMediatorConnection(BluetoothSocket socket, BluetoothDevice device) {
        mSocket = socket;
        mDevice = device;
        mBluetoothConnectedThread = null;
    }

    /**
     * Gets the BluetoothSocket used for a client.
     *
     * @return the BluetoothSocket
     */
    public BluetoothSocket getSocket() {
        return mSocket;
    }

    /**
     * Gets the BluetoothDevice of a client.
     *
     * @return the BluetoothConnectedThread
     */
    public BluetoothDevice getDevice() {
        return mDevice;
    }

    /**
     * Gets the BluetoothConnectedThread for a client.
     *
     * @return the BluetoothConnectedThread
     */
    public synchronized BluetoothMediatorConnectedThread getBluetoothConnectedThread() {
        return mBluetoothConnectedThread;
    }

    /**
     * Sets the BluetoothConnectedThread for a client
     *
     * @param bluetoothConnectedThread new BluetoothConnectedThread
     */
    public synchronized void setBluetoothConnectedThread(
        BluetoothMediatorConnectedThread bluetoothConnectedThread) {
        mBluetoothConnectedThread = bluetoothConnectedThread;
    }
}