#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BitBarons.Bindings;
using BitBarons.Editor.Util;
using System.Collections.Generic;


namespace BitBarons.Editor.Bindings
{
    [CustomEditor(typeof(BindingBehaviour))]
    public class BindingBehaviour_Inspector : UnityEditor.Editor
    {
        VariableAccessorGui accessorGui;
        BindingBehaviour binding;

        List<BaseNamePartEditor> parts = new List<BaseNamePartEditor>();


        void OnEnable()
        {
            if(binding == null)
                binding = target as BindingBehaviour;

            if(accessorGui == null)
                accessorGui = new VariableAccessorGui(binding.Accessor, true);

            if (binding.Accessor.Object == null)
                binding.Accessor.Object = binding.gameObject;

            if (binding.BoundObject == null)
                binding.BoundObject = binding.gameObject;

            parts = SharedEditorMethods.GetEditorParts(binding.NameParts, binding.BoundObject, true);
        }
        
        public override void OnInspectorGUI()
        {
            using (EditorGuiZone.Vertical("Box"))
            {
                accessorGui.OnGUI();
            }

            SharedEditorMethods.NamePartsGUI(binding.BoundObject, parts, "bla", ref binding.Format,
                (o) => { binding.NameParts.Add(o.NamePart); }, // create
                (o) => { binding.NameParts.Remove(o.NamePart); }, // delete
                true);



            //check for changes in values
            if (GUI.changed)
            {

            }
        }
    }
}
#endif