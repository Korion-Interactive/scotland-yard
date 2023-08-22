/*
 * Copyright (c) 2016, Lost Polygon. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 * 
 * This file is subject to Asset Store Terms of Service and EULA.
 * Please see http://unity3d.com/company/legal/as_terms for more information.
 */

package com.lostpolygon.unity.bluetoothmediator.mediator;

import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.LinkedHashSet;
import java.util.Set;
import java.util.UUID;

import android.Manifest.permission;
import android.annotation.TargetApi;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothClass;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.os.Build;
import android.os.Parcel;
import android.provider.Settings;
import android.text.TextUtils;

import com.lostpolygon.unity.bluetoothmediator.interop.LogHelper;
import com.lostpolygon.unity.bluetoothmediator.interop.UnityEvents;
import com.unity3d.player.UnityPlayer;

/**
 * The central class that provides convenient interface to the plugin. Unity must only use this one.
 */
public class BluetoothMediator implements IBluetoothMediatorCallback {
    private static final String LOG_TITLE = "BluetoothMediator";

    // Intent ID's
    private static final int REQUEST_ENABLE_BT = 1;
    private static final int REQUEST_ENABLE_DISCOVERABILITY = 2;

    /**
     * The BluetoothMediator singleton instance.
     */
    private static BluetoothMediator sInstance;

    /**
     * Whether the verbose logging is used.
     */
    private static boolean mIsVerboseLog = false;

    /**
     * The context in which plugin runs. Must usually target to main Unity activity.
     */
    private final Activity mContext;

    /**
     * Local Bluetooth adapter.
     */
    private BluetoothAdapter mAdapter = null;

    /**
     * Local Bluetooth adapter address.
     */
    private String mAdapterAddress = null;

    /**
     * Receives the Bluetooth adapter change events.
     */
    private BluetoothStateChangeReceiver mBluetoothStateChangeReceiver;

    /**
     * Receives the events of Bluetooth device picker Activity.
     */
    private BluetoothDevicePickerReceiver mBluetoothDevicePickerReceiver;

    /**
     * Receives the Bluetooth discovery events.
     */
    private BluetoothDiscoveryReceiver mBluetoothDiscoveryReceiver;

    /**
     * Current mediator mode.
     */
    private volatile MediatorMode mMediatorMode = MediatorMode.None;

    /**
     * Tunneling server instance.
     */
    private volatile BluetoothMediatorServer mServer;

    /**
     * Tunneling client instance.
     */
    private volatile BluetoothMediatorClient mClient;

    /**
     * The public MediatorSettings instance.
     */
    private MediatorSettings mSettings;

    /**
     * Whether the mediator can run on current device.
     */
    private boolean mIsMediatorAvailable = false;

    private boolean mIsDiscoveryStartedEventReceived = false;

    /**
     * Gets the BluetoothMediator singleton. Only used from Unity.
     *
     * @return the BluetoothMediator singleton
     */
    public synchronized static BluetoothMediator getSingleton() {
        if (sInstance == null) {
            try {
                Activity unityActivity = UnityPlayer.currentActivity;
                return getSingleton(unityActivity);
            } catch (Exception e) {
                LogHelper.logError("Exception while retrieving UnityPlayer.currentActivity", LOG_TITLE, e);
            }
        }

        return sInstance;
    }

    /**
     * Gets the BluetoothMediator singleton.
     *
     * @param context the context to run BluetoothMediator with
     * @return the BluetoothMediator singleton
     */
    public synchronized static BluetoothMediator getSingleton(Activity context) {
        if (sInstance == null)
            sInstance = new BluetoothMediator(context);

        return sInstance;
    }

    /**
     * Called from context Activity.onActivityResult().
     *
     * @param requestCode the request code
     * @param resultCode  the result code
     * @param data        the data
     */
    public synchronized static void onActivityResult(int requestCode, int resultCode, Intent data) {
        // Passing the data to the BluetoothMediator sInstance
        if (sInstance != null)
            sInstance.onActivityResultProcess(requestCode, resultCode, data);
    }

    /**
     * Sets the state of verbose logging.
     *
     * @param isEnabled the new state of verbose logging
     */
    public static void setVerboseLog(boolean isEnabled) {
        mIsVerboseLog = isEnabled;
    }

    /**
     * Checks the state of verbose logging.
     *
     * @return true, if verbose log is enabled
     */
    public static boolean isVerboseLog() {
        return mIsVerboseLog;
    }

    private boolean checkPermission(String permission) {
        PackageManager pm = mContext.getPackageManager();
        String packageName = mContext.getPackageName();

        if (pm.checkPermission(permission, packageName) == PackageManager.PERMISSION_DENIED) {
            LogHelper.logError(permission + " permission not granted. Check your AndroidManifest.xml", LOG_TITLE, null);
            return false;
        }
        return true;
    }

    /**
     * Instantiates a new Bluetooth mediator instance.
     *
     * @param context the context
     */
    private BluetoothMediator(Activity context) {
        LogHelper.logStartupMessage();
        mContext = context;

        if (context == null) {
            LogHelper.logError("context == null, make sure you are passing a valid Activity", LOG_TITLE, null);
            return;
        }

        // Check for Bluetooth permissions
        PackageManager pm = mContext.getPackageManager();

        final String packageName = mContext.getPackageName();

        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.S) {
            if (!checkPermission(permission.BLUETOOTH)) return;
            if (!checkPermission(permission.BLUETOOTH_ADMIN)) return;
        }

        logBluetoothDiscoveryRequiresLocationMessage(mContext);

        try {
            mAdapter = BluetoothAdapter.getDefaultAdapter();
        } catch (Exception e) {
            mAdapter = null;
        }

        // Creating default settings
        mSettings = new MediatorSettings();
        mSettings.usePacketSeparation = true;
        mSettings.uuid = UUID.fromString("8ce255c1-200a-11e0-ac64-0800200c9a66");

        // If the adapter is null, then Bluetooth is not supported
        if (mAdapter == null) {
            LogHelper.logError("Bluetooth is not available", LOG_TITLE, null);

            return;
        }

        mIsMediatorAvailable = true;

        // Discovery receiver
        mBluetoothDiscoveryReceiver = new BluetoothDiscoveryReceiver();

        // Adapter state change receiver
        mBluetoothStateChangeReceiver = new BluetoothStateChangeReceiver();
        IntentFilter bluetoothStateChangedFilter = new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED);
        mContext.registerReceiver(mBluetoothStateChangeReceiver, bluetoothStateChangedFilter);

        // Device picker receiver
        mBluetoothDevicePickerReceiver = new BluetoothDevicePickerReceiver();
        IntentFilter deviceSelectedFilter = new IntentFilter();
        deviceSelectedFilter.addAction(IBluetoothDevicePicker.ACTION_DEVICE_SELECTED);

        mContext.registerReceiver(mBluetoothDevicePickerReceiver, deviceSelectedFilter);
    }

    /**
     * @see java.lang.Object#finalize()
     */
    protected void finalize() throws Throwable {
        try {
            super.finalize();
        } finally {
            if (mIsMediatorAvailable) {
                stop();
                dispose();
            }
        }
    }

    /**
     * Sets the Bluetooth service UUID.
     *
     * @param uuid the UUID
     * @return true, if successful
     */
    public boolean initUuid(String uuid) {
        if (!mIsMediatorAvailable)
            return false;

        try {
            mSettings.uuid = UUID.fromString(uuid);
        } catch (IllegalArgumentException e) {
            return false;
        }

        return true;
    }

    /**
     * Start a Bluetooth server, listening for incoming Bluetooth connections.
     *
     * @param remoteHost the remote hostname for UDP tunneling
     * @param remotePort the remote port for UDP tunneling
     * @return true, if successful
     */
    public synchronized boolean startServer(String remoteHost, int remotePort) {
        if (!mIsMediatorAvailable || !isBluetoothEnabled())
            return false;

        stop();
        if (mServer != null)
            mServer.stop();

        mMediatorMode = MediatorMode.Server;

        mSettings.remoteHost = remoteHost;
        mSettings.remotePort = remotePort;

        mServer = new BluetoothMediatorServer(this);
        mServer.start();

        return true;
    }

    /**
     * Start the Bluetooth client and try to connect to the server.
     *
     * @param remoteHost        the remote hostname for UDP tunneling
     * @param remotePort        the remote port for UDP tunneling
     * @param hostDeviceAddress the host BluetoothDevice address
     * @return true, if successful
     */
    public synchronized boolean startClient(String remoteHost, int remotePort, String hostDeviceAddress) {
        if (!mIsMediatorAvailable || !isBluetoothEnabled())
            return false;

        stop();
        if (mClient != null)
            mClient.stop();

        mMediatorMode = MediatorMode.Client;

        mSettings.remoteHost = remoteHost;
        mSettings.remotePort = remotePort;
        mSettings.hostDevice = mAdapter.getRemoteDevice(hostDeviceAddress);

        mClient = new BluetoothMediatorClient(this);
        mClient.start();

        return true;
    }

    /**
     * Stop all connectivity.
     *
     * @return true, if successful
     */
    public synchronized boolean stop() {
        if (!mIsMediatorAvailable)
            return false;

        try {
            if (mClient != null) {
                mClient.stop();
                mClient = null;
            }

            if (mServer != null) {
                mServer.stop();
                mServer = null;
            }
        } finally {
            mMediatorMode = MediatorMode.None;

            stopDiscovery(false);
        }

        return true;
    }

    /**
     * Gets the current mediator mode.
     *
     * @return the current mediator mode
     */
    public byte getCurrentMode() {
        switch (mMediatorMode) {
            case Client:
                return 2;
            case Server:
                return 1;
            default:
                return 0;
        }
    }

    /**
     * Sets whether to use raw packets.
     *
     * @param isEnabled whether to use raw packets
     * @return true, if successful
     */
    public boolean setRawPackets(boolean isEnabled) {
        if (mMediatorMode != MediatorMode.None)
            return false;

        mSettings.usePacketSeparation = !isEnabled;

        return true;
    }

    /**
     * Gets the BluetoothAdapter instance.
     *
     * @see IBluetoothMediatorCallback#getAdapter()
     */
    @Override
    public BluetoothAdapter getAdapter() {
        return mAdapter;
    }

    /**
     * Gets the mediator settings.
     *
     * @see IBluetoothMediatorCallback#getSettings()
     */
    @Override
    public MediatorSettings getSettings() {
        return mSettings;
    }

    /**
     * Called from within client/server to notify that connection has been stopped.
     */
    @Override
    public void onMediatorStopped() {
        mMediatorMode = MediatorMode.None;
    }

    /**
     * Start listening for new incoming connections, if in server mode.
     *
     * @return true, if successful
     */
    public synchronized boolean startListening() {
        if (!mIsMediatorAvailable)
            return false;

        if (mMediatorMode == MediatorMode.Server && mServer != null) {
            mServer.startListening();
            return true;
        }

        return false;
    }

    /**
     * Stop listening for new incoming connections, if in server mode.
     *
     * @return true, if successful
     */
    public synchronized boolean stopListening() {
        if (!mIsMediatorAvailable)
            return false;

        if (mMediatorMode == MediatorMode.Server && mServer != null) {
            mServer.stopListening();
            return true;
        }

        return false;
    }

    /**
     * Open a dialog asking user to make device discoverable on Bluetooth.
     *
     * @param discoverableDuration the desired duration of discoverability (in seconds)
     * @return true, if successful
     */
    public boolean requestEnableDiscoverability(int discoverableDuration) {
        if (!mIsMediatorAvailable)
            return false;

        if (mAdapter.getScanMode() != BluetoothAdapter.SCAN_MODE_CONNECTABLE_DISCOVERABLE) {
            Intent discoverableIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_DISCOVERABLE);
            discoverableIntent.putExtra(BluetoothAdapter.EXTRA_DISCOVERABLE_DURATION, discoverableDuration);
            mContext.startActivityForResult(discoverableIntent, BluetoothMediator.REQUEST_ENABLE_DISCOVERABILITY);

            return true;
        }

        return false;
    }

    /**
     * Checks Bluetooth availability.
     *
     * @return true, if Bluetooth is available on the device
     */
    public boolean isBluetoothAvailable() {
        if (!mIsMediatorAvailable)
            return false;

        return mAdapter != null;
    }

    /**
     * Request enabling of the Bluetooth adapter.
     *
     * @return true, if successful
     */
    public boolean requestEnableBluetooth() {
        if (!mIsMediatorAvailable || isBluetoothEnabled())
            return false;

        Intent enableIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
        mContext.startActivityForResult(enableIntent, REQUEST_ENABLE_BT);

        return true;
    }

    /**
     * Enable the Bluetooth adapter, if possible.
     *
     * @return true, if successful
     */
    public boolean enableBluetooth() {
        if (!mIsMediatorAvailable)
            return false;

        return mAdapter.enable();
    }

    /**
     * Disable the Bluetooth adapter, if possible.
     *
     * @return true, if successful
     */
    public boolean disableBluetooth() {
        if (!mIsMediatorAvailable)
            return false;

        return mAdapter.disable();
    }

    /**
     * Checks the Bluetooth state.
     *
     * @return true, if Bluetooth is currently enabled and ready for use.
     */
    public boolean isBluetoothEnabled() {
        if (!mIsMediatorAvailable)
            return false;

        return mAdapter.isEnabled();
    }

    /**
     * Show the Bluetooth device picker dialog.
     *
     * @return true, if successful
     */
    public boolean showDeviceList(boolean showAllDeviceTypes) {
        if (!mIsMediatorAvailable || !isBluetoothEnabled())
            return false;

        // Launch the DeviceListActivity to see devices and do scan
        int deviceType = showAllDeviceTypes ? IBluetoothDevicePicker.FILTER_TYPE_ALL : IBluetoothDevicePicker.FILTER_TYPE_TRANSFER;
        final Intent intent =
            new Intent(IBluetoothDevicePicker.ACTION_LAUNCH)
            .putExtra(IBluetoothDevicePicker.EXTRA_NEED_AUTH, false)
            .putExtra(IBluetoothDevicePicker.EXTRA_FILTER_TYPE, deviceType)
            .addFlags(Intent.FLAG_ACTIVITY_EXCLUDE_FROM_RECENTS);

        mContext.startActivity(intent);

        return true;
    }

    /**
     * The array of bonded (paired) Bluetooth devices.
     */
    Set<BluetoothDevice> mBondedDevices = new LinkedHashSet<>();

    /**
     * The array of bonded (paired) Bluetooth devices and Bluetooth devices discovered during
     * current discovery session.
     */
    Set<BluetoothDevice> mDiscoveredDevices = new LinkedHashSet<>();

    /**
     * The array of Bluetooth devices discovered during current discovery session.
     */
    Set<BluetoothDevice> mNewDiscoveredDevices = new LinkedHashSet<>();

    /**
     * Whether the discovery was initiated by {@code startDiscovery}.
     */
    boolean mIsDiscoveryStartedByUser = false;

    /**
     * Checks if Bluetooth device discovery is going on.
     *
     * @return true, if discovering
     */
    public boolean isDiscovering() {
        if (!mIsMediatorAvailable)
            return false;

        return mAdapter.isDiscovering();
    }

    /**
     * Checks if Bluetooth device is discoverable by other devices.
     *
     * @return true, if discoverable
     */
    public boolean isDiscoverable() {
        if (!mIsMediatorAvailable)
            return false;

        return mAdapter.getScanMode() == BluetoothAdapter.SCAN_MODE_CONNECTABLE_DISCOVERABLE;
    }

    /**
     * Start discovery of nearby discoverable Bluetooth devices.
     *
     * @return true, if successful
     */
    public boolean startDiscovery() {
        if (!mIsMediatorAvailable || !isBluetoothEnabled())
            return false;

        // API 23 requires Location to be enabled to do Bluetooth discovery
        logBluetoothDiscoveryRequiresLocationMessage(mContext);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M && Build.VERSION.SDK_INT < Build.VERSION_CODES.S && !isLocationEnabled(mContext)) {
            LogHelper.logError(
                "On Android 6.0 up to Android 11, Location must be enabled in order to do Bluetooth discovery. " +
                "Please enable Location in your device settings.",
                LOG_TITLE, null);

            return false;
        }

        stopDiscovery(false);

        // Start the event receiver
        IntentFilter filter = new IntentFilter();
        filter.addAction(BluetoothDevice.ACTION_FOUND);
        filter.addAction(BluetoothAdapter.ACTION_DISCOVERY_STARTED);
        filter.addAction(BluetoothAdapter.ACTION_DISCOVERY_FINISHED);

        mContext.registerReceiver(mBluetoothDiscoveryReceiver, filter);

        return mAdapter.startDiscovery();
    }

    /**
     * Stop discovery of nearby discoverable Bluetooth devices.
     *
     * @return true, if successful
     */
    public boolean stopDiscovery() {
        return stopDiscovery(false);
    }

    /**
     * Returns a {@code BluetoothDevice instance}, given the Bluetooth address.
     * @param bluetoothDeviceAddress the Bluetooth address
     * @return
     */
    public BluetoothDevice getBluetoothDeviceFromAddress(String bluetoothDeviceAddress) {
        if (!mIsMediatorAvailable || !isBluetoothEnabled())
            return null;

        return mAdapter.getRemoteDevice(bluetoothDeviceAddress);
    }

    /**
     * Stop discovery of nearby discoverable Bluetooth devices.
     *
     * @param force whether to notify Unity
     * @return true, if successful
     */
    private boolean stopDiscovery(boolean force) {
        if (!mIsMediatorAvailable)
            return false;

        try {
            mContext.unregisterReceiver(mBluetoothDiscoveryReceiver);
        } catch (IllegalArgumentException e) {
            // Just ignore that, as we won't need that receiver anyway
        }

        mIsDiscoveryStartedByUser = false;
        mIsDiscoveryStartedEventReceived = false;

        boolean wasDiscovering = mAdapter.isDiscovering();
        boolean cancelResult = mAdapter.cancelDiscovery();

        if (force || (wasDiscovering && cancelResult)) {
            UnityEvents.discoveryFinished();
        }

        return cancelResult;
    }

    /**
     * Gets bonded Bluetooth devices.
     *
     * @return the array of bonded Bluetooth devices.
     */
    public Set<BluetoothDevice> getBondedDevices() {
        if (!mIsMediatorAvailable || !isBluetoothEnabled())
            return null;

        return mAdapter.getBondedDevices();
    }

    /**
     * Gets Bluetooth devices discovered during current discovery session.
     *
     * @return the array of Bluetooth devices discovered during current discovery session.
     */
    public Set<BluetoothDevice> getNewDiscoveredDevices() {
        if (!mIsMediatorAvailable || !isBluetoothEnabled())
            return null;

        return mNewDiscoveredDevices;
    }

    /**
     * Gets bonded (paired) devices and devices discovered during the ongoing discovery session.
     *
     * @return the array of bonded (paired) devices and devices discovered during the ongoing
     * discovery session
     */
    public Set<BluetoothDevice> getDiscoveredDevices() {
        if (!mIsMediatorAvailable || !isBluetoothEnabled())
            return null;

        mDiscoveredDevices.addAll(mAdapter.getBondedDevices());
        return mDiscoveredDevices;
    }

    /**
     * Called from context Activity.onActivityResult().
     *
     * @param requestCode the request code
     * @param resultCode  the result code
     * @param data        the data
     */
    public void onActivityResultProcess(int requestCode, int resultCode, Intent data) {
        switch (requestCode) {
            case BluetoothMediator.REQUEST_ENABLE_BT:
                // When the request to enable Bluetooth returns
                if (resultCode != Activity.RESULT_OK) {
                    // User did not enable Bluetooth or an error occurred
                    UnityEvents.adapterEnableFailed();
                }
                break;
            case BluetoothMediator.REQUEST_ENABLE_DISCOVERABILITY:
                // When the request to enable Bluetooth discoverability returns
                if (resultCode == Activity.RESULT_CANCELED) {
                    // User did not authorized enabling discoverability or an error occurred
                    UnityEvents.discoverabilityEnableFailed();
                } else {
                    // User has authorized the discoverability.
                    // resultCode contains the duration (in seconds) of discoverability
                    UnityEvents.discoverabilityEnabled(resultCode);
                }
                break;
        }
    }

    /**
     * Gets the current Bluetooth device info.
     *
     * @return current Bluetooth device info
     */
    public FakeBluetoothDevice getCurrentDevice() {
        if (!mIsMediatorAvailable || !isBluetoothEnabled())
            return null;

        // Set the corresponding class if the device is a tablet
        int deviceClass =
            isTablet() ?
                BluetoothClass.Device.COMPUTER_HANDHELD_PC_PDA :
                BluetoothClass.Device.PHONE_SMART;

        // An ugly hack to create a BluetoothClass
        final Parcel bluetoothClassParcel = Parcel.obtain();
        bluetoothClassParcel.writeInt(deviceClass);
        bluetoothClassParcel.setDataPosition(0);
        BluetoothClass bluetoothClass = BluetoothClass.CREATOR.createFromParcel(bluetoothClassParcel);

        return new FakeBluetoothDevice(
            mAdapter.getName(),
            getAdapterAddress(),
            BluetoothDevice.BOND_BONDED,
            bluetoothClass
        );
    }

    private boolean isTablet() {
        int screenLayout = mContext.getResources().getConfiguration().screenLayout;
        boolean xlarge = ((screenLayout & Configuration.SCREENLAYOUT_SIZE_MASK) == Configuration.SCREENLAYOUT_SIZE_XLARGE);
        boolean large = ((screenLayout & Configuration.SCREENLAYOUT_SIZE_MASK) == Configuration.SCREENLAYOUT_SIZE_LARGE);
        return xlarge || large;
    }

    private String getAdapterAddress() {
        if (mAdapterAddress != null)
            return mAdapterAddress;

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            try {
                // Method 1
                mAdapterAddress = Settings.Secure.getString(mContext.getContentResolver(), "bluetooth_address");
                if (isVerboseLog())
                    LogHelper.log("BT MAC method1: " + mAdapterAddress, LOG_TITLE);

                // Method 2
                if (TextUtils.isEmpty(mAdapterAddress)) {
                    final Field bluetoothManagerServiceField = BluetoothAdapter.class.getDeclaredField("mService");
                    bluetoothManagerServiceField.setAccessible(true);
                    Object bluetoothManagerService = bluetoothManagerServiceField.get(mAdapter);
                    if (bluetoothManagerService != null) {
                        final Method bluetoothManagerServiceGetAddressMethod = bluetoothManagerService.getClass().getDeclaredMethod("getAddress");
                        bluetoothManagerServiceGetAddressMethod.setAccessible(true);
                        Object address = bluetoothManagerServiceGetAddressMethod.invoke(bluetoothManagerService);
                        if (address != null && address instanceof String) {
                            mAdapterAddress = (String) address;
                            if (isVerboseLog()) {
                                LogHelper.log("BT MAC method2: " + mAdapterAddress, LOG_TITLE);
                            }
                        }
                    } else {
                        LogHelper.logWarning("Couldn't find bluetoothManagerService", LOG_TITLE);
                    }
                }
            } catch (Throwable e) {
                e.printStackTrace();
            }
        }

        // Fallback to standard method. This will produce fake MAC on API >= 23
        if (TextUtils.isEmpty(mAdapterAddress)) {
            mAdapterAddress = mAdapter.getAddress();
        }

        return mAdapterAddress;
    }

    // Local events

    /**
     * Called when Bluetooth adapter was disabled.
     */
    private void onBluetoothDisabled() {
        if (!mIsMediatorAvailable)
            return;

        stop();
    }

    /**
     * Called when Bluetooth adapter was enabled.
     */
    private void onBluetoothEnabled() {
        if (!mIsMediatorAvailable)
            return;
    }

    private void dispose() {
        if (mAdapter != null && mAdapter.isDiscovering())
            mAdapter.cancelDiscovery();

        mContext.unregisterReceiver(mBluetoothStateChangeReceiver);
        mContext.unregisterReceiver(mBluetoothDiscoveryReceiver);
        mContext.unregisterReceiver(mBluetoothDevicePickerReceiver);
    }

    @TargetApi(Build.VERSION_CODES.KITKAT)
    private static boolean isLocationEnabled(Context context) {
        int locationMode = 0;
        String locationProviders;

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
            try {
                locationMode = Settings.Secure.getInt(context.getContentResolver(), Settings.Secure.LOCATION_MODE);
            } catch (Settings.SettingNotFoundException e) {
                e.printStackTrace();
            }

            return locationMode != Settings.Secure.LOCATION_MODE_OFF;
        } else {
            //noinspection deprecation
            locationProviders = Settings.Secure.getString(context.getContentResolver(), Settings.Secure.LOCATION_PROVIDERS_ALLOWED);
            return !TextUtils.isEmpty(locationProviders);
        }
    }

    private static void logBluetoothDiscoveryRequiresLocationMessage(Context context) {
        PackageManager pm = context.getPackageManager();
        String packageName = context.getPackageName();
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M && Build.VERSION.SDK_INT < Build.VERSION_CODES.S) {
            if (pm.checkPermission(permission.ACCESS_FINE_LOCATION, packageName) == PackageManager.PERMISSION_DENIED &&
                pm.checkPermission(permission.ACCESS_COARSE_LOCATION, packageName) == PackageManager.PERMISSION_DENIED) {
                LogHelper.logError(
                    "ACCESS_FINE_LOCATION or ACCESS_COARSE_LOCATION permission not granted. " +
                        "At least one of them must be granted for Bluetooth discovery function to work on Android 6.0 and newer. " +
                        "Check your application permission settings and AndroidManifest.xml.",
                    LOG_TITLE,
                    null);
            }
        }
    }

    /**
     * Receives the events of Bluetooth device picker Activity.
     */
    private class BluetoothDevicePickerReceiver extends BroadcastReceiver implements IBluetoothDevicePicker {

        /*
         * (non-Javadoc)
         * @see android.content.BroadcastReceiver#onReceive(android.content.Context,
         * android.content.Intent)
         */
        @Override
        public void onReceive(Context context, Intent intent) {
            if (ACTION_DEVICE_SELECTED.equals(intent.getAction())) {
                // context.unregisterReceiver(this);
                BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
                UnityEvents.devicePicked(device);
            }
        }
    }

    /**
     * Receives the Bluetooth adapter change events.
     */
    private class BluetoothStateChangeReceiver extends BroadcastReceiver {

        /*
         * (non-Javadoc)
         * @see android.content.BroadcastReceiver#onReceive(android.content.Context,
         * android.content.Intent)
         */
        @Override
        public void onReceive(Context context, Intent intent) {
            switch (intent.getAction()) {
                case BluetoothAdapter.ACTION_STATE_CHANGED:
                    final int state = intent.getIntExtra(BluetoothAdapter.EXTRA_STATE, BluetoothAdapter.ERROR);
                    switch (state) {
                        case BluetoothAdapter.STATE_TURNING_OFF:
                        case BluetoothAdapter.ERROR:
//                            if (!mIsDiscoveryStartedEventReceived) {
//                                LogHelper.logError("Got STATE_TURNING_OFF or ERROR while mIsDiscoveryStartedEventReceived was still false", this, null);
//                            }
                            UnityEvents.adapterDisabled();
                            onBluetoothDisabled();
                            break;
                        case BluetoothAdapter.STATE_ON:
                            UnityEvents.adapterEnabled();
                            onBluetoothEnabled();
                            break;
                    }
                    break;
            }
        }
    }

    /**
     * The BroadcastReceiver that listens for device discovery events.
     */
    private class BluetoothDiscoveryReceiver extends BroadcastReceiver{
        @Override
        public void onReceive(Context context, Intent intent) {
            if (!isBluetoothAvailable())
                return;

            LogHelper.logWarning(intent.getAction(), LOG_TITLE);
            // Sending the events to Unity
            switch (intent.getAction()) {
                case BluetoothDevice.ACTION_FOUND:
                    BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
                    boolean addResultDiscovered = mDiscoveredDevices.add(device);
                    boolean addResultNewDiscovered = mNewDiscoveredDevices.add(device);
                    boolean isNewDevice = addResultDiscovered || addResultNewDiscovered;

                    if (isNewDevice) {
                        UnityEvents.deviceDiscovered(device);
                    }
                    break;
                case BluetoothAdapter.ACTION_DISCOVERY_STARTED:
                    if (!mIsDiscoveryStartedEventReceived) {
                        mIsDiscoveryStartedEventReceived = true;

                        mBondedDevices = new LinkedHashSet<>(mAdapter.getBondedDevices());
                        mDiscoveredDevices = new LinkedHashSet<>(mBondedDevices);
                        mNewDiscoveredDevices = new LinkedHashSet<>();

                        mIsDiscoveryStartedByUser = true;
                        UnityEvents.discoveryStarted();
                    }
                    break;
                case BluetoothAdapter.ACTION_DISCOVERY_FINISHED:
                    stopDiscovery(true);
                    // UnityEvents.onBluetoothDiscoveryFinished();
                    break;
            }
        }
    }

    /**
     * Holds some public settings that can be safely passed everywhere.
     */
    public final class MediatorSettings {

        /**
         * Remote hostname for UDP tunneling.
         */
        public String remoteHost;

        /**
         * Remote port for UDP tunneling.
         */
        public int remotePort;

        /**
         * Bluetooth connection UUID. Must be unique for each game.
         */
        public UUID uuid;

        /**
         * The server device client has to connect to.
         */
        public BluetoothDevice hostDevice;

        /**
         * Must always be set to true when working with Unity networking. Set to false to use raw
         * data packets.
         */
        public boolean usePacketSeparation;
    }

    /**
     * Defines the possible states of the mediator.
     */
    public enum MediatorMode {
        None,
        Server,
        Client
    }

    /**
     * Used to create fake {@code BluetoothDevice} instances, and has the same interface.
     * {@see BluetoothDevice}
     */
    public final class FakeBluetoothDevice {
        private final String mName;
        private final String mAddress;
        private final int mBondState;
        private final BluetoothClass mBluetoothClass;

        private FakeBluetoothDevice(String name, String address, int bondState, BluetoothClass bluetoothClass) {
            mName = name;
            mAddress = address;
            mBondState = bondState;
            mBluetoothClass = bluetoothClass;
        }

        public String getName() {
            return mName;
        }

        public String getAddress() {
            return mAddress;
        }

        public int getBondState() {
            return mBondState;
        }

        public BluetoothClass getBluetoothClass() {
            return mBluetoothClass;
        }
    }
}
