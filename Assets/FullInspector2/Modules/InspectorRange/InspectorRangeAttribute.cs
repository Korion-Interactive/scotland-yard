using System;

namespace FullInspector.Samples.Other.Limits {
    [Obsolete("Please use [InspectorRange] instead of [Limits]")]
    public class LimitsAttribute : Attribute {
        public float Min;
        public float Max;

        public LimitsAttribute(float min, float max) {
            Min = min;
            Max = max;
        }
    }
}

namespace FullInspector {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InspectorRangeAttribute : Attribute {
        /// <summary>
        /// The minimum value.
        /// </summary>
        public float Min;

        /// <summary>
        /// The maximum value.
        /// </summary>
        public float Max;

        public InspectorRangeAttribute(float min, float max) {
            Min = min;
            Max = max;
        }
    }
}