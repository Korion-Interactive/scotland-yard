using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

#if UNITY_PS5
namespace Unity.PSN.PS5.UDS
{
    /// <summary>
    /// The Universal Data System ("UDS") is a part of the data platform that accumulates in-game events and user actions and standardizes them as UDS model data
    /// </summary>
    public class UniversalDataSystem
    {
        enum NativeMethods : UInt32
        {
            StartSystem = 0x0200001u,
            StopSystem = 0x0200002u,
            GetMemoryStats = 0x0200003u,
            PostEvent = 0x0200006u,
            EventToString = 0x0200007u,
            UnlockTrophy = 0x0200010u,
            UnlockTrophyProgress = 0x0200011u,
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("UDS");
        }

        internal static void Stop()
        {
            workerQueue.Stop();
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal UniversalDataSystem queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// Has the Trophy system been initialised. <see cref="StartSystemRequest"/> and <see cref="StopSystemRequest"/>
        /// </summary>
        public static bool IsInitialized { get; internal set; } = false;

        /// <summary>
        /// A UDS Event
        /// </summary>
        public class UDSEvent
        {
            /// <summary>
            /// Event name
            /// </summary>
            public string Name { get; internal set; }

            /// <summary>
            /// Properties to set
            /// </summary>
            public EventProperties Properties { get; set; }

            /// <summary>
            /// Create an event.
            /// </summary>
            /// <param name="name">Name of the event</param>
            /// <param name="properties">Event properties</param>
            public void Create(string name, EventProperties properties = null)
            {
                Name = name;

                if (properties != null)
                {
                    Properties = properties;
                }
                else
                {
                    Properties = new EventProperties();
                }
            }

            internal void Serialise(BinaryWriter writer)
            {
                writer.WritePrxString(Name);

                if (Properties != null)
                {
                    writer.Write(true);
                    Properties.Serialise(writer);
                }
                else
                {
                    writer.Write(false);
                }
            }
        }

        public enum PropertyType
        {
            /// <summary>Invalid</summary>
            Invalid,

            /// <summary>32 bit Integer</summary>
            Int32,
            /// <summary>32 bit Unsigned Integer</summary>
            UInt32,
            /// <summary>64 bit Integer</summary>
            Int64,
            /// <summary>64 bit Unsigned Integer</summary>
            UInt64,
            /// <summary>String</summary>
            String,
            /// <summary>32 bit Float</summary>
            Float,
            /// <summary>64 bit Float (double)</summary>
            Float64,
            /// <summary>Boolean</summary>
            Bool,
            /// <summary>Binary array (byte[])</summary>
            Binary,
            /// <summary>Subset of properties <see cref="EventProperties"/></summary>
            Properties,
            /// <summary>An array of properties <see cref="EventPropertyArray"/> </summary>
            Array,
        }

        /// <summary>
        /// A base class properties container
        /// </summary>
        public class PropertiesContainer
        {
            internal System.Object Parent { set; get; } // Parent can an EventProperty or another EventPropertyArray

            protected internal void SetParent(EventPropertyArray parentArray)
            {
                if (Parent != null)
                {
                    throw new System.InvalidOperationException("Attempting to add EventPropertyArray as a parent when this container already has a parent");
                }

                Parent = parentArray;
            }

            protected internal void SetParent(EventProperty property)
            {
                if (Parent != null)
                {
                    throw new System.InvalidOperationException("Attempting to add EventProperty as a parent when this container already has a parent");
                }

                Parent = property;
            }

            protected internal void ClearParent()
            {
                Parent = null;
            }
        }

        /// <summary>
        /// A set of properties keys and values
        /// </summary>
        public class EventProperties : PropertiesContainer
        {
            Dictionary<string, EventProperty> properties = new Dictionary<string, EventProperty>();

            /// <summary>
            /// Clear the properties
            /// </summary>
            public void Reset()
            {
                properties.Clear();
            }

            /// <summary>
            /// Either add or update an existing property base on key
            /// </summary>
            /// <param name="prop"></param>
            public void Set(EventProperty prop)
            {
                if (properties.ContainsKey(prop.Key))
                {
                    EventProperty oldProp;
                    if (properties.TryGetValue(prop.Key, out oldProp))
                    {
                        // If the property contains another properties dictionary or array update it's parent as it's been removed.
                        oldProp.ClearParent();

                        properties.Remove(prop.Key);
                    }
                }

                properties.Add(prop.Key, prop);

                // If the property contains another properties dictionary or array update it's parent.
                prop.SetParent(this);
            }

            /// <summary>
            /// Set or update an integer property.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Value</param>
            public void Set(string key, Int32 value) { Set(new EventProperty(key, value)); }

            /// <summary>
            /// Set or update an unsigned integer property.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Value</param>
            public void Set(string key, UInt32 value) { Set(new EventProperty(key, value)); }

            /// <summary>
            /// Set or update an integer property.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Value</param>
            public void Set(string key, Int64 value) { Set(new EventProperty(key, value)); }

            /// <summary>
            /// Set or update an unsigned integer property.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Value</param>
            public void Set(string key, UInt64 value) { Set(new EventProperty(key, value)); }

            /// <summary>
            /// Set or update an string property.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Value</param>
            public void Set(string key, string value) { Set(new EventProperty(key, value)); }

            /// <summary>
            /// Set or update an float property.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Value</param>
            public void Set(string key, float value) { Set(new EventProperty(key, value)); }

            /// <summary>
            /// Set or update an double property.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Value</param>
            public void Set(string key, double value) { Set(new EventProperty(key, value)); }

            /// <summary>
            /// Set or update an boolean property.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Value</param>
            public void Set(string key, bool value) { Set(new EventProperty(key, value)); }

            /// <summary>
            /// Set or update an binary (byte[]) property.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Value</param>
            public void Set(string key, byte[] value) { Set(new EventProperty(key, value, value.Length)); }

            /// <summary>
            /// Set or update an binary (byte[]) property.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Array of byte values</param>
            /// <param name="size">Number of array elements to include</param>
            public void Set(string key, byte[] value, int size) { Set(new EventProperty(key, value, size)); }

            /// <summary>
            /// Attach or update a set of properties to this group.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">The set of key/value properties</param>
            public void Set(string key, EventProperties value) { Set(new EventProperty(key, value)); }

            /// <summary>
            /// Attach or update an array of properties to this group.
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">The array of properties</param>
            public void Set(string key, EventPropertyArray value) { Set(new EventProperty(key, value)); }

            /// <summary>
            /// Create or replace a property key and type
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="type">Value type</param>
            /// <returns>Object containing the property data</returns>
            public EventProperty Set(string key, PropertyType type)
            {
                EventProperty prop = new EventProperty(key, type);
                Set(prop);
                return prop;
            }

            internal void Serialise(BinaryWriter writer)
            {
                writer.Write((UInt32)properties.Count);

                foreach (var item in properties.Values)
                {
                    item.Serialise(writer);
                }
            }
        }

        /// <summary>
        /// An array of property values of the same type
        /// </summary>
        public class EventPropertyArray : PropertiesContainer
        {
            IList list;

            /// <summary>
            /// The type of value
            /// </summary>
            public PropertyType ArrayType { get; internal set; } = PropertyType.Invalid;

            /// <summary>
            /// Create a property array of a set type
            /// </summary>
            /// <param name="arrayType"></param>
            public EventPropertyArray(PropertyType arrayType)
            {
                ArrayType = arrayType;

                // Create a list of the correct type
                switch (ArrayType)
                {
                    case PropertyType.Int32:
                        list = new List<Int32>();
                        break;
                    case PropertyType.UInt32:
                        list = new List<UInt32>();
                        break;
                    case PropertyType.Int64:
                        list = new List<Int64>();
                        break;
                    case PropertyType.UInt64:
                        list = new List<UInt64>();
                        break;
                    case PropertyType.String:
                        list = new List<string>();
                        break;
                    case PropertyType.Float:
                        list = new List<float>();
                        break;
                    case PropertyType.Float64:
                        list = new List<double>();
                        break;
                    case PropertyType.Bool:
                        list = new List<bool>();
                        break;
                    case PropertyType.Binary:
                        list = new List<byte[]>();
                        break;
                    case PropertyType.Properties:
                        list = new List<EventProperties>();
                        break;
                    case PropertyType.Array:
                        list = new List<EventPropertyArray>();
                        break;
                }
            }

            /// <summary>
            /// Reset the array
            /// </summary>

            public void Reset()
            {
                // Enumerate the list, if it contains EventPropertyArray or EventProperties and clear that Parent link
                if (ArrayType == PropertyType.Array)
                {
                    List<EventPropertyArray> array = list as List<EventPropertyArray>;

                    for (int i = 0; i < array.Count; i++)
                    {
                        array[i].ClearParent();
                    }
                }
                else if (ArrayType == PropertyType.Properties)
                {
                    List<EventProperties> properties = list as List<EventProperties>;

                    for (int i = 0; i < properties.Count; i++)
                    {
                        properties[i].ClearParent();
                    }
                }

                list.Clear();
            }

            /// <summary>
            /// Add a value to the array
            /// </summary>
            /// <typeparam name="T">Value type</typeparam>
            /// <param name="value">The value to add</param>
            public void Set<T>(T value)
            {
                if (list is List<T>)
                {
                    if (value is EventPropertyArray)
                    {
                        EventPropertyArray array = value as EventPropertyArray;
                        if (array != null) array.SetParent(this);
                    }
                    else
                    {
                        EventProperties properties = value as EventProperties;
                        if (properties != null) properties.SetParent(this);
                    }

                    ((List<T>)list).Add(value);
                }
            }

            /// <summary>
            /// Copy values from an Array to this properties array, replacing any item in this array
            /// </summary>
            /// <typeparam name="T">The value type</typeparam>
            /// <param name="values">The array of values</param>
            public void CopyValues<T>(T[] values)
            {
                if (list is List<T>)
                {
                    Reset();
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] is EventPropertyArray)
                        {
                            EventPropertyArray array = values[i] as EventPropertyArray;
                            if (array != null) array.SetParent(this);
                        }
                        else
                        {
                            EventProperties properties = values[i] as EventProperties;
                            if (properties != null) properties.SetParent(this);
                        }

                        ((List<T>)list).Add(values[i]);
                    }
                }
            }

            /// <summary>
            /// Append values from an Array to this properties array
            /// </summary>
            /// <typeparam name="T">The value type</typeparam>
            /// <param name="values">The array of values</param>
            public void AppendValues<T>(T[] values)
            {
                if (list is List<T>)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] is EventPropertyArray)
                        {
                            EventPropertyArray array = values[i] as EventPropertyArray;
                            if (array != null) array.SetParent(this);
                        }
                        else
                        {
                            EventProperties properties = values[i] as EventProperties;
                            if (properties != null) properties.SetParent(this);
                        }

                        ((List<T>)list).Add(values[i]);
                    }
                }
            }

            internal void Serialise(BinaryWriter writer)
            {
                writer.Write((UInt32)ArrayType);

                writer.Write((UInt32)list.Count);

                WriteValues(writer);
            }

            internal void WriteValues(BinaryWriter writer)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    switch (ArrayType)
                    {
                        case PropertyType.Int32:
                            writer.Write((Int32)((List<Int32>)list)[i]);
                            break;
                        case PropertyType.UInt32:
                            writer.Write((UInt32)((List<UInt32>)list)[i]);
                            break;
                        case PropertyType.Int64:
                            writer.Write((Int64)((List<Int64>)list)[i]);
                            break;
                        case PropertyType.UInt64:
                            writer.Write((UInt64)((List<UInt64>)list)[i]);
                            break;
                        case PropertyType.String:
                            writer.WritePrxString(((List<string>)list)[i]);
                            break;
                        case PropertyType.Float:
                            writer.Write((float)((List<float>)list)[i]);
                            break;
                        case PropertyType.Float64:
                            writer.Write((double)((List<double>)list)[i]);
                            break;
                        case PropertyType.Bool:
                            writer.Write((bool)((List<bool>)list)[i]);
                            break;
                        case PropertyType.Binary:
                            var bytes = ((List<byte[]>)list)[i];
                            writer.Write(bytes.Length);
                            writer.Write(bytes);
                            break;
                        case PropertyType.Properties:
                            EventProperties properties = ((List<EventProperties>)list)[i];
                            properties.Serialise(writer);
                            break;
                        case PropertyType.Array:
                            EventPropertyArray propertyArray = ((List<EventPropertyArray>)list)[i];
                            propertyArray.Serialise(writer);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// A UDS property containing an support type of property value
        /// </summary>
        public class EventProperty
        {
            internal EventProperties Parent { get; set; }  // Parent can only be an EventProperties dictionary

            /// <summary>
            /// Create an empty property
            /// </summary>
            public EventProperty()
            {

            }

            /// <summary>
            /// Create a property with a key and value type
            /// </summary>
            /// <param name="key">Property Key</param>
            /// <param name="type">Value Type</param>
            public EventProperty(string key, PropertyType type)
            {
                Key = key;
                PropType = type;
            }

            /// <summary>
            /// Set a property value
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Interger value</param>
            public EventProperty(string key, Int32 value) { Set(key, PropertyType.Int32, (UInt64)value); }

            /// <summary>
            /// Set a property value
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Interger value</param>
            public EventProperty(string key, UInt32 value) { Set(key, PropertyType.UInt32, (UInt64)value); }

            /// <summary>
            /// Set a property value
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Interger value</param>
            public EventProperty(string key, Int64 value) { Set(key, PropertyType.Int64, (UInt64)value); }

            /// <summary>
            /// Set a property value
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Interger value</param>
            public EventProperty(string key, UInt64 value) { Set(key, PropertyType.UInt64, (UInt64)value); }

            /// <summary>
            /// Set a property value
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">String value</param>
            public EventProperty(string key, string value) { Set(key, value); }

            /// <summary>
            /// Set a property value
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Float value</param>
            public EventProperty(string key, float value) { Set(key, value); }

            /// <summary>
            /// Set a property value
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Double value</param>
            public EventProperty(string key, double value) { Set(key, value); }

            /// <summary>
            /// Set a property value
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Boolean value</param>
            public EventProperty(string key, bool value) { Set(key, value); }

            /// <summary>
            /// Set a property value
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">Binary data</param>
            public EventProperty(string key, byte[] value, int size) { Set(key, value, size); }

            /// <summary>
            /// Set a property value
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">EventProperties collection</param>
            public EventProperty(string key, EventProperties value) { SetProperties(key, value); }

            /// <summary>
            /// Set a property value
            /// </summary>
            /// <param name="key">Property key</param>
            /// <param name="value">EventPropertyArray collection</param>
            public EventProperty(string key, EventPropertyArray value) { SetPropertiesArray(key, value); }

            /// <summary>
            /// The property key
            /// </summary>
            public string Key { get; internal set; }

            /// <summary>
            /// The property value type
            /// </summary>
            public PropertyType PropType { get; internal set; } = PropertyType.Invalid;

            // Used to internal store any integer value
            internal UInt64 IntValue { get; set; }

            // Used to internaly store any floating point value
            internal float FloatValue { get; set; }

            internal double DoubleValue { get; set; }

            internal bool BoolValue { get; set; }

            // Used to internaly store any string
            internal string StrValue { get; set; }

            internal byte[] ByteValue { get; set; }

            internal EventProperties PropertiesValue { get; set; }

            internal EventPropertyArray PropertiesArray { get; set; }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="value">Integer value</param>
            public void Update(Int32 value) { if (PropType == PropertyType.Int32) IntValue = (UInt64)value; }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="value">Integer value</param>
            public void Update(UInt32 value) { if (PropType == PropertyType.UInt32) IntValue = (UInt64)value; }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="value">Integer value</param>
            public void Update(Int64 value) { if (PropType == PropertyType.Int64) IntValue = (UInt64)value; }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="value">Integer value</param>
            public void Update(UInt64 value) { if (PropType == PropertyType.UInt64) IntValue = (UInt64)value; }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="value">String value</param>
            public void Update(string value) { if (PropType == PropertyType.String) StrValue = value; }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="value">Float value</param>
            public void Update(float value) { if (PropType == PropertyType.Float) FloatValue = value; }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="value">Double value</param>
            public void Update(double value) { if (PropType == PropertyType.Float64) DoubleValue = value; }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="value">Boolean value</param>
            public void Update(bool value) { if (PropType == PropertyType.Bool) BoolValue = value; }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="value">Binary data</param>
            public void Update(byte[] value) { Update(value, value.Length); }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="value">Binary data</param>
            /// <param name="size">Number of elements in the binary data to use</param>
            public void Update(byte[] value, int size)
            {
                if (PropType == PropertyType.Binary)
                {
                    ByteValue = new byte[size];
                    Array.Copy(value, ByteValue, size);
                }
            }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="properties">EventProperties collection</param>
            public void Update(EventProperties properties)
            {
                if (PropType == PropertyType.Properties)
                {
                    if (PropertiesValue != null) PropertiesValue.ClearParent();
                    PropertiesValue = properties;
                    PropertiesValue.SetParent(this);
                }
            }

            /// <summary>
            /// Update the value
            /// </summary>
            /// <param name="propertiesArray">EventPropertyArray collection</param>
            public void Update(EventPropertyArray propertiesArray)
            {
                if (PropType == PropertyType.Array)
                {
                    if (PropertiesArray != null) PropertiesArray.ClearParent();
                    PropertiesArray = propertiesArray;
                    PropertiesArray.SetParent(this);
                }
            }

            internal void Set(string key, PropertyType pType, UInt64 value)
            {
                Key = key;
                PropType = pType;
                IntValue = value;
            }

            internal void Set(string key, float value)
            {
                Key = key;
                PropType = PropertyType.Float;
                FloatValue = value;
            }

            internal void Set(string key, double value)
            {
                Key = key;
                PropType = PropertyType.Float64;
                DoubleValue = value;
            }

            internal void Set(string key, bool value)
            {
                Key = key;
                PropType = PropertyType.Bool;
                BoolValue = value;
            }

            internal void Set(string key, string value)
            {
                Key = key;
                PropType = PropertyType.String;
                StrValue = value;
            }

            internal void Set(string key, byte[] value, int size)
            {
                Key = key;
                PropType = PropertyType.Binary;

                ByteValue = new byte[size];

                Array.Copy(value, ByteValue, size);
            }

            internal void SetProperties(string key, EventProperties properties)
            {
                Key = key;
                PropType = PropertyType.Properties;
                PropertiesValue = properties;

                properties.SetParent(this);
            }

            internal void SetPropertiesArray(string key, EventPropertyArray propertiesArray)
            {
                Key = key;
                PropType = PropertyType.Array;
                PropertiesArray = propertiesArray;

                propertiesArray.SetParent(this);
            }

            internal void SetParent(EventProperties parent)
            {
                if (Parent != null)
                {
                    throw new System.InvalidOperationException("Attempting to add EventProperty to a container when it already has been added to the same or another container.");
                }

                // An EventProperty can only be added to a EventProperties dictionary.
                Parent = parent;
            }

            internal void ClearParent()
            {
                Parent = null;
            }

            internal void Serialise(BinaryWriter writer)
            {
                writer.WritePrxString(Key);

                writer.Write((UInt32)PropType);

                switch (PropType)
                {
                    case PropertyType.Int32:
                        writer.Write((Int32)IntValue);
                        break;
                    case PropertyType.UInt32:
                        writer.Write((UInt32)IntValue);
                        break;
                    case PropertyType.Int64:
                        writer.Write((Int64)IntValue);
                        break;
                    case PropertyType.UInt64:
                        writer.Write((UInt64)IntValue);
                        break;
                    case PropertyType.String:
                        writer.WritePrxString(StrValue);
                        break;
                    case PropertyType.Float:
                        writer.Write(FloatValue);
                        break;
                    case PropertyType.Float64:
                        writer.Write(DoubleValue);
                        break;
                    case PropertyType.Bool:
                        writer.Write(BoolValue);
                        break;
                    case PropertyType.Binary:
                        writer.Write(ByteValue.Length);
                        writer.Write(ByteValue);
                        break;
                    case PropertyType.Properties:
                        PropertiesValue.Serialise(writer);
                        break;
                    case PropertyType.Array:
                        PropertiesArray.Serialise(writer);
                        break;
                }
            }
        }

        /// <summary>
        /// Start the Universal Data System. To test is the system is initialized <see cref="IsInitialized"/>
        /// </summary>
        public class StartSystemRequest : Request
        {
            /// <summary>
            ///  Size of the memory pool to be used by the UniversalDataSystem library (bytes)
            /// </summary>
            public UInt64 PoolSize { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.StartSystem);

                nativeMethod.Writer.Write(PoolSize);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);

                IsInitialized = true;
            }
        }

        /// <summary>
        /// Stop the Universal Data System. To test is the system is initialized <see cref="IsInitialized"/>
        /// </summary>
        public class StopSystemRequest : Request
        {
            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.StopSystem);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);

                IsInitialized = false;
            }
        }

        /// <summary>
        /// Get memory information of the UniversalDataSystem library
        /// </summary>
        public class GetMemoryStatsRequest : Request
        {
            /// <summary>
            /// Size of the UniversalDataSystem library memory pool. <see cref="StartSystemRequest.PoolSize"/>
            /// </summary>
            public UInt64 PoolSize { get; internal set; }

            /// <summary>
            /// Maximum size of memory used by the UniversalDataSystem library
            /// </summary>
            public UInt64 MaxInuseSize { get; internal set; }

            /// <summary>
            /// Size of memory currently used by the NpUniversalDataSystem library
            /// </summary>
            public UInt64 CurrentInuseSize { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetMemoryStats);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    PoolSize = nativeMethod.Reader.ReadUInt64();
                    MaxInuseSize = nativeMethod.Reader.ReadUInt64();
                    CurrentInuseSize = nativeMethod.Reader.ReadUInt64();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Post an event
        /// </summary>
        public class PostEventRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Event to post
            /// </summary>
            public UDSEvent EventData { get; set; }

            /// <summary>
            /// Calculate the estimated size of the event after sending
            /// </summary>
            public bool CalculateEstimatedSize { get; set; }

            /// <summary>
            /// The returned estimated size of the event. <see cref="CalculateEstimatedSize"/>
            /// </summary>
            public UInt64 EstimatedSize { get; internal set; }

            protected internal override void Run()
            {
                EstimatedSize = 0;

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.PostEvent);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(CalculateEstimatedSize);

                EventData.Serialise(nativeMethod.Writer);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    EstimatedSize = nativeMethod.Reader.ReadUInt64();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Convert an event to a readable string
        /// </summary>
        public class EventDebugStringRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Event to convert
            /// </summary>
            public UDSEvent EventData { get; set; }

            /// <summary>
            /// Output string
            /// </summary>
            public string Output { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.EventToString);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                EventData.Serialise(nativeMethod.Writer);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    Output = nativeMethod.Reader.ReadPrxString();
                }
                else
                {
                    Output = "";
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Event to unlock a trophy
        /// </summary>
        public class UnlockTrophyRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Trophy Id to unlock
            /// </summary>
            public Int32 TrophyId { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.UnlockTrophy);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(TrophyId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Request to unlock a progress trophy
        /// </summary>
        [Obsolete("Please use UpdateTrophyProgressRequest instead")]
        public class UnlockTrophyProgressRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Trophy Id to unlock
            /// </summary>
            public Int32 TrophyId { get; set; }

            /// <summary>
            /// The trophy progress
            /// </summary>
            public Int64 Progress { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.UnlockTrophyProgress);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(TrophyId);
                nativeMethod.Writer.Write(Progress);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Request to update a progress trophy
        /// </summary>
        public class UpdateTrophyProgressRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Trophy Id to unlock
            /// </summary>
            public Int32 TrophyId { get; set; }

            /// <summary>
            /// The trophy progress
            /// </summary>
            public Int64 Progress { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.UnlockTrophyProgress);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(TrophyId);
                nativeMethod.Writer.Write(Progress);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
    }
}
#endif
