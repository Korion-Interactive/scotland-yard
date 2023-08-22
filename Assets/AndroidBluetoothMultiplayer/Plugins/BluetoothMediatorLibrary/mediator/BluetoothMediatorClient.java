/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.mediator;

import java.io.IOException;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;

import com.lostpolygon.unity.bluetoothmediator.interop.LogHelper;
import com.lostpolygon.unity.bluetoothmediator.interop.UnityEvents;

/**
 * Class representing the Bluetooth tunneling client.
 */
class BluetoothMediatorClient extends BluetoothMediatorEntityBase {
    private static final String LOG_TITLE = "Client";
    
    /**
     * Bluetooth connection UDP tunneling thread.
     */
    private BluetoothMediatorConnectedThread mBluetoothConnectedClientThread;

    /**
     * A thread that connects to the server.
     */
    private BluetoothMediatorConnectThread mBluetoothConnectThread;

    /**
     * Instantiates a new Bluetooth mediator client.
     *
     * @param bluetoothMediator the IBluetoothMediatorCallback interface instance
     */
    BluetoothMediatorClient(IBluetoothMediatorCallback bluetoothMediator) {
        super(bluetoothMediator);
    }

    /**
     * Start the connection procedure.
     */
    public synchronized void start() {
        if (BluetoothMediator.isVerboseLog())
            LogHelper.log("Starting", LOG_TITLE);

        mIsStopped = false;

        // Cancel any thread attempting to make a connection
        if (mBluetoothConnectThread != null) {
            mBluetoothConnectThread.interrupt();
            mBluetoothConnectThread = null;
        }

        // Start the thread to listen on a BluetoothServerSocket
        mBluetoothConnectThread = new BluetoothMediatorConnectThread(this);
        mBluetoothConnectThread.start();
    }

    /**
     * Stop the connection procedure and the ongoing connection, if any.
     */
    public synchronized void stop() {
        if (BluetoothMediator.isVerboseLog())
            LogHelper.log("Stopping", LOG_TITLE);

        if (mBluetoothConnectThread != null) {
            if (BluetoothMediator.isVerboseLog())
                LogHelper.log("Stopping ConnectThread", LOG_TITLE);

            mBluetoothConnectThread.interrupt();
            mBluetoothConnectThread = null;
        }

        if (mBluetoothConnectedClientThread != null) {
            if (BluetoothMediator.isVerboseLog())
                LogHelper.log("Stopping ConnectedClientThread", LOG_TITLE);

            mBluetoothConnectedClientThread.interrupt();
            mBluetoothConnectedClientThread = null;
        }
    }

    /**
     * Called when an attempt to connect to the server failed.
     *
     * @param socket BluetoothSocket used for server connection
     * @param device the server BluetoothDevice
     */
    public synchronized void onConnectToBluetoothServerFailed(BluetoothSocket socket, BluetoothDevice device) {
        UnityEvents.connectionToServerFailed(device);
        mMediatorCallback.onMediatorStopped();
    }

    /**
     * Called on successful connection to the server.
     *
     * @param socket BluetoothSocket used for server connection
     * @param device the server BluetoothDevice
     */
    public synchronized void onConnectToBluetoothServer(BluetoothSocket socket, BluetoothDevice device) {
        mBluetoothConnectThread = null;
        if (mIsStopped) {
            try {
                socket.close();
            } catch (IOException e) {
                LogHelper.logError("Socket close() failed", LOG_TITLE, e);
            }

            return;
        }

        // Start the thread to manage the connection and perform data transfer
        mBluetoothConnectedClientThread =
            new BluetoothMediatorConnectedClientThread(
                socket,
                device,
                mMediatorCallback.getSettings().remoteHost,
                0,
                mMediatorCallback.getSettings().remotePort,
                mMediatorCallback.getSettings().usePacketSeparation
            );

        if (mBluetoothConnectedClientThread.isRunning()) {
            mBluetoothConnectedClientThread.setReassignDstPortOnPacket();
            mBluetoothConnectedClientThread.start();
        }

        // Notify Unity
        UnityEvents.connectedToServer(device);
    }

    /**
     * Called when disconnected from the server.
     *
     * @param device the device
     */
    private void onServerDisconnected(BluetoothDevice device) {
        UnityEvents.disconnectedFromServer(device);
        mMediatorCallback.onMediatorStopped();
    }

    /**
     * Bluetooth connection handling thread.
     */
    private class BluetoothMediatorConnectedClientThread extends BluetoothMediatorConnectedThread {
        /**
         * Instantiates a thread that encapsulates an established connection.
         *
         * @param socket              BluetoothSocket used for connection
         * @param bluetoothDevice     remote BluetoothDevice
         * @param dstHost             destination hostname used for UDP tunneling
         * @param dstPort             destination port used for UDP tunneling
         * @param srcPort             source port from which the UDP data will be sent
         * @param usePacketSeparation whether to use packet separation. Must generally be true
        */
        public BluetoothMediatorConnectedClientThread(
            BluetoothSocket socket,
            BluetoothDevice bluetoothDevice,
            String dstHost,
            int dstPort,
            int srcPort,
            boolean usePacketSeparation
        ) {
            super(socket, bluetoothDevice, dstHost, dstPort, srcPort, usePacketSeparation);
        }

        /**
         * Stops the connection and notifies the BluetoothMediatorClient.
         *
         * @see BluetoothMediatorConnectedThread#cancel()
         */
        public void cancel() {
            super.cancel();

            try {
                BluetoothMediatorClient.this.onServerDisconnected(mBluetoothDevice);
            } catch (Exception e) {
                if (BluetoothMediator.isVerboseLog())
                    LogHelper.logError("Exception while disconnecting", LOG_TITLE, e);
            }
        }
    }
}
