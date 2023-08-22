using BitBarons.Util;
using System;
//using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BitBarons.Editor.Util
{
    public class VariableAccessorGui
    {
        public VariableAccessor Accessor = new VariableAccessor();
        string[] availableComponents, availableVariables, availableHierachyObjects;
        int componentIndex = 0;
        int variableIndex = 0;
        int objHierachyIndex;

        GameObject lastAccessorObject; 

        public bool DisplayObjectField;

        public VariableAccessorGui(VariableAccessType access, bool displayObjectField)
        {
            this.Accessor.AccessType = access;
            this.DisplayObjectField = displayObjectField;
        }

        public VariableAccessorGui(VariableAccessor variableAccessor, bool displayObjectField)
        {
            this.Accessor = variableAccessor;
            this.DisplayObjectField = displayObjectField;
        }


        public void OnGUI()
        {
            if (DisplayObjectField)
                Accessor.Object = EditorGUILayout.ObjectField("Parent Object", Accessor.Object, typeof(GameObject), true) as GameObject;


            if (Accessor.Object != null)
            {
                HierachyOnGui("Hierachy Object",
                    () => Accessor.ObjectHierachy,
                    (o) => Accessor.ObjectHierachy = o,
                    UpdateAvailableHierachy,
                    availableHierachyObjects, ref objHierachyIndex);

                if (lastAccessorObject != Accessor.Object)
                {
                    try
                    {
                        FilterComponents();
                        UpdateAvailableVariables();
                    }
                    catch (Exception)
                    {
                        componentIndex = 0;
                        variableIndex = 0;
                        Accessor.ComponentName = null;
                        Accessor.VariableHierachy = string.Empty;
                    }
                }

                if (availableComponents == null)
                    FilterComponents();

                var c = componentIndex;
                componentIndex = EditorGUILayout.Popup("Component", componentIndex, availableComponents);
                if (c != componentIndex)
                {
                    if (componentIndex == 0)
                        Accessor.ComponentName = null;
                    else
                        Accessor.ComponentName = availableComponents[componentIndex];

                    UpdateAvailableVariables();
                }



                HierachyOnGui("Variable", 
                    () => Accessor.VariableHierachy, 
                    (o) => Accessor.VariableHierachy = o,
                    UpdateAvailableVariables,
                    availableVariables, ref variableIndex);

                lastAccessorObject = Accessor.GetHierachyObject();
            }
        }


        private void HierachyOnGui(string FieldName, Func<string> getHierachyString, Action<string> setHierachyString, Action updateHierachy, string[] availableEntries, ref int entryIndex)
        {

            using (EditorGuiZone.Horizontal())
            {
                string h = getHierachyString();
                try
                {
                    setHierachyString(EditorGUILayout.TextField(FieldName, getHierachyString()));
                }
                catch (Exception) { }

                if (h != getHierachyString())
                    updateHierachy();

                if (!string.IsNullOrEmpty(getHierachyString()) && GUILayout.Button("←", GUILayout.Width(30)))
                {
                    RemoveLastVariable(getHierachyString, setHierachyString, updateHierachy);
                }


                if (availableEntries == null) 
                    updateHierachy();

                if (availableEntries.Length > 1)
                    entryIndex = EditorGUILayout.Popup(entryIndex, availableEntries, GUILayout.Width(40));

                if (entryIndex != 0)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(getHierachyString()))
                            setHierachyString(availableEntries[entryIndex]);
                        else
                            setHierachyString(getHierachyString() + "." + availableEntries[entryIndex]);

                        updateHierachy();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Could not add: " + ex.Message);
                        RemoveLastVariable(getHierachyString, setHierachyString, updateHierachy);
                    }
                }

            }

        }


        private void RemoveLastVariable(Func<string> getHierachyString, Action<string> setHierachyString, Action updateHierachy)
        {
            var before = getHierachyString().Split('.');
            string after = "";
            for (int i = 0; i < before.Length - 1; i++)
                after += before[i] + ".";
            after = after.TrimEnd('.');
            setHierachyString(after);

            updateHierachy();
        }
        private void UpdateAvailableHierachy()
        {
            List<string> list = new List<string>();
            list.Add("+");
            list.Add(VariableAccessor.HIERACHY_SELF);
            list.Add(VariableAccessor.HIERACHY_PARENT);

            list.AddRange(Accessor.GetHierachyObject().transform.GetAllChilds().Select((o) => o.name));
            list.Sort();

            availableHierachyObjects = list.ToArray();
            objHierachyIndex = 0;

            FilterComponents();
            UpdateAvailableVariables();
        }

        private void UpdateAvailableVariables()
        {
            List<string> list = new List<string>();
            list.Add("+");
            list.AddRange(Accessor.GetAllowedSubVariables());
            list.Sort();

            availableVariables = list.ToArray();
            variableIndex = 0;
        }
        private void FilterComponents()
        {
            List<string> list = new List<string>();

            list.Add("<GameObject>");

            UnityEngine.Object[] dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { Accessor.Object });

            foreach(var o in dependencies)
            {
                if (o.GetClassName() == "Behaviour")
                    continue;

                if (o is Component)
                {
                    list.Add(o.GetClassName());
                }
            }

            availableComponents = list.ToArray();
        }
    }
}