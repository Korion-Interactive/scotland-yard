using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BitBarons.Util
{
    [Flags]
    public enum VariableAccessType
    {
        Get = 1,
        Set = 2,
        GetAndSet = Get | Set,
    }

    /// <summary>
    /// This class can access a member inside a GameObject or one of its components.
    /// This can be useful for data binding.
    /// This may not work on iOS or other AOT platforms (but maybe it works - not tested).
    /// </summary>
    [Serializable]
    public class VariableAccessor
    {
        public const string HIERACHY_SELF = "<self>";
        public const string HIERACHY_PARENT = "<parent>";

        /// <summary>
        /// The GameObject with the variable to get / set
        /// </summary>
        public GameObject Object;


        /// <summary>
        /// The GameObject containing the variable in relative hierachy to the Object variable. (dot-separated)
        /// </summary>
        public string ObjectHierachy = HIERACHY_SELF;

        /// <summary>
        /// The name of the Component which contains the accessable variable.
        /// If null or empty, the Object itself is searched for the variable
        /// </summary>
        public string ComponentName;

        /// <summary>
        /// The path to the variable to access. Can contain members as well as properties.
        /// The format of the path is dot separated: "myParentVariable.myChildVariaple.mySubChildVariable (...)
        /// </summary>
        public string VariableHierachy = string.Empty;

        /// <summary>
        /// The AccessType helps to filter available variables. 
        /// It is possible to get variables inside structs for example,
        /// but it is not possible to set them if the struct is accessed via a property.
        /// </summary>
        public VariableAccessType AccessType = VariableAccessType.GetAndSet;

        /// <summary>
        /// Shortcut to check the AccessType against Get
        /// </summary>
        public bool AccessGet { get { return (VariableAccessType.Get & AccessType) == VariableAccessType.Get; } }
        /// <summary>
        /// Shortcut to check the AccessType against Set
        /// </summary>
        public bool AccessSet { get { return (VariableAccessType.Set & AccessType) == VariableAccessType.Set; } }
        
        /// <summary>
        /// Tries to parse the given input to the correct type and sets the defined variable to the result value.
        /// </summary>
        /// <param name="input">the string to parse.</param>
        public void ParseAndSetValue(string input)
        {
            var tmp = AccessType;
            AccessType = VariableAccessType.GetAndSet;

            object result;
            try
            {
                var o = GetValue();
                result = Parser.Parse(o.GetType(), input);
            }
            catch (Exception ex)
            {
                throw Exception("ParseAndSet - " + ex.Message);
            }
            finally
            {
                AccessType = tmp;
            }

            SetValue(result);
        }

        /// <summary>
        /// Sets the defined variable to the passed value.
        /// </summary>
        public void SetValue(object value)
        {
            //if (!AccessSet)
            //    throw Exception("Set() - this accessor is meant for get operations only.");

            bool canGet, canSet;
            MemberInfo info;
            var obj = AccessVariable(Object, ObjectHierachy, ComponentName, VariableHierachy, out info, out canGet, out canSet);

            if (!canSet)
                throw Exception("Set() - member cannot be set. Probably because of a struct accessed through a property.");

            if (info is PropertyInfo)
                (info as PropertyInfo).SetValue(obj, value, null);
            else if (info is FieldInfo)
                (info as FieldInfo).SetValue(obj, value);
            else
                throw Exception("Set() - something went wrong... code not up to date?");
        }

        /// <summary>
        /// Gets the value of the defined variable and tries to convet it to T.
        /// </summary>
        /// <typeparam name="T">The expected type of the variable to get.</typeparam>
        public T GetValue<T>()
        {
            object val = GetValue();
            if (val is T)
                return (T)val;

            throw Exception("Get() - could not convert value to type " + typeof(T).ToString());
        }
        /// <summary>
        /// Gets the value of the defined variable.
        /// </summary>
        public object GetValue()
        {
            if (!AccessGet)
                throw Exception("Get() - this accessor is meant for set operations only.");


            bool canGet, canSet;
            MemberInfo info;
            var obj = AccessVariable(Object, ObjectHierachy, ComponentName, VariableHierachy, out info, out canGet, out canSet);

            if (!canGet)
                throw Exception("Get() - cannot get member. Probably because the variable to set is a property without a getter.");

            if (info is PropertyInfo)
                return (info as PropertyInfo).GetValue(obj, null);
            else if (info is FieldInfo)
                return (info as FieldInfo).GetValue(obj);
            else
                throw Exception("Get() - something went wrong... code not up to date?");
        }

        public string[] GetAllowedSubVariables()
        {
            if (Object == null)
                throw Exception("Object not set.");

            GameObject hObj = GetHierachyObject();

            Type t;
            if (string.IsNullOrEmpty(VariableHierachy))
            {
                t = (string.IsNullOrEmpty(ComponentName)) ? hObj.GetType() : hObj.GetComponent(ComponentName).GetType();
            }
            else
            {
                var tmp = AccessType;
                AccessType = VariableAccessType.Get;
                var obj = GetValue();
                AccessType = tmp;
                t = obj.GetType();
            }

            List<string> result = new List<string>();
            foreach(var member in t.GetMembers())
            {
                if (member.MemberType != MemberTypes.Property
                    && member.MemberType != MemberTypes.Field)
                    continue;
                
                if (member.ReflectedType.IsArray || !member.ReflectedType.IsPublic)
                    continue;

                if(member is FieldInfo)
                {
                    FieldInfo field = member as FieldInfo;
                    if (field.IsStatic)
                        continue;
                }
                if (member is PropertyInfo)
                {
                    PropertyInfo prop = member as PropertyInfo;
                    var indexed = prop.GetIndexParameters();
                    if (indexed != null && indexed.Length > 0) // no index-variables allowed atm
                        continue;
                }


                bool get, set;
                MemberInfo info;
                AccessVariable(Object, ObjectHierachy, ComponentName, (VariableHierachy + "." + member.Name).TrimStart('.'), out info, out get, out set);

                if ((get || !AccessGet) && (set || !AccessSet))
                    result.Add(member.Name);
            }

            return result.ToArray();
        }


        private object AccessVariable(GameObject rootObject, string objectHierachy, string componentName, string variableHierachy, out MemberInfo info, out bool canGet, out bool canSet)
        {
            if (rootObject == null)
                throw Exception("Object is null");

            GameObject holder = FindObjectInHierachy(rootObject, objectHierachy);

            object root = (string.IsNullOrEmpty(componentName))
                ? holder
                : holder.GetComponent(componentName) as object;

            string[] path = variableHierachy.Split('.');
            object current = root;
            canSet = true; canGet = true;

            for(int i = 0; i < path.Length; i++)
            {
                string varName = path[i];
                bool final = (i == path.Length - 1);

                Type t = current.GetType();


                // PROPERTY 
                var prop = t.GetProperty(varName);

                if(prop != null)
                {
                    if (final)
                    {
                        canGet = canGet && prop.CanRead;
                        canSet = canSet && prop.CanWrite;
                        info = prop;
                        return current;
                    }
                    else
                    {
                        if (!prop.CanRead)
                            throw Exception("Porperty has no getter: " + varName);

                        current = prop.GetValue(current, null);

                        if (current.GetType().IsValueType)
                            canSet = false;
                    }
                }
                // FIELD
                else
                {
                    FieldInfo mem = t.GetField(varName);
                    if (mem != null)
                    {
                        if (mem.IsStatic)
                            canSet = false;

                        if (final)
                        {
                            info = mem;
                            return current;
                        }
                        else
                        {
                            current = mem.GetValue(current);
                        }
                    }
                    else
                    {
                        throw Exception("Member not found: " + varName);
                    }
                }

            }

            throw Exception("this exception should never be reached.");
        }

        private GameObject FindObjectInHierachy(GameObject root, string hierachy)
        {
            Transform cur = root.transform;
            string[] parts = hierachy.Split('.');

            foreach(string part in parts)
            {
                if (part == HIERACHY_SELF || part == "." || string.IsNullOrEmpty(part))
                    continue;

                if(part == HIERACHY_PARENT)
                {
                    cur = cur.parent;
                    continue;
                }

                cur = cur.GetChildByName(part);

                if(cur == null)
                    return null;
            }

            return cur.gameObject;
        }

        private Exception Exception(string message)
        {
            return new Exception(this.ToString() + ": " + message);
        }

        public override string ToString()
        {
            string objName = (this.Object == null) ? "[null]" : Object.name;
            string parent = (string.IsNullOrEmpty(ComponentName)) ? "gameObject" : ComponentName;
            string var = (string.IsNullOrEmpty(VariableHierachy)) ? "[-]" : VariableHierachy;
            return "VariableAccessor " + objName + "->" + parent + "." + var;
        }

        public GameObject GetHierachyObject()
        {
            return FindObjectInHierachy(Object, ObjectHierachy);
        }
    }
}
