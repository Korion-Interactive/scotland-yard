/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.udp;

public interface IUdpSocketConnectionEventListener {

    /**
     * Called when a UDP datagram was received.
     *
     * @param buffer
     *            datagram byte array
     * @param bufferSize
     *            the size of the datagram
     * @param srcPort
     *            the port from which the datagram came from
     */
    void onRead(byte[] buffer, int bufferSize, int srcPort);

    /**
     * Called when some socket failure occurs. This generally means connection is interrupted.
     */
    void onSocketFailure();

}
