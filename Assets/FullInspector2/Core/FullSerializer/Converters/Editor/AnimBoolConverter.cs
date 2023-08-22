using System;
using FullSerializer;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace FullInspector.Serializers.FullSerializer {
    [InitializeOnLoad]
    public class AnimBoolConverter : fsConverter {
        public static void Register() {
            FullSerializerSerializer.AddConverter(new AnimBoolConverter());
        }

        public override bool CanProcess(Type type) {
            return type == typeof(AnimBool);
        }

        public override fsFailure TrySerialize(object instance, out fsData serialized, Type storageType) {
            var anim = (AnimBool)instance;
            serialized = new fsData(anim.target);
            return fsFailure.Success;
        }

        public override fsFailure TryDeserialize(fsData data, ref object instance, Type storageType) {
            instance = new AnimBool(data.AsBool);
            return fsFailure.Success;
        }

        public override bool RequestCycleSupport(Type storageType) {
            return false;
        }

        public override bool RequestInheritanceSupport(Type storageType) {
            return false;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return storageType;
        }
    }
}