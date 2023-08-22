using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Internal {
    /// <summary>
    /// Stores graph metadata so that it will persist through play-mode, assembly reloads, and
    /// through editor sessions.
    /// </summary>
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class fiGraphMetadataPersistentStorage : BaseBehavior<FullSerializerSerializer>,
        IEditorOnlyTag, ISerializationAlwaysDirtyTag, ISerializationCallbacks {

        #region Serialization
        [SerializeField]
        private UnityObject[] _keys;
        [SerializeField]
        private fiGraphMetadata[] _values;

        void ISerializationCallbacks.OnBeforeSerialize() {
            _keys = null;
            _values = null;

            if (SavedGraphs != null) {
                _keys = SavedGraphs.Keys.ToArray();
                _values = SavedGraphs.Values.ToArray();
            }
        }

        void ISerializationCallbacks.OnAfterSerialize() {
        }

        void ISerializationCallbacks.OnBeforeDeserialize() {
        }

        void ISerializationCallbacks.OnAfterDeserialize() {
            SavedGraphs = fiUtility.CreateDictionary(_keys, _values);
        }
        #endregion

        // We use this to destroy any metadata instances that don't get deleted, except when
        // we are creating a new metadata instance and we want it to be alive. This variable
        // will disable the deletion code in OnEnable (but only once -- it is automatically
        // set back to false).
        protected static bool _preventDestroyOnEnable;

        protected void OnEnable() {
            if (!_preventDestroyOnEnable && fiSettings.EnableMetadataPersistence == false) {
                fiUtility.DestroyObject(gameObject);
            }
            _preventDestroyOnEnable = false;
        }

        [NotSerialized, HideInInspector]
        public Dictionary<UnityObject, fiGraphMetadata> SavedGraphs = new Dictionary<UnityObject, fiGraphMetadata>();

        [NotSerialized, ShowInInspector]
        public Dictionary<UnityObject, fiGraphMetadata> ViewableMetadata {
            get {
                var dict = new Dictionary<UnityObject, fiGraphMetadata>(SavedGraphs);
                dict.Remove(this);
                dict.Remove(gameObject);
                return dict;
            }
            set { }
        }

        private static fiGraphMetadataPersistentStorage _instance;
        public static fiGraphMetadataPersistentStorage Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<fiGraphMetadataPersistentStorage>();

                    if (ReferenceEquals(_instance, null)) {
                        var container = fiLateBindings.EditorUtility.CreateGameObjectWithHideFlags("fiSceneMetadata", HideFlags.HideInHierarchy);
                        _preventDestroyOnEnable = true;
                        _instance = container.AddComponent<fiGraphMetadataPersistentStorage>();
                    }

                    if (fiSettings.EnableMetadataPersistence == false) {
                        _instance.gameObject.hideFlags |= HideFlags.DontSave;
                    }
                }

                return _instance;
            }
        }
    }
}
