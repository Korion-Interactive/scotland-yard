/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.mediator;

import java.io.IOException;

import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;

import com.lostpolygon.unity.bluetoothmediator.interop.LogHelper;
import com.lostpolygon.unity.bluetoothmediator.interop.UnityEvents;

/**
 * The thread that accepts incoming Bluetooth connections.
 */
class BluetoothMediatorAcceptThread extends Thread {
    private static final String LOG_TITLE = "Server";
    
    /**
     * The BluetoothMediatorServer instance.
     */
    private final BluetoothMediatorServer mBluetoothMediatorServer;
    // local server socket
    /**
     * The Bluetooth socket used for connecting.
     */
    private BluetoothServerSocket mBluetoothAcceptSocket;
    private boolean mIsCancelled;

    /**
     * Instantiates a new Bluetooth accept thread.
     *
     * @param bluetoothMediatorServer the BluetoothMediatorServer instance
     */
    BluetoothMediatorAcceptThread(BluetoothMediatorServer bluetoothMediatorServer) {
        super("BluetoothMediatorAcceptThread");
        mBluetoothMediatorServer = bluetoothMediatorServer;
    }

    /**
     * Allocates the listening RFCOMM channel.
     */
    public synchronized void startListening() {
        // Create a new listening server socket
        try {
            mBluetoothAcceptSocket =
                mBluetoothMediatorServer
                    .getMediatorCallback()
                    .getAdapter()
                    .listenUsingRfcommWithServiceRecord(
                        "BluetoothMediatorInsecure",
                        mBluetoothMediatorServer.getMediatorCallback().getSettings().uuid
                    );
        } catch (IOException e) {
            if (BluetoothMediator.isVerboseLog()) {
                LogHelper.logError("Socket listen() failed", LOG_TITLE, e);
            }
            cancel();
        }
    }

    /**
     * @see java.lang.Thread#run()
     */
    public void run() {
        if (mBluetoothAcceptSocket == null)
            return;

        if (BluetoothMediator.isVerboseLog())
            LogHelper.log("Starting AcceptThread", LOG_TITLE);

        BluetoothSocket socket;

        // Listen to the server socket if we're not connected
        UnityEvents.listeningStarted();
        while (true) {
            try {
                // This is a blocking call and will only return on a
                // successful connection or an exception
                socket = mBluetoothAcceptSocket.accept();
            } catch (IOException e) {
                if (!mIsCancelled && BluetoothMediator.isVerboseLog()) {
                    LogHelper.logError("IOException in mBluetoothAcceptSocket.accept(), canceling", LOG_TITLE, e);
                }

                cancel();
                break;
            }

            // If a connection was accepted, notifying the server instance
            if (socket != null) {
                mBluetoothMediatorServer.onDeviceConnected(socket, socket.getRemoteDevice());
            }
        }

        if (BluetoothMediator.isVerboseLog())
            LogHelper.log("Ending AcceptThread", LOG_TITLE);

        // When listening is canceled
        synchronized (BluetoothMediatorAcceptThread.this) {
            if (mBluetoothAcceptSocket != null) {
                if (BluetoothMediator.isVerboseLog())
                    LogHelper.log("Socket cancel", LOG_TITLE);

                cancel();
                mBluetoothAcceptSocket = null;
            }
        }
    }

    /**
     * Cancels the listening process.
     */
    public synchronized void cancel() {
        if (mBluetoothAcceptSocket != null && !mIsCancelled) {
            try {
                mIsCancelled = true;
                mBluetoothAcceptSocket.close();
                mBluetoothAcceptSocket = null;
            } catch (IOException e) {
                LogHelper.logError("Socket close() failed", LOG_TITLE, e);
            }

            UnityEvents.listeningStopped();
        }
    }
}