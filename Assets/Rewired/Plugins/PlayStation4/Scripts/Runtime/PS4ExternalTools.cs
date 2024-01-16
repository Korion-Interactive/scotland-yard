#if UNITY_PS4 && !UNITY_EDITOR

// Copyright (c) 2022 Augie R. Maddox, Guavaman Enterprises. All rights reserved.
#pragma warning disable 0219
#pragma warning disable 0618
#pragma warning disable 0649
#pragma warning disable 0067

using UnityEngine;

namespace Rewired.Utils.Platforms.PS4 {

    public sealed class PS4ExternalTools : IPS4ExternalTools {
    
        public PS4ExternalTools() {
        }

        public Vector3 PS4Input_GetLastAcceleration(int id) {
            return UnityEngine.PS4.PS4Input.PadGetLastAcceleration(id);
        }

        public Vector3 PS4Input_GetLastGyro(int id) {
            return UnityEngine.PS4.PS4Input.PadGetLastGyro(id);
        }

        public Vector4 PS4Input_GetLastOrientation(int id) {
            return UnityEngine.PS4.PS4Input.PadGetLastOrientation(id);
        }

        public void PS4Input_GetLastTouchData(int id, out int touchNum, out int touch0x, out int touch0y, out int touch0id, out int touch1x, out int touch1y, out int touch1id) {
            UnityEngine.PS4.PS4Input.GetLastTouchData(id, out touchNum, out touch0x, out touch0y, out touch0id, out touch1x, out touch1y, out touch1id);
        }

        public void PS4Input_GetPadControllerInformation(int id, out float touchpixelDensity, out int touchResolutionX, out int touchResolutionY, out int analogDeadZoneLeft, out int analogDeadZoneright, out int connectionType) {
            UnityEngine.PS4.PS4Input.ConnectionType connectionTypeEnum;
            UnityEngine.PS4.PS4Input.GetPadControllerInformation(id, out touchpixelDensity, out touchResolutionX, out touchResolutionY, out analogDeadZoneLeft, out analogDeadZoneright, out connectionTypeEnum);
            connectionType = (int)connectionTypeEnum;
        }

        public void PS4Input_PadSetMotionSensorState(int id, bool bEnable) {
            UnityEngine.PS4.PS4Input.PadSetMotionSensorState(id, bEnable);
        }

        public void PS4Input_PadSetTiltCorrectionState(int id, bool bEnable) {
            UnityEngine.PS4.PS4Input.PadSetTiltCorrectionState(id, bEnable);
        }

        public void PS4Input_PadSetAngularVelocityDeadbandState(int id, bool bEnable) {
            UnityEngine.PS4.PS4Input.PadSetAngularVelocityDeadbandState(id, bEnable);
        }

        public void PS4Input_PadSetLightBar(int id, int red, int green, int blue) {
            UnityEngine.PS4.PS4Input.PadSetLightBar(id, red, green, blue);
        }

        public void PS4Input_PadResetLightBar(int id) {
            UnityEngine.PS4.PS4Input.PadResetLightBar(id);
        }

        public void PS4Input_PadSetVibration(int id, int largeMotor, int smallMotor) {
            UnityEngine.PS4.PS4Input.PadSetVibration(id, largeMotor, smallMotor);
        }

        public void PS4Input_PadResetOrientation(int id) {
            UnityEngine.PS4.PS4Input.PadResetOrientation(id);
        }

        public bool PS4Input_PadIsConnected(int id) {
            return UnityEngine.PS4.PS4Input.PadIsConnected(id);
        }

        public void PS4Input_GetUsersDetails(int slot, object loggedInUser) {
            if(loggedInUser == null) throw new System.ArgumentNullException("loggedInUser");
            UnityEngine.PS4.PS4Input.LoggedInUser user = UnityEngine.PS4.PS4Input.GetUsersDetails(slot);
            Rewired.Platforms.PS4.Internal.LoggedInUser retUser = loggedInUser as Rewired.Platforms.PS4.Internal.LoggedInUser;
            if(retUser == null) throw new System.ArgumentException("loggedInUser is not the correct type.");

            retUser.status = user.status;
            retUser.primaryUser = user.primaryUser;
            retUser.userId = user.userId;
            retUser.color = user.color;
            retUser.userName = user.userName;
            retUser.padHandle = user.padHandle;
            retUser.move0Handle = user.move0Handle;
            retUser.move1Handle = user.move1Handle;
            retUser.aimHandle = user.aimHandle;
        }

        public int PS4Input_GetDeviceClassForHandle(int handle) {
            return (int)UnityEngine.PS4.PS4Input.GetDeviceClassForHandle(handle);
        }

        public string PS4Input_GetDeviceClassString(int intValue) {
            return ((UnityEngine.PS4.PS4Input.DeviceClass)intValue).ToString();
        }

        public int PS4Input_PadGetUsersHandles2(int maxControllers, int[] handles) {
            return UnityEngine.PS4.PS4Input.PadGetUsersHandles2(maxControllers, handles);
        }

        private readonly UnityEngine.PS4.PS4Input.ControllerInformation _controllerInformation = new UnityEngine.PS4.PS4Input.ControllerInformation();

        public void PS4Input_GetSpecialControllerInformation(int id, int padIndex, object controllerInformation) {
            if(controllerInformation == null) throw new System.ArgumentNullException("controllerInformation");
            Rewired.Platforms.PS4.Internal.ControllerInformation tControllerInformation = controllerInformation as Rewired.Platforms.PS4.Internal.ControllerInformation;
            if(tControllerInformation == null) throw new System.ArgumentException("controllerInformation is not the correct type.");
            UnityEngine.PS4.PS4Input.ControllerInformation c = _controllerInformation;
            UnityEngine.PS4.PS4Input.GetSpecialControllerInformation(id, padIndex, ref c);
            tControllerInformation.padControllerInformation.touchPadInfo.pixelDensity = c.padControllerInformation.touchPadInfo.pixelDensity;
            tControllerInformation.padControllerInformation.touchPadInfo.resolutionX = c.padControllerInformation.touchPadInfo.resolutionX;
            tControllerInformation.padControllerInformation.touchPadInfo.resolutionY = c.padControllerInformation.touchPadInfo.resolutionY;
            tControllerInformation.padControllerInformation.stickInfo.deadZoneLeft = c.padControllerInformation.stickInfo.deadZoneLeft;
            tControllerInformation.padControllerInformation.stickInfo.deadZoneRight = c.padControllerInformation.stickInfo.deadZoneRight;
            tControllerInformation.padControllerInformation.connectionType = c.padControllerInformation.connectionType;
            tControllerInformation.padControllerInformation.connectedCount = c.padControllerInformation.connectedCount;
            tControllerInformation.padControllerInformation.connected = c.padControllerInformation.connected;
            tControllerInformation.padControllerInformation.deviceClass = (int)c.padControllerInformation.deviceClass;
            tControllerInformation.padDeviceClassExtendedInformation.deviceClass = (int)c.padDeviceClassExtendedInformation.deviceClass;
            tControllerInformation.padDeviceClassExtendedInformation.capability = c.padDeviceClassExtendedInformation.capability;
            tControllerInformation.padDeviceClassExtendedInformation.quantityOfSelectorSwitch = c.padDeviceClassExtendedInformation.quantityOfSelectorSwitch;
            tControllerInformation.padDeviceClassExtendedInformation.maxPhysicalWheelAngle = c.padDeviceClassExtendedInformation.maxPhysicalWheelAngle;
        }

        public Vector3 PS4Input_SpecialGetLastAcceleration(int id) {
            return UnityEngine.PS4.PS4Input.SpecialGetLastAcceleration(id);
        }

        public Vector3 PS4Input_SpecialGetLastGyro(int id) {
            return UnityEngine.PS4.PS4Input.SpecialGetLastGyro(id);
        }

        public Vector4 PS4Input_SpecialGetLastOrientation(int id) {
            return UnityEngine.PS4.PS4Input.SpecialGetLastOrientation(id);
        }

        public int PS4Input_SpecialGetUsersHandles(int maxNumberControllers, int[] handles) {
            return UnityEngine.PS4.PS4Input.SpecialGetUsersHandles(maxNumberControllers, handles);
        }

        public int PS4Input_SpecialGetUsersHandles2(int maxNumberControllers, int[] handles) {
            return UnityEngine.PS4.PS4Input.SpecialGetUsersHandles2(maxNumberControllers, handles);
        }

        public bool PS4Input_SpecialIsConnected(int id) {
            return UnityEngine.PS4.PS4Input.SpecialIsConnected(id);
        }

        public void PS4Input_SpecialResetLightSphere(int id) {
            UnityEngine.PS4.PS4Input.SpecialResetLightSphere(id);
        }

        public void PS4Input_SpecialResetOrientation(int id) {
            UnityEngine.PS4.PS4Input.SpecialResetOrientation(id);
        }

        public void PS4Input_SpecialSetAngularVelocityDeadbandState(int id, bool bEnable) {
            UnityEngine.PS4.PS4Input.SpecialSetAngularVelocityDeadbandState(id, bEnable);
        }

        public void PS4Input_SpecialSetLightSphere(int id, int red, int green, int blue) {
            UnityEngine.PS4.PS4Input.SpecialSetLightSphere(id, red, green, blue);
        }

        public void PS4Input_SpecialSetMotionSensorState(int id, bool bEnable) {
            UnityEngine.PS4.PS4Input.SpecialSetMotionSensorState(id, bEnable);
        }

        public void PS4Input_SpecialSetTiltCorrectionState(int id, bool bEnable) {
            UnityEngine.PS4.PS4Input.SpecialSetTiltCorrectionState(id,  bEnable);
        }

        public void PS4Input_SpecialSetVibration(int id, int largeMotor, int smallMotor) {
            UnityEngine.PS4.PS4Input.SpecialSetVibration(id, largeMotor, smallMotor);
        }

        // Aim

        public Vector3 PS4Input_AimGetLastAcceleration(int id) {
            return UnityEngine.PS4.PS4Input.AimGetLastAcceleration(id);
        }

        public Vector3 PS4Input_AimGetLastGyro(int id) {
            return UnityEngine.PS4.PS4Input.AimGetLastGyro(id);
        }

        public Vector4 PS4Input_AimGetLastOrientation(int id) {
            return UnityEngine.PS4.PS4Input.AimGetLastOrientation(id);
        }

        public int PS4Input_AimGetUsersHandles(int maxNumberControllers, int[] handles) {
            return UnityEngine.PS4.PS4Input.AimGetUsersHandles(maxNumberControllers, handles);
        }

        public int PS4Input_AimGetUsersHandles2(int maxNumberControllers, int[] handles) {
            return UnityEngine.PS4.PS4Input.AimGetUsersHandles2(maxNumberControllers, handles);
        }

        public bool PS4Input_AimIsConnected(int id) {
            return UnityEngine.PS4.PS4Input.AimIsConnected(id);
        }

        public void PS4Input_AimResetLightSphere(int id) {
            UnityEngine.PS4.PS4Input.AimResetLightSphere(id);
        }

        public void PS4Input_AimResetOrientation(int id) {
            UnityEngine.PS4.PS4Input.AimResetOrientation(id);
        }

        public void PS4Input_AimSetAngularVelocityDeadbandState(int id, bool bEnable) {
            UnityEngine.PS4.PS4Input.AimSetAngularVelocityDeadbandState(id, bEnable);
        }

        public void PS4Input_AimSetLightSphere(int id, int red, int green, int blue) {
            UnityEngine.PS4.PS4Input.AimSetLightSphere(id, red, green, blue);
        }

        public void PS4Input_AimSetMotionSensorState(int id, bool bEnable) {
            UnityEngine.PS4.PS4Input.AimSetMotionSensorState(id, bEnable);
        }

        public void PS4Input_AimSetTiltCorrectionState(int id, bool bEnable) {
            UnityEngine.PS4.PS4Input.AimSetTiltCorrectionState(id, bEnable);
        }

        public void PS4Input_AimSetVibration(int id, int largeMotor, int smallMotor) {
            UnityEngine.PS4.PS4Input.AimSetVibration(id, largeMotor, smallMotor);
        }

        // Move

        public Vector3 PS4Input_GetLastMoveAcceleration(int id, int index) {
            return UnityEngine.PS4.PS4Input.GetLastMoveAcceleration(id, index);
        }

        public Vector3 PS4Input_GetLastMoveGyro(int id, int index) {
            return UnityEngine.PS4.PS4Input.GetLastMoveGyro(id, index);
        }

        public int PS4Input_MoveGetButtons(int id, int index) {
            return UnityEngine.PS4.PS4Input.MoveGetButtons(id, index);
        }

        public int PS4Input_MoveGetAnalogButton(int id, int index) {
            return UnityEngine.PS4.PS4Input.MoveGetAnalogButton(id, index);
        }

        public bool PS4Input_MoveIsConnected(int id, int index) {
            return UnityEngine.PS4.PS4Input.MoveIsConnected(id, index);
        }

        public int PS4Input_MoveGetUsersMoveHandles(int maxNumberControllers, int[] primaryHandles, int[] secondaryHandles) {
            return UnityEngine.PS4.PS4Input.MoveGetUsersMoveHandles(maxNumberControllers, primaryHandles, secondaryHandles);
        }

        public int PS4Input_MoveGetUsersMoveHandles(int maxNumberControllers, int[] primaryHandles) {
            return UnityEngine.PS4.PS4Input.MoveGetUsersMoveHandles(maxNumberControllers, primaryHandles);
        }

        public int PS4Input_MoveGetUsersMoveHandles(int maxNumberControllers) {
            return UnityEngine.PS4.PS4Input.MoveGetUsersMoveHandles(maxNumberControllers);
        }

        public System.IntPtr PS4Input_MoveGetControllerInputForTracking() {
            return UnityEngine.PS4.PS4Input.MoveGetControllerInputForTracking();
        }

        public int PS4Input_MoveSetLightSphere(int id, int index, int red, int green, int blue) {
            return UnityEngine.PS4.PS4Input.MoveSetLightSphere(id, index, red, green, blue);
        }

        public int PS4Input_MoveSetVibration(int id, int index, int motor) {
            return UnityEngine.PS4.PS4Input.MoveSetVibration(id, index, motor);
        }
    }
}
#endif
