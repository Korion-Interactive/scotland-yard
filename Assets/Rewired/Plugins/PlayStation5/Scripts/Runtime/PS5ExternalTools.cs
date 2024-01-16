#if UNITY_PS5 && !UNITY_EDITOR

// Copyright (c) 2022 Augie R. Maddox, Guavaman Enterprises. All rights reserved.
#pragma warning disable 0219
#pragma warning disable 0618
#pragma warning disable 0649
#pragma warning disable 0067

using UnityEngine;

namespace Rewired.Utils.Platforms.PS5 {

    public sealed class PS5ExternalTools : IPS5ExternalTools {
    
        private byte[] _tempTriggerEffectPositionValueArray = new byte[10];
    
        public PS5ExternalTools() {
        }

        public Vector3 PS5Input_GetLastAcceleration(int id) {
            return UnityEngine.PS5.PS5Input.PadGetLastAcceleration(id);
        }

        public Vector3 PS5Input_GetLastGyro(int id) {
            return UnityEngine.PS5.PS5Input.PadGetLastGyro(id);
        }

        public Vector4 PS5Input_GetLastOrientation(int id) {
            return UnityEngine.PS5.PS5Input.PadGetLastOrientation(id);
        }

        public void PS5Input_GetLastTouchData(int id, out int touchNum, out int touch0x, out int touch0y, out int touch0id, out int touch1x, out int touch1y, out int touch1id) {
            UnityEngine.PS5.PS5Input.GetLastTouchData(id, out touchNum, out touch0x, out touch0y, out touch0id, out touch1x, out touch1y, out touch1id);
        }

        public void PS5Input_GetPadControllerInformation(int id, out float touchpixelDensity, out int touchResolutionX, out int touchResolutionY, out int analogDeadZoneLeft, out int analogDeadZoneright, out int connectionType) {
            UnityEngine.PS5.PS5Input.ConnectionType connectionTypeEnum;
            UnityEngine.PS5.PS5Input.GetPadControllerInformation(id, out touchpixelDensity, out touchResolutionX, out touchResolutionY, out analogDeadZoneLeft, out analogDeadZoneright, out connectionTypeEnum);
            connectionType = (int)connectionTypeEnum;
        }

        public void PS5Input_PadSetMotionSensorState(int id, bool bEnable) {
            UnityEngine.PS5.PS5Input.PadSetMotionSensorState(id, bEnable);
        }

        public void PS5Input_PadSetTiltCorrectionState(int id, bool bEnable) {
            UnityEngine.PS5.PS5Input.PadSetTiltCorrectionState(id, bEnable);
        }

        public void PS5Input_PadSetAngularVelocityDeadbandState(int id, bool bEnable) {
            UnityEngine.PS5.PS5Input.PadSetAngularVelocityDeadbandState(id, bEnable);
        }

        public void PS5Input_PadSetLightBar(int id, int red, int green, int blue) {
            UnityEngine.PS5.PS5Input.PadSetLightBar(id, red, green, blue);
        }

        public void PS5Input_PadResetLightBar(int id) {
            UnityEngine.PS5.PS5Input.PadResetLightBar(id);
        }

        public void PS5Input_PadSetVibration(int id, int largeMotor, int smallMotor) {
            UnityEngine.PS5.PS5Input.PadSetVibration(id, largeMotor, smallMotor);
        }

        public void PS5Input_PadSetVibrationMode(int id, Rewired.Platforms.PS5.PS5GamepadVibrationMode mode) {
            UnityEngine.PS5.PS5Input.PadSetVibrationMode(id, (UnityEngine.PS5.PS5Input.VibrationMode)mode);
        }

        public void PS5Input_PadResetOrientation(int id) {
            UnityEngine.PS5.PS5Input.PadResetOrientation(id);
        }

        public bool PS5Input_PadIsConnected(int id) {
            return UnityEngine.PS5.PS5Input.PadIsConnected(id);
        }

        public void PS5Input_GetUsersDetails(int slot, object loggedInUser) {
            if(loggedInUser == null) throw new System.ArgumentNullException("loggedInUser");
            UnityEngine.PS5.PS5Input.LoggedInUser user = UnityEngine.PS5.PS5Input.GetUsersDetails(slot);
            Rewired.Platforms.PS5.Internal.LoggedInUser retUser = loggedInUser as Rewired.Platforms.PS5.Internal.LoggedInUser;
            if(retUser == null) throw new System.ArgumentException("loggedInUser is not the correct type.");

            retUser.status = user.status;
            retUser.primaryUser = user.primaryUser;
            retUser.userId = user.userId;
            retUser.userName = user.userName;
            retUser.padHandle = user.padHandle;
        }

        public int PS5Input_GetDeviceClassForHandle(int handle) {
            return (int)UnityEngine.PS5.PS5Input.GetDeviceClassForHandle(handle);
        }

        public string PS5Input_GetDeviceClassString(int intValue) {
            return ((UnityEngine.PS5.PS5Input.DeviceClass)intValue).ToString();
        }

        public int PS5Input_PadGetUsersHandles2(int maxControllers, int[] handles) {
            return UnityEngine.PS5.PS5Input.PadGetUsersHandles2(maxControllers, handles);
        }

        private readonly UnityEngine.PS5.PS5Input.ControllerInformation _controllerInformation = new UnityEngine.PS5.PS5Input.ControllerInformation();

        public void PS5Input_GetSpecialControllerInformation(int id, int padIndex, object controllerInformation) {
            if(controllerInformation == null) throw new System.ArgumentNullException("controllerInformation");
            Rewired.Platforms.PS5.Internal.ControllerInformation tControllerInformation = controllerInformation as Rewired.Platforms.PS5.Internal.ControllerInformation;
            if(tControllerInformation == null) throw new System.ArgumentException("controllerInformation is not the correct type.");
            UnityEngine.PS5.PS5Input.ControllerInformation c = _controllerInformation;
            UnityEngine.PS5.PS5Input.GetSpecialControllerInformation(id, padIndex, ref c);
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

        public Vector3 PS5Input_SpecialGetLastAcceleration(int id) {
            return UnityEngine.PS5.PS5Input.SpecialGetLastAcceleration(id);
        }

        public Vector3 PS5Input_SpecialGetLastGyro(int id) {
            return UnityEngine.PS5.PS5Input.SpecialGetLastGyro(id);
        }

        public Vector4 PS5Input_SpecialGetLastOrientation(int id) {
            return UnityEngine.PS5.PS5Input.SpecialGetLastOrientation(id);
        }

        public int PS5Input_SpecialGetUsersHandles(int maxNumberControllers, int[] handles) {
            return UnityEngine.PS5.PS5Input.SpecialGetUsersHandles(maxNumberControllers, handles);
        }

        public int PS5Input_SpecialGetUsersHandles2(int maxNumberControllers, int[] handles) {
            return UnityEngine.PS5.PS5Input.SpecialGetUsersHandles2(maxNumberControllers, handles);
        }

        public bool PS5Input_SpecialIsConnected(int id) {
            return UnityEngine.PS5.PS5Input.SpecialIsConnected(id);
        }

        public void PS5Input_SpecialResetLightSphere(int id) {
            UnityEngine.PS5.PS5Input.SpecialResetLightSphere(id);
        }

        public void PS5Input_SpecialResetOrientation(int id) {
            UnityEngine.PS5.PS5Input.SpecialResetOrientation(id);
        }

        public void PS5Input_SpecialSetAngularVelocityDeadbandState(int id, bool bEnable) {
            UnityEngine.PS5.PS5Input.SpecialSetAngularVelocityDeadbandState(id, bEnable);
        }

        public void PS5Input_SpecialSetLightSphere(int id, int red, int green, int blue) {
            UnityEngine.PS5.PS5Input.SpecialSetLightSphere(id, red, green, blue);
        }

        public void PS5Input_SpecialSetMotionSensorState(int id, bool bEnable) {
            UnityEngine.PS5.PS5Input.SpecialSetMotionSensorState(id, bEnable);
        }

        public void PS5Input_SpecialSetTiltCorrectionState(int id, bool bEnable) {
            UnityEngine.PS5.PS5Input.SpecialSetTiltCorrectionState(id,  bEnable);
        }

        public void PS5Input_SpecialSetVibration(int id, int largeMotor, int smallMotor) {
            UnityEngine.PS5.PS5Input.SpecialSetVibration(id, largeMotor, smallMotor);
        }

        public void PS5Input_SetGamepadTriggerEffect(int id, Rewired.Platforms.PS5.PS5GamepadTriggerType trigger, Rewired.Platforms.PS5.IPS5GamepadTriggerEffect effect) {
            switch (effect.triggerEffectType) {
                case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.Off:
                    UnityEngine.PS5.PS5Input.SetGamepadTriggerEffectOff(id, (UnityEngine.PS5.PS5Input.GamepadTrigger)trigger);
                    break;
                case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.Feedback: {
                        var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectFeedback)effect;
                        UnityEngine.PS5.PS5Input.GamepadTriggerEffectFeedbackParam args = new UnityEngine.PS5.PS5Input.GamepadTriggerEffectFeedbackParam();
                        args.position = tEffect.position;
                        args.strength = tEffect.strength;
                        UnityEngine.PS5.PS5Input.SetGamepadTriggerEffectFeedback(id, (UnityEngine.PS5.PS5Input.GamepadTrigger)trigger, args);
                    }
                    break;
                case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.Weapon: {
                        var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectWeapon)effect;
                        UnityEngine.PS5.PS5Input.GamepadTriggerEffectWeaponParam args = new UnityEngine.PS5.PS5Input.GamepadTriggerEffectWeaponParam();
                        args.startPosition = tEffect.startPosition;
                        args.endPosition = tEffect.endPosition;
                        args.strength = tEffect.strength;
                        UnityEngine.PS5.PS5Input.SetGamepadTriggerEffectWeapon(id, (UnityEngine.PS5.PS5Input.GamepadTrigger)trigger, args);
                    }
                    break;
                case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.Vibration: {
                        var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectVibration)effect;
                        UnityEngine.PS5.PS5Input.GamepadTriggerEffectVibrationParam args = new UnityEngine.PS5.PS5Input.GamepadTriggerEffectVibrationParam();
                        args.position = tEffect.position;
                        args.amplitude = tEffect.amplitude;
                        args.frequency = tEffect.frequency;
                        UnityEngine.PS5.PS5Input.SetGamepadTriggerEffectVibration(id, (UnityEngine.PS5.PS5Input.GamepadTrigger)trigger, args);
                    }
                    break;
                case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.MultiplePositionFeedback: {
                        var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectMultiplePositionFeedback)effect;
                        UnityEngine.PS5.PS5Input.GamepadTriggerEffectMultiplePositionFeedbackParam args = new UnityEngine.PS5.PS5Input.GamepadTriggerEffectMultiplePositionFeedbackParam();
                        tEffect.strength.CopyTo(_tempTriggerEffectPositionValueArray);
                        args.strength = _tempTriggerEffectPositionValueArray;
                        UnityEngine.PS5.PS5Input.SetGamepadTriggerEffectMultiplePositionFeedback(id, (UnityEngine.PS5.PS5Input.GamepadTrigger)trigger, args);
                    }
                    break;
                case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.SlopeFeedback: {
                        var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectSlopeFeedback)effect;
                        UnityEngine.PS5.PS5Input.GamepadTriggerEffectSlopeFeedbackParam args = new UnityEngine.PS5.PS5Input.GamepadTriggerEffectSlopeFeedbackParam();
                        args.startPosition = tEffect.startPosition;
                        args.startStrength = tEffect.startStrength;
                        args.endPosition = tEffect.endPosition;
                        args.endStrength = tEffect.endStrength;
                        UnityEngine.PS5.PS5Input.SetGamepadTriggerEffectSlopeFeedback(id, (UnityEngine.PS5.PS5Input.GamepadTrigger)trigger, args);
                    }
                    break;
                case Rewired.Platforms.PS5.PS5GamepadTriggerEffectType.MultiplePositionVibration: {
                        var tEffect = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectMultiplePositionVibration)effect;
                        UnityEngine.PS5.PS5Input.GamepadTriggerEffectMultiplePositionVibrationParam args = new UnityEngine.PS5.PS5Input.GamepadTriggerEffectMultiplePositionVibrationParam();
                        tEffect.amplitude.CopyTo(_tempTriggerEffectPositionValueArray);
                        args.frequency = tEffect.frequency;
                        args.amplitude = _tempTriggerEffectPositionValueArray;
                        UnityEngine.PS5.PS5Input.SetGamepadTriggerEffectMultiplePositionVibration(id, (UnityEngine.PS5.PS5Input.GamepadTrigger)trigger, args);
                    }
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }

        public Rewired.Platforms.PS5.PS5GamepadTriggerEffectStates PS5Input_GetGamepadTriggerEffectStates(int id) {
            Rewired.Platforms.PS5.PS5GamepadTriggerEffectStates value = new Rewired.Platforms.PS5.PS5GamepadTriggerEffectStates();
            UnityEngine.PS5.PS5Input.GamepadTriggerEffectState left;
            UnityEngine.PS5.PS5Input.GamepadTriggerEffectState right;
            UnityEngine.PS5.PS5Input.GetGamepadTriggerEffectState(id, out left, out right);
            value.leftTrigger = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectState)left;
            value.rightTrigger = (Rewired.Platforms.PS5.PS5GamepadTriggerEffectState)right;
            return value;
        }
    }
}
#endif