/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.udp;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.Inet4Address;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.net.ServerSocket;

import com.lostpolygon.unity.bluetoothmediator.mediator.BluetoothMediator;
import com.lostpolygon.unity.bluetoothmediator.interop.LogHelper;

/**
 * This class manages the UDP socket connection.
 */
public class UdpSocketConnectionThread extends Thread {
    private static final String LOG_TITLE = "UDP";
    
    /**
     * Socket event listener (onRead and onSocketFailure).
     */
    private final IUdpSocketConnectionEventListener mListener;

    /**
     * Destination host name.
     */
    private final String mDstHost;

    private final DatagramPacket mSentPacket = new DatagramPacket(new byte[0], 0);

    /**
     * Destination port.
     */
    private int mDstPort;

    /**
     * Destination host address.
     */
    private InetAddress mDstAddress;

    /**
     * Source port to send data from
     */
    private int mSrcPort;

    /**
     * UDP socket being managed.
     */
    private DatagramSocket mSocket = null;

    /**
     * Value indicating whether the UDP socket is working and active.
     */
    private boolean mIsRunning = false;

    /**
     * Value indicating whether the socket in the process of being closed.
     */
    private boolean mIsDisconnecting = false;

    /**
     * Value indicating whether the socket is closed.
     */
    private boolean mIsDisconnected = true;

    /**
     * Constant value indicating the maximum packet size. While UDP packet technically can be up to
     * 65512 bytes, that usually won't occur in a typical game.
     */
    private static final int BUFFER_SIZE = 65000;

    /**
     * Instantiates a new UDP socket connection manager.
     *
     * @param listener Socket event listener (onRead and onSocketFailure)
     * @param dstName  Destination host name
     * @param dstPort  Destination port
     * @param srcPort  Source port to send data from
     */
    public UdpSocketConnectionThread(
        IUdpSocketConnectionEventListener listener,
        String dstName,
        int dstPort,
        int srcPort) {
        super();
        String threadName = String.format("(dstName: %1s, dstPort: %2s, srcPort: %3s)", dstName, dstPort, srcPort);
        setName("UdpSocketConnectionThread " + threadName);
        if (BluetoothMediator.isVerboseLog())
            LogHelper.log(threadName, LOG_TITLE);

        mDstHost = dstName;
        mDstPort = dstPort;
        mSrcPort = srcPort;
        mListener = listener;
    }

    /**
     * Sets the destination port.
     *
     * @param dstPort the new destination port
     */
    public void setDstPort(int dstPort) {
        mDstPort = dstPort;
    }

    /**
     * Initializes the socket
     *
     * @return port number of created socket
     */
    public int initSocket() {
        stopConnection(false);

        try {
            // Select a random port
            // if we haven't set any particular port
            if (mSrcPort <= 0) {
                mSrcPort = findFreePort();
            }

            // Setting up the socket
            mSocket = new DatagramSocket(null);
            mSocket.setBroadcast(false);
            mSocket.setReceiveBufferSize(BUFFER_SIZE);
            mSocket.setSendBufferSize(BUFFER_SIZE);
            mSocket.bind(new InetSocketAddress(Inet4Address.getByName("0.0.0.0"), mSrcPort));
            mDstAddress = InetAddress.getByName(mDstHost);

            mIsDisconnected = false;

            return mSrcPort;
        } catch (IOException e) {
            // Stopping the connection in case of exception.
            if (BluetoothMediator.isVerboseLog()) {
                LogHelper.logError("IOException in initSocket()", LOG_TITLE, e);
            }

            stopConnection(true);
            return -1;
        }
    }

    /**
     * Send the byte array to the mDstPort.
     *
     * @param buffer the packet byte array
     * @param length the size of the packet
     * @return true, if successful
     */

    public boolean send(byte[] buffer, int offset, int length) {
        if (!mIsRunning || mIsDisconnecting)
            return false;

        try {
            mSentPacket.setData(buffer, offset, length);
            mSentPacket.setAddress(mDstAddress);
            mSentPacket.setPort(mDstPort);

            mSocket.send(mSentPacket);
        } catch (IOException e) {
            stopConnection(true);

            LogHelper.logError("Error while sending", LOG_TITLE, e);
            return false;
        }

        return true;
    }

    /**
     * The main packet processing loop.
     *
     * @see java.lang.Thread#run()
     */
    public void run() {
        if (mSocket == null || !mSocket.isBound())
            return;

        if (BluetoothMediator.isVerboseLog()) {
            LogHelper.log("Entering run() loop", LOG_TITLE);
        }

        try {
            mIsRunning = true;

            byte[] buffer = new byte[BUFFER_SIZE];
            DatagramPacket receivedPacket = new DatagramPacket(buffer, BUFFER_SIZE);
            // The loop runs until disconnected or some error happened
            while (mIsRunning && !isDisconnecting() && mListener != null) {
                receivedPacket.setLength(BUFFER_SIZE);
                // Receiving the packet
                mSocket.receive(receivedPacket);
                // Passing the received packet to the listener
                mListener.onRead(receivedPacket.getData(), receivedPacket.getLength(), receivedPacket.getPort());
            }
        } catch (Exception e) {
            // Stopping the connection on any exception
            if (BluetoothMediator.isVerboseLog()) {
                LogHelper.log("Exited run() loop", LOG_TITLE);
            }

            try {
                if (mIsRunning && !mIsDisconnecting) {
                    stopConnection(true);
                }
            } catch (Exception er) {
                LogHelper.logError("Exception while stopping connection", LOG_TITLE, e);
            }
        } finally {
            mIsRunning = false;
        }
    }

    /**
     * Stops the connections and closes the socket gracefully. There is no need to notify the
     * Listener, as only Listener should call this method.
     */
    public void stopConnection() {
        stopConnection(false);
    }

    /**
     * Checks if the UDP socket is working and active.
     *
     * @return true, if the UDP socket is working and active.
     */
    public boolean isRunning() {
        return mIsRunning;
    }

    /**
     * Checks if the socket in the process of being closed.
     *
     * @return true, if the socket in the process of being closed.
     */
    public boolean isDisconnecting() {
        return mIsDisconnecting;
    }

    /**
     * Checks if the socket is closed.
     *
     * @return true, if the socket is closed.
     */
    public boolean isDisconnected() {
        return mIsDisconnected;
    }

    /**
     * Stops the connections and closes the socket gracefully.
     *
     * @param notifyListener Whether to notify the listener that the connection was terminated
     */
    private void stopConnection(boolean notifyListener) {
        mIsDisconnecting = true;
        if (mSocket != null && !mSocket.isClosed()) {
            mSocket.close();
        }

        mSocket = null;

        mIsRunning = false;
        mIsDisconnecting = false;
        mIsDisconnected = true;

        if (notifyListener) {
            mListener.onSocketFailure();
        }
    }

    /**
     * Finds a free random port. This is done by creating a ServerSocket with random port,
     * retrieving the port number, and closing the socket.
     *
     * @return the free port number
     * @throws IOException Signals that an I/O exception has occurred. This could only possibly happen if
     *                     there are no free ports.
     */
    private int findFreePort() throws IOException {
        ServerSocket socket = null;
        try {
            socket = new ServerSocket(0);
            return socket.getLocalPort();
        } finally {
            if (socket != null) {
                socket.close();
            }
        }
    }

}
