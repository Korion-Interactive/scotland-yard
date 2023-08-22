/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.mediator;

/**
 * Device picker dialog declarations. Taken from AOSP.
 */
interface IBluetoothDevicePicker {
    String EXTRA_NEED_AUTH = "android.bluetooth.devicepicker.extra.NEED_AUTH";
    String EXTRA_FILTER_TYPE = "android.bluetooth.devicepicker.extra.FILTER_TYPE";
    String EXTRA_LAUNCH_PACKAGE = "android.bluetooth.devicepicker.extra.LAUNCH_PACKAGE";
    String EXTRA_LAUNCH_CLASS = "android.bluetooth.devicepicker.extra.DEVICE_PICKER_LAUNCH_CLASS";

    String ACTION_DEVICE_SELECTED = "android.bluetooth.devicepicker.action.DEVICE_SELECTED";
    String ACTION_LAUNCH = "android.bluetooth.devicepicker.action.LAUNCH";

    /**
     * Ask device picker to show all kinds of BT devices
     */
    int FILTER_TYPE_ALL = 0;
    /**
     * Ask device picker to show BT devices that support AUDIO profiles
     */
    int FILTER_TYPE_AUDIO = 1;
    /**
     * Ask device picker to show BT devices that support Object Transfer
     */
    int FILTER_TYPE_TRANSFER = 2;
    /**
     * Ask device picker to show BT devices that support Personal Area Networking User (PANU)
     * profile
     */
    int FILTER_TYPE_PANU = 3;
    /**
     * Ask device picker to show BT devices that support Network Access Point (NAP) profile
     */
    int FILTER_TYPE_NAP = 4;
}