/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.mediator;

import java.io.IOException;
import java.util.concurrent.atomic.AtomicBoolean;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;

import com.lostpolygon.unity.bluetoothmediator.interop.LogHelper;

/**
 * Manages the process of connecting to the server and returns the result to BluetoothMediatorClient
 * instance.
 */
class BluetoothMediatorConnectThread extends Thread {
    private static final String LOG_TITLE = "Client";
    
    /**
     * The connection Bluetooth socket.
     */
    private final BluetoothSocket mSocket;

    /**
     * The server Bluetooth device.
     */
    private final BluetoothDevice mDevice;

    /**
     * The BluetoothMediatorClient instance that has created the thread. Connection result will be
     * passed to it.
     */
    private final BluetoothMediatorClient mBluetoothMediatorClient;

    /**
     * Whether the thread is going to be interrupted and the sockets is going to be closed.
     */
    private volatile AtomicBoolean mIsClosingSocket = new AtomicBoolean(false);

    /**
     * Instantiates a new Bluetooth connection thread.
     *
     * @param bluetoothMediatorClient the BluetoothMediatorClient instance. Connection result will be passed to it.
     */
    BluetoothMediatorConnectThread(BluetoothMediatorClient bluetoothMediatorClient) {
        super("BluetoothMediatorConnectThread [" + bluetoothMediatorClient.getMediatorCallback().getSettings().hostDevice.getAddress() + "]");
        mBluetoothMediatorClient = bluetoothMediatorClient;
        mDevice = mBluetoothMediatorClient.getMediatorCallback().getSettings().hostDevice;

        BluetoothSocket tmpSocket = null;
        // Get a BluetoothSocket for a connection to the given BluetoothDevice
        try {
            tmpSocket = mDevice.createRfcommSocketToServiceRecord(mBluetoothMediatorClient.getMediatorCallback().getSettings().uuid);
        } catch (IOException e) {
            if (BluetoothMediator.isVerboseLog()) {
                LogHelper.logError("Socket createRfcommSocketToServiceRecord() failed", LOG_TITLE, e);
            }

            // Notifying the client about the failure
            mBluetoothMediatorClient.onConnectToBluetoothServerFailed(null, mDevice);
        }

        mSocket = tmpSocket;
    }

    @Override
    public void interrupt() {
        cancel();
        super.interrupt();
    }

    @Override
    public boolean isInterrupted() {
        return mIsClosingSocket.get() || super.isInterrupted();
    }

    /**
     * @see java.lang.Thread#run()
     */
    public void run() {
        if (BluetoothMediator.isVerboseLog())
            LogHelper.log("Start ConnectThread", LOG_TITLE);

        // Always cancel discovery because it will slow down a connection
        mBluetoothMediatorClient.getMediatorCallback().getAdapter().cancelDiscovery();

        // Make a connection to the BluetoothSocket
        try {
            // This is a blocking call and will only return on a
            // successful connection, error, or interruption
            mSocket.connect();
        } catch (IOException e) {
            // Error occurred, closing connections
            if (isInterrupted())
                return;

            // Close the socket
            try {
                mSocket.close();
            } catch (IOException e2) {
                LogHelper.logError("Unable to close() socket during connection failure", LOG_TITLE, e);
            }
            if (BluetoothMediator.isVerboseLog()) {
                LogHelper.logError("Connection failed", LOG_TITLE, e);
            }

            // Notifying the client about the failure
            mBluetoothMediatorClient.onConnectToBluetoothServerFailed(mSocket, mDevice);

            return;
        }

        if (isInterrupted())
            return;

        // Start the connected thread on successful connection
        mBluetoothMediatorClient.onConnectToBluetoothServer(mSocket, mDevice);
    }

    /**
     * Cancel the connection.
     */
    protected void cancel() {
        if (mIsClosingSocket.get())
            return;

        try {
            mIsClosingSocket.set(true);

            if (BluetoothMediator.isVerboseLog())
                LogHelper.log("BTSocket close() start", LOG_TITLE);

            mSocket.close();

            if (BluetoothMediator.isVerboseLog())
                LogHelper.log("BTSocket close() successful", LOG_TITLE);
        } catch (IOException e) {
            // We don't care about exception as we won't need that socket anyway
            // Log.e(TAG, "close() of connect " + mSocketType + " socket failed", e);
        }
    }
}
