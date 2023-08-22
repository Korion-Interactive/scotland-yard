using System;

namespace FullInspector {
    /// <summary>
    /// This will prevent Full Inspector from constructing an object instance in the
    /// inspector by default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InspectorNotDefaultConstructedAttribute : Attribute {   
    }
}