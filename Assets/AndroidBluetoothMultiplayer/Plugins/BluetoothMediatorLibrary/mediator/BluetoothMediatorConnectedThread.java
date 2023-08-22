/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.mediator;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.concurrent.atomic.AtomicBoolean;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;

import com.lostpolygon.unity.bluetoothmediator.interop.LogHelper;
import com.lostpolygon.unity.bluetoothmediator.udp.IUdpSocketConnectionEventListener;
import com.lostpolygon.unity.bluetoothmediator.udp.UdpSocketConnectionThread;

/**
 * This thread runs during a connection with a remote device. It handles all incoming and outgoing
 * transmissions.
 */
class BluetoothMediatorConnectedThread extends Thread {
    private static final String LOG_TITLE = "Connection";
    
    private static final int INT_SIZE = Integer.SIZE / 8;
    private static final int SHORT_SIZE = INT_SIZE / 2;

    /**
     * Defines the length of packet header. As UDP datagram can't be longer than 65512 bytes, 2
     * bytes for header are enough.
     */
    private static final int HEADER_SIZE = SHORT_SIZE;

    /**
     * Maximum allowed Bluetooth packet size.
     */
    private static final int BUFFER_SIZE = 65000;

    /**
     * Remote Bluetooth device.
     */
    protected final BluetoothDevice mBluetoothDevice;

    /**
     * Remote Bluetooth connection socket.
     */
    protected final BluetoothSocket mBluetoothSocket;

    /**
     * Bluetooth connection input stream.
     */
    protected final InputStream mBluetoothInStream;

    /**
     * Bluetooth connection output stream.
     */
    protected final OutputStream mBluetoothOutStream;

    /**
     * The UDP connection manager.
     */
    protected final UdpSocketConnectionThread mUdpSocketConnection;

    /**
     * The UDP connection events listener.
     */
    protected final NetworkConnectionListener mNetworkConnectionListener;

    /**
     * Whether to use packet separation.
     */
    protected final boolean mUsePacketSeparation;

    /**
     * Whether the tunneling is alive and running.
     */
    private final AtomicBoolean mIsRunning = new AtomicBoolean(false);

    /**
     * Whether to reassign the dstPort on a incoming datagram.
     */
    private boolean mIsReassignDstPortOnPacket;

    /**
     * Instantiates a new bluetooth mediator connected thread.
     *
     * @param socket              BluetoothSocket used for connection
     * @param bluetoothDevice     remote BluetoothDevice
     * @param dstHost             destination hostname used for UDP tunneling
     * @param dstPort             destination port used for UDP tunneling
     * @param srcPort             source port from which the UDP data will be sent
     * @param usePacketSeparation whether to use packet separation. Must generally be true
     */
    public BluetoothMediatorConnectedThread(
        BluetoothSocket socket,
        BluetoothDevice bluetoothDevice,
        String dstHost,
        int dstPort,
        int srcPort,
        boolean usePacketSeparation
    ) {
        super("BluetoothMediatorConnectedThread [" + bluetoothDevice.getAddress() + "]");
        setDaemon(true);

        mBluetoothDevice = bluetoothDevice;

        if (BluetoothMediator.isVerboseLog())
            LogHelper.log("Create BluetoothMediatorConnectedThread", LOG_TITLE);

        mIsRunning.set(true);
        mIsReassignDstPortOnPacket = false;

        // Create UDP connection
        mUsePacketSeparation = usePacketSeparation;
        mNetworkConnectionListener = new NetworkConnectionListener();
        mUdpSocketConnection =
            new UdpSocketConnectionThread(
                mNetworkConnectionListener,
                dstHost,
                dstPort,
                srcPort
            );

        mUdpSocketConnection.initSocket();
        if (!mUdpSocketConnection.isDisconnected()) {
            if (BluetoothMediator.isVerboseLog())
                LogHelper.log("Created UDP socket", LOG_TITLE);
        } else {
            LogHelper.logError("UDP socket creation error", LOG_TITLE, null);
            mIsRunning.set(false);

            mBluetoothSocket = socket;
            mBluetoothInStream = null;
            mBluetoothOutStream = null;

            cancel();
            return;
        }

        if (!mUdpSocketConnection.isDisconnecting()) {
            mUdpSocketConnection.setName("UdpSocketConnectionThread [" + mBluetoothDevice.getAddress() + "]");

            // Starting the UDP datagram receive loop
            mUdpSocketConnection.start();
        }

        // Setting Bluetooth stream
        mBluetoothSocket = socket;
        InputStream tmpIn;
        OutputStream tmpOut;

        // Get the BluetoothSocket input and output streams
        try {
            tmpIn = socket.getInputStream();
            tmpOut = socket.getOutputStream();
        } catch (IOException e) {
            tmpIn = null;
            tmpOut = null;
            LogHelper.logError("Temp streams not created", LOG_TITLE, e);
        }

        mBluetoothInStream = tmpIn;
        mBluetoothOutStream = tmpOut;
    }

    @Override
    public void interrupt() {
        cancel();
        super.interrupt();
    }

    @Override
    public boolean isInterrupted() {
        return !mIsRunning.get() || super.isInterrupted();
    }

    /**
     * Main thread. Receives data from Bluetooth and retransmits it over UDP.
     *
     * @see java.lang.Thread#run()
     */
    public void run() {
        if (!isRunning())
            return;

        if (BluetoothMediator.isVerboseLog()) {
            LogHelper.log("Start ConnectedThread", LOG_TITLE);
        }

        byte[] buffer = new byte[BUFFER_SIZE];
        int readBytes;

        // Keep listening to the InputStream while connected
        while (true) {
            try {
                // Read from the InputStream
                if (mUsePacketSeparation) {
                    boolean packetIsReceivingSize = true;
                    int packetSize = HEADER_SIZE;
                    int packetReadBytes = 0;
                    while (mIsRunning.get() && (readBytes = mBluetoothInStream.read(buffer, packetReadBytes, packetSize - packetReadBytes)) != -1) {
                        packetReadBytes += readBytes;

                        // Read packet size
                        if (packetIsReceivingSize) {
                            if (packetReadBytes == HEADER_SIZE) {
                                packetSize = byteArrayToShortInt(buffer);
                                if (packetSize > BUFFER_SIZE)
                                    throw new Exception("Erroneous packet received: packetSize > BUFFER_SIZE. This should not ever happen.");

                                packetIsReceivingSize = false;
                                packetReadBytes = 0;
                            }
                        }
                        // Read packet
                        else {
                            if (packetReadBytes == packetSize) {
                                // Send packet
                                onBluetoothRead(buffer, 0, packetSize);

                                // Start reading another packet
                                packetSize = HEADER_SIZE;
                                packetIsReceivingSize = true;
                                packetReadBytes = 0;
                            }
                        }
                    }
                } else {
                    while (mIsRunning.get() && (readBytes = mBluetoothInStream.read(buffer)) != -1) {
                        // Send packet
                        onBluetoothRead(buffer, 0, readBytes);
                    }
                }
            } catch (IOException e) {
                LogHelper.log("Disconnected", LOG_TITLE);
                if (mIsRunning.get())
                    cancel();

                break;
            } catch (Exception e) {
                LogHelper.logError("Unexpected exception", LOG_TITLE, e);
                if (mIsRunning.get())
                    cancel();

                break;
            }
        }

        if (BluetoothMediator.isVerboseLog())
            LogHelper.log("End ConnectedThread", LOG_TITLE);
    }


    /**
     * Cancels the tunneled connection.
     */
    protected void cancel() {
        synchronized (mIsRunning) {
            if (!mIsRunning.get())
                return;

            mIsRunning.set(false);

            try {
                if (BluetoothMediator.isVerboseLog())
                    LogHelper.log("BTSocket close() start", LOG_TITLE);

                if (mBluetoothSocket != null) {
                    mBluetoothInStream.close();
                    mBluetoothOutStream.close();
                    mBluetoothSocket.close();
                }

                if (BluetoothMediator.isVerboseLog())
                    LogHelper.log("BTSocket close() successful", LOG_TITLE);
            } catch (IOException e) {
                LogHelper.logError("BTSocket close() failed", LOG_TITLE, e);
            }

            if (mUdpSocketConnection != null)
                mUdpSocketConnection.stopConnection();
        }
    }

    /**
     * Sets whether to reassign the dstPort on a incoming datagram
     */
    public void setReassignDstPortOnPacket() {
        mIsReassignDstPortOnPacket = true;
    }

    /**
     * Checks the connection status.
     *
     * @return true, if the tunneled connection is alive and running
     */
    public boolean isRunning() {
        return mIsRunning.get();
    }

    /**
     * Called when a data packet is received via Bluetooth.
     *
     * @param buffer Received bytes
     * @param length the read packet size
     */
    private void onBluetoothRead(byte[] buffer, int offset, int length) {
        mUdpSocketConnection.send(buffer, offset, length);
    }

    /**
     * Write to the Bluetooth stream.
     *
     * @param buffer The bytes to write
     * @param length the buffer size
     * @return true, if successful
     */
    private boolean bluetoothWrite(byte[] buffer, int offset, int length) {
        try {
            if (length > 0)
                mBluetoothOutStream.write(buffer, offset, length);

            return true;
        } catch (IOException e) {
            LogHelper.logError("Exception during write", LOG_TITLE, e);
        }

        return false;
    }

    /**
     * Converts a two-byte int into a byte[].
     *
     * @param value the value
     */
    private static void shortIntToByteArray(int value, byte[] buffer) {
        buffer[0] = (byte) (value >>> 8);
        buffer[1] = (byte) (value);
    }

    /**
     * Converts an length of 2 byte[] into an int.
     *
     * @param bytes the bytes
     * @return the int
     */
    private static int byteArrayToShortInt(byte[] bytes) {
        return (bytes[0] & 0xFF) << 8 | (bytes[1] & 0xFF);
    }

    /**
     * Listens for UDP connection events.
     */
    private class NetworkConnectionListener implements IUdpSocketConnectionEventListener {

        /**
         * Packet buffer.
         */
        private final byte[] mNewBuffer = new byte[BUFFER_SIZE + HEADER_SIZE];

        /**
         * Conversion buffer.
         */
        private final byte[] mConversionBuffer = new byte[SHORT_SIZE];

        /**
         * Called when a UDP datagram was received.
         *
         * @param buffer     datagram byte array
         * @param bufferSize the size of the datagram
         * @param srcPort    the port from which the datagram came from
         * @see com.lostpolygon.unity.bluetoothmediator.udp.IUdpSocketConnectionEventListener#onRead(byte[],
         * int, int)
         */
        @Override
        public void onRead(byte[] buffer, int bufferSize, int srcPort) {
            if (mIsReassignDstPortOnPacket) {
                BluetoothMediatorConnectedThread.this.mUdpSocketConnection.setDstPort(srcPort);
            }

            // Construct the packet and send it via Bluetooth
            if (mUsePacketSeparation) {
                System.arraycopy(buffer, 0, mNewBuffer, HEADER_SIZE, bufferSize);

                // Fill header
                shortIntToByteArray(bufferSize, mConversionBuffer);
                mNewBuffer[0] = mConversionBuffer[0];
                mNewBuffer[1] = mConversionBuffer[1];

                BluetoothMediatorConnectedThread.this.bluetoothWrite(mNewBuffer, 0, HEADER_SIZE + bufferSize);
            } else {
                BluetoothMediatorConnectedThread.this.bluetoothWrite(buffer, 0, bufferSize);
            }
        }

        /**
         * Called when some socket failure occurs. This generally means connection is interrupted.
         */
        @Override
        public void onSocketFailure() {
            LogHelper.logError("onSocketFailure()", LOG_TITLE, null);
            cancel();
        }
    }
}
