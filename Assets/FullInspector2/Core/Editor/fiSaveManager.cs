﻿using System;
using FullInspector.Internal;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace FullInspector {
    /// <summary>
    /// A few APIs to forcibly call SaveState and RestoreState on every type that implements
    /// ISerializedObject. This API should no longer have any use as of Unity 4.5, because of
    /// serialization callbacks.
    /// </summary>
    public static class fiSaveManager {
        /// <summary>
        /// Forcibly save the state of all objects which derive from ISerializedObject.
        /// ISerializedObject saving is managed automatically when you use the editor (and can be
        /// customized in fiSettings). However, if you're playing a game and make a save
        /// level, ensure to call FullInspectorSaveManager.SaveAll()
        /// </summary>
        [MenuItem("Window/Full Inspector/Developer/Save All", priority = 0)]
        public static void SaveAll() {
            foreach (Type serializedObjectType in
                fiRuntimeReflectionUtility.AllSimpleTypesDerivingFrom(typeof(ISerializedObject))) {

                UnityObject[] objects = UnityObject.FindObjectsOfType(serializedObjectType);
                for (int i = 0; i < objects.Length; ++i) {
                    var obj = (ISerializedObject)objects[i];
                    obj.SaveState();
                }
            }
        }

        /// <summary>
        /// Forcibly restore the state of all objects which derive from ISerializedObject.
        /// </summary>
        [MenuItem("Window/Full Inspector/Developer/Restore All", priority = 1)]
        public static void RestoreAll() {
            foreach (Type serializedObjectType in
                fiRuntimeReflectionUtility.AllSimpleTypesDerivingFrom(typeof(ISerializedObject))) {

                UnityObject[] objects = UnityObject.FindObjectsOfType(serializedObjectType);
                for (int i = 0; i < objects.Length; ++i) {
                    var obj = (ISerializedObject)objects[i];
                    obj.RestoreState();
                }
            }
        }

    }
}