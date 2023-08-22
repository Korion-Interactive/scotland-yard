using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector {
    public class fiObject : ISerializationCallbackReceiver {

        private List<UnityObject> _objectReferences;

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            throw new System.NotImplementedException();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            throw new System.NotImplementedException();
        }
    }
}