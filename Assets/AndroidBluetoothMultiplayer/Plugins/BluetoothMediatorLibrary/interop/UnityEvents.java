/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.interop;

import android.bluetooth.BluetoothDevice;

/**
 * A wrapper class for Unity callbacks.
 */
public final class UnityEvents {

    public static void listeningStarted() {
        UnityInterop.UnitySendMessage("JavaListeningStartedHandler", "1");
    }

    public static void listeningStopped() {
        UnityInterop.UnitySendMessage("JavaListeningStoppedHandler", "1");
    }

    public static void adapterEnabled() {
        UnityInterop.UnitySendMessage("JavaAdapterEnabledHandler", "1");
    }

    public static void adapterEnableFailed() {
        UnityInterop.UnitySendMessage("JavaAdapterEnableFailedHandler", "1");
    }

    public static void discoverabilityEnableFailed() {
        UnityInterop.UnitySendMessage("JavaDiscoverabilityEnableFailedHandler", "1");
    }

    public static void discoverabilityEnabled(int discoverabilityDuration) {
        UnityInterop.UnitySendMessage("JavaDiscoverabilityEnabledHandler", Integer.toString(discoverabilityDuration));
    }

    public static void adapterDisabled() {
        UnityInterop.UnitySendMessage("JavaAdapterDisabledHandler", "1");
    }

    public static void connectedToServer(BluetoothDevice device) {
        UnityInterop.UnitySendMessage("JavaConnectedToServerHandler", device.getAddress());
    }

    public static void connectionToServerFailed(BluetoothDevice device) {
        UnityInterop.UnitySendMessage("JavaConnectionToServerFailedHandler", device.getAddress());
    }

    public static void disconnectedFromServer(BluetoothDevice device) {
        UnityInterop.UnitySendMessage("JavaDisconnectedFromServerHandler", device.getAddress());
    }

    public static void clientConnected(BluetoothDevice device) {
        UnityInterop.UnitySendMessage("JavaClientConnectedHandler", device.getAddress());
    }

    public static void clientDisconnected(BluetoothDevice device) {
        UnityInterop.UnitySendMessage("JavaClientDisconnectedHandler", device.getAddress());
    }

    public static void devicePicked(BluetoothDevice device) {
        UnityInterop.UnitySendMessage("JavaDevicePickedHandler", device.getAddress());
    }

    public static void discoveryStarted() {
        UnityInterop.UnitySendMessage("JavaDiscoveryStartedHandler", "1");
    }

    public static void discoveryFinished() {
        UnityInterop.UnitySendMessage("JavaDiscoveryFinishedHandler", "1");
    }

    public static void deviceDiscovered(BluetoothDevice device) {
        UnityInterop.UnitySendMessage("JavaDeviceDiscoveredHandler", device.getAddress());
    }

}
