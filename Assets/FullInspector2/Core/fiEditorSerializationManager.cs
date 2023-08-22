using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Internal {
    /// <summary>
    /// If a type extends this interface, then it will always be serialized when Unity sends a
    /// serialization request.
    /// </summary>
    public interface ISerializationAlwaysDirtyTag { }

    /// <summary>
    /// Manages the serialization of ISerializedObject instances. Unity provides a nice
    /// serialization callback, ISerializationCallbackReceiver, however, it often executes the
    /// callback methods on auxiliary threads. Some of the serializers have issues with this
    /// because they invoke unityObj == null, which is only available on the primary thread. To
    /// deal with this, this class defers deserialization so that it always executes on the
    /// primary thread.
    /// </summary>
    public static class fiEditorSerializationManager {
        // !! note !!
        // RunDeserializations is registered to EditorApplicatoin.update on application
        // initialization in fiEditorSerializationManagerEditorInjector

        /// <summary>
        /// Should serialization be disabled? This is used by the serialization migration system
        /// where after migration serialization should not happen automatically.
        /// </summary>
        [NonSerialized]
        public static bool DisableAutomaticSerialization;

        /// <summary>
        /// Items to deserialize. We use a queue because deserialization of one item may
        /// technically cause another item to be added to the queue.
        /// </summary>
        private static readonly Queue<ISerializedObject> _toDeserialize = new Queue<ISerializedObject>();

        /// <summary>
        /// A set of objects that we have deserialized. This determines if we can reserialize
        /// an object at scene save / etc.
        /// </summary>
        private static readonly HashSet<ISerializedObject> _deserialized = new HashSet<ISerializedObject>();

        /// <summary>
        /// Items that have been modified and should be saved again. Unity will happily send
        /// lots and lots of serialization requests, but most of them are unnecessary.
        /// </summary>
        // TODO: see if this is actually necessary since we just always reserialize all of the deserialized items
        private static readonly HashSet<ISerializedObject> _dirty = new HashSet<ISerializedObject>();

        /// <summary>
        /// Attempts to serialize the given object. Serialization will only occur if the object is
        /// dirty. After being serialized, the object is no longer dirty.
        /// </summary>
        public static void SubmitSerializeRequest(ISerializedObject obj) {
            lock (typeof (fiEditorSerializationManager)) {
                bool isDirty = _dirty.Contains(obj);

                // Serialization is disabled
                if (DisableAutomaticSerialization) {
                    return;
                }

                // The given object is the current inspected object and we havne't marked it as
                // dirty. There is no need to serialize it.
                if (!isDirty && fiLateBindings.Selection.activeObject == GetGameObjectOrScriptableObjectFrom(obj)) {
                    return;
                }

                // The object is dirty or we have deserialized it. A serialize request has been submitted
                // so we should actually service it.
                if (isDirty || _deserialized.Contains(obj) || obj is ISerializationAlwaysDirtyTag) {
                    obj.SaveState();
                    _dirty.Remove(obj);
                    ObjectModificationDetector.Update(obj);
                }
            }
        }

        /// <summary>
        /// Fetches the associated GameObject/ScriptableObject from the given serialized object.
        /// </summary>
        private static UnityObject GetGameObjectOrScriptableObjectFrom(ISerializedObject obj) {
            if (obj is MonoBehaviour) {
                return ((MonoBehaviour)obj).gameObject;
            }
            return (UnityObject)obj;
        }

        /// <summary>
        /// Attempt to deserialize the given object. The deserialization will occur on the next
        /// call to RunDeserializations(). This does nothing if we are not an editor.
        /// </summary>
        public static void SubmitDeserializeRequest(ISerializedObject obj) {
            lock (typeof (fiEditorSerializationManager)) {
                if (fiUtility.IsEditor == false) return;

                _toDeserialize.Enqueue(obj);
            }
        }

        public static void RunDeserializations() {
            // We never run deserializations outside of the editor
            if (fiUtility.IsEditor == false) {
                return;
            }

            // Serialization is disabled
            if (DisableAutomaticSerialization) {
                return;
            }

            // Do not deserialize in the middle of a level load that might be running on another thread
            // (asynchronous) which can lead to a race condition causing the following assert:
            // ms_IDToPointer->find (obj->GetInstanceID ()) == ms_IDToPointer->end ()
            //
            // Very strange that the load is happening on another thread since RunDeserializations only
            // gets invoked from EditorApplication.update and EditorWindow.OnGUI.
            //
            // This method will get called again at a later point so there is no worries that we haven't
            // finished doing the deserializations.

			// disabled deprecation warning concerning Application.isLoadingLevel to avoid falsely refactoring FullInspector2
			#pragma warning disable 618
            if (fiLateBindings.EditorApplication.isPlaying && Application.isLoadingLevel) {
                return;
            }
			#pragma warning restore 618

            while (_toDeserialize.Count > 0) {
                ISerializedObject item = _toDeserialize.Dequeue();

                // If we're in play-mode, then we don't want to deserialize anything as that can wipe
                // user-data. We cannot do this in SubmitDeserializeRequest because
                // EditorApplication.isPlaying can only be called from the main thread. However,
                // we *do* want to restore prefabs and general disk-based resources which will not have
                // Awake called.
                if (fiLateBindings.EditorApplication.isPlaying) {
                    // note: We do a null check against unityObj to also check for destroyed objects,
                    //       which we don't need to bother restoring. Doing a null check against an
                    //       ISerializedObject instance will *not* call the correct == method, so
                    //       we need to be explicit about calling it against UnityObject.
                    var unityObj = item as UnityObject;

                    if (unityObj == null ||
                        fiLateBindings.PrefabUtility.IsPrefab(unityObj) == false) continue;
                }

                item.RestoreState();
                _deserialized.Add(item);
            }
        }

        public static void SetDirty(ISerializedObject obj) {
            _dirty.Add(obj);
        }
    }
}