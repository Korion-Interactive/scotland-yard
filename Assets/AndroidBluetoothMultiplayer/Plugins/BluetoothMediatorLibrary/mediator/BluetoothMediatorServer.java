/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.mediator;

import java.io.IOException;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;

import com.lostpolygon.unity.bluetoothmediator.interop.UnityEvents;
import com.lostpolygon.unity.bluetoothmediator.interop.LogHelper;

/**
 * Class representing the Bluetooth tunneling server.
 */
class BluetoothMediatorServer extends BluetoothMediatorEntityBase {
    private static final String LOG_TITLE = "Server";
    
    /**
     * The incoming connection listening thread.
     */
    private BluetoothMediatorAcceptThread mBluetoothAcceptThread;

    /**
     * The client manager.
     */
    private final ClientManager mClientManager = new ClientManager();

    /**
     * Whether server is listening to the clients.
     */
    private boolean mIsListening = false;

    /**
     * Instantiates a new Bluetooth multiplayer server.
     *
     * @param bluetoothMediator the IBluetoothMediatorCallback interface instance
     */
    BluetoothMediatorServer(IBluetoothMediatorCallback bluetoothMediator) {
        super(bluetoothMediator);
    }

    /**
     * Start listening to incoming connections.
     */
    public synchronized void start() {
        if (BluetoothMediator.isVerboseLog())
            LogHelper.log("Starting", LOG_TITLE);

        mIsStopped = false;
        startListening();
    }

    public synchronized void startListening() {
        // Cancel any thread attempting to make a connection
        if (mBluetoothAcceptThread != null) {
            mBluetoothAcceptThread.cancel();
            mBluetoothAcceptThread = null;
        }

        // Start the thread to listen on a BluetoothServerSocket
        mBluetoothAcceptThread = new BluetoothMediatorAcceptThread(this);
        mBluetoothAcceptThread.startListening();
        mBluetoothAcceptThread.start();

        mIsListening = true;
    }

    /**
     * Stops listening to incoming connections
     *
     * @return true, if successful
     */
    public synchronized boolean stopListening() {
        if (mBluetoothAcceptThread != null) {
            if (BluetoothMediator.isVerboseLog())
                LogHelper.log("stopListening()", LOG_TITLE);

            // Canceling the accept thread
            mIsListening = false;
            mBluetoothAcceptThread.cancel();

            return true;
        }

        return false;
    }

    /**
     * Stop the server.
     */
    public synchronized void stop() {
        mIsStopped = true;

        stopListening();

        if (BluetoothMediator.isVerboseLog())
            LogHelper.log("Disconnecting clients", LOG_TITLE);

        synchronized (mClientManager) {
            for (BluetoothMediatorConnection client : mClientManager.getClients().values()) {
                if (client == null) {
                    LogHelper.logWarning("Attempt to clear a null client. This should not happen, please report this.", LOG_TITLE);
                    continue;
                }

                BluetoothMediatorConnectedThread clientThread = client.getBluetoothConnectedThread();

                if (clientThread != null)
                    clientThread.cancel();

                client.setBluetoothConnectedThread(null);
            }

            mClientManager.getClients().clear();
        }

        mMediatorCallback.onMediatorStopped();
    }

    /**
     * Start the BluetoothMediatorConnectedThread to begin managing a Bluetooth connection.
     *
     * @param socket The BluetoothSocket on which the connection was made
     * @param device The BluetoothDevice that has been connected
     */
    public synchronized void onDeviceConnected(BluetoothSocket socket, BluetoothDevice device) {
        if (mIsStopped || !mIsListening) {
            try {
                socket.close();
            } catch (IOException e) {
                // No need to process the exception when the server has already closed
            }

            return;
        }

        // Create the thread to manage the connection and perform transmissions
        BluetoothMediatorConnectedThread bluetoothConnectedServerThread = new BluetoothMediatorConnectedServerThread(
            socket,
            device,
            mMediatorCallback.getSettings().remoteHost,
            mMediatorCallback.getSettings().remotePort,
            0,
            mMediatorCallback.getSettings().usePacketSeparation);

        // Start the thread
        if (bluetoothConnectedServerThread.isRunning()) {
            bluetoothConnectedServerThread.start();

            // Updating client list
            synchronized (mClientManager) {
                Map<Integer, BluetoothMediatorConnection> clients = mClientManager.getClients();
                // Checking if client already exists
                BluetoothMediatorConnection existingClient = clients.get(device.hashCode());
                if (existingClient != null) {
                    // Updating client
                    existingClient.getBluetoothConnectedThread().cancel();
                    existingClient.setBluetoothConnectedThread(bluetoothConnectedServerThread);

                    clients.put(device.hashCode(), existingClient);

                    if (BluetoothMediator.isVerboseLog())
                        LogHelper.log("Client updated, device address: " + device.getAddress(), LOG_TITLE);
                } else {
                    // Adding new client
                    mClientManager.addClient(device, socket, bluetoothConnectedServerThread);
                    UnityEvents.clientConnected(device);

                    if (BluetoothMediator.isVerboseLog())
                        LogHelper.log("Client connected, device address: " + device.getAddress(), LOG_TITLE);
                }

                if (BluetoothMediator.isVerboseLog())
                    LogHelper.log("Total devices connected: " + Integer.toString(clients.size()), LOG_TITLE);
            }
        }
    }

    /**
     * Called when a client device disconnects. Removes the device from the client list and notifies
     * Unity
     *
     * @param device The BluetoothDevice that disconnected
     * @throws RuntimeException
     */
    private void onDeviceDisconnected(BluetoothDevice device) throws RuntimeException {
        synchronized (mClientManager) {
            if (BluetoothMediator.isVerboseLog())
                LogHelper.log("Removing device " + device.getAddress() + " from client list", LOG_TITLE);

            Map<Integer, BluetoothMediatorConnection> clients = mClientManager.getClients();
            BluetoothMediatorConnection disconnectedClient = clients.get(device.hashCode());
            if (disconnectedClient != null) {
                clients.remove(device.hashCode());

                UnityEvents.clientDisconnected(device);

                if (BluetoothMediator.isVerboseLog()) {
                    LogHelper.log("Device " + device.getAddress() + " disconnected", LOG_TITLE);
                    LogHelper.log("Total devices connected now: " + Integer.toString(clients.size()), LOG_TITLE);
                }
            } else {
                throw new RuntimeException("Non-existent client disconnected?!");
            }
        }
    }

    /**
     * Bluetooth connection handling thread.
     */
    private class BluetoothMediatorConnectedServerThread extends BluetoothMediatorConnectedThread {
        /**
         * Instantiates a new Bluetooth connection handling thread.
         *
         * @param socket              BluetoothSocket used for connection
         * @param bluetoothDevice     remote BluetoothDevice
         * @param dstHost             destination hostname used for UDP tunneling
         * @param dstPort             destination port used for UDP tunneling
         * @param srcPort             source port from which the UDP data will be sent
         * @param usePacketSeparation whether to use packet separation. Must generally be true
         */
        public BluetoothMediatorConnectedServerThread(
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
         * Stops the connection and notifies the BluetoothMediatorServer.
         *
         * @see BluetoothMediatorConnectedThread#cancel()
         */
        public void cancel() {
            super.cancel();

            try {
                BluetoothMediatorServer.this.onDeviceDisconnected(mBluetoothDevice);
            } catch (Exception e) {
                LogHelper.logError("Error while disconnecting", LOG_TITLE, e);
            }
        }
    }

    /**
     * Manages the connected Bluetooth client devices.
     */
    private final class ClientManager {
        /**
         * The clients HashMap.
         */
        private final ConcurrentHashMap<Integer, BluetoothMediatorConnection> clients = new ConcurrentHashMap<>();

        /**
         * Gets the clients HashMap.
         *
         * @return the clients map
         */
        public synchronized ConcurrentHashMap<Integer, BluetoothMediatorConnection> getClients() {
            return clients;
        }

        /**
         * Adds the client.
         *
         * @param socket                   BluetoothSocket used for connection
         * @param device                   remote BluetoothDevice
         * @param bluetoothConnectedThread the BluetoothMediatorConnectedThread instance of a client
         * @return the client
         */
        public synchronized BluetoothMediatorConnection addClient(
            BluetoothDevice device,
            BluetoothSocket socket,
            BluetoothMediatorConnectedThread bluetoothConnectedThread) {
            if (clients.containsKey(device.hashCode()))
                throw new RuntimeException(LOG_TITLE + " - Attempting to add client that already exists");

            BluetoothMediatorConnection newClient =
                new BluetoothMediatorConnection(
                    socket,
                    device
                );
            newClient.setBluetoothConnectedThread(bluetoothConnectedThread);
            clients.put(device.hashCode(), newClient);
            return newClient;
        }
    }
}