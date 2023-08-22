using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BitBarons.Editor.Util
{
    public enum EditorZoneType
    {
        Vertical,
        Horizontal,
        ScrollView,
        ToggleGroup,
    }

    public class EditorGuiZone : IDisposable
    {
        EditorZoneType zoneType;

        public static EditorGuiZone Horizontal(params UnityEngine.GUILayoutOption[] options) { return new EditorGuiZone(EditorZoneType.Horizontal, options); }
        public static EditorGuiZone Horizontal(GUIStyle style, params UnityEngine.GUILayoutOption[] options) { return new EditorGuiZone(EditorZoneType.Horizontal, style, options); }
        public static EditorGuiZone Vertical(params UnityEngine.GUILayoutOption[] options) { return new EditorGuiZone(EditorZoneType.Vertical, options); }
        public static EditorGuiZone Vertical(GUIStyle style, params UnityEngine.GUILayoutOption[] options) { return new EditorGuiZone(EditorZoneType.Vertical, style, options); }

        public static EditorGuiZone ToggleGroup(GUIContent label, ref bool toggle, params UnityEngine.GUILayoutOption[] options)
        {
            var zone = new EditorGuiZone(EditorZoneType.ToggleGroup);
            toggle = EditorGUILayout.BeginToggleGroup(label, toggle);
            return zone;
        }
        public static EditorGuiZone ToggleGroup(string label, ref bool toggle, params UnityEngine.GUILayoutOption[] options)
        {
            var zone = new EditorGuiZone(EditorZoneType.ToggleGroup);
            toggle = EditorGUILayout.BeginToggleGroup(label, toggle);
            return zone;
        }
        public static EditorGuiZone ScrollView(ref Vector2 scrollPos, params UnityEngine.GUILayoutOption[] options)
        {
            var zone = new EditorGuiZone(EditorZoneType.ScrollView);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, options);
            return zone;
        }

        public static bool FoldOut(ref bool foldout, string content)
        {
            foldout = EditorGUILayout.Foldout(foldout, content);
            return foldout;
        }

        private EditorGuiZone(EditorZoneType zoneType)
        {
            this.zoneType = zoneType;
        }

        private EditorGuiZone(EditorZoneType zoneType, params UnityEngine.GUILayoutOption[] options)
        {
            this.zoneType = zoneType;
            switch (zoneType)
            {
                case EditorZoneType.Horizontal:
                    EditorGUILayout.BeginHorizontal(options);
                    break;
                case EditorZoneType.Vertical:
                    EditorGUILayout.BeginVertical(options);
                    break;
            }

        }
        private EditorGuiZone(EditorZoneType zoneType, GUIStyle style, params GUILayoutOption[] options)
        {
            this.zoneType = zoneType;
            switch (zoneType)
            {
                case EditorZoneType.Horizontal:
                    EditorGUILayout.BeginHorizontal(style, options);
                    break;
                case EditorZoneType.Vertical:
                    EditorGUILayout.BeginVertical(style, options);
                    break;
            }
        }
        public void Dispose()
        {
            switch (zoneType)
            {
                case EditorZoneType.Horizontal:
                    EditorGUILayout.EndHorizontal();
                    break;
                case EditorZoneType.Vertical:
                    EditorGUILayout.EndVertical();
                    break;
                case EditorZoneType.ToggleGroup:
                    EditorGUILayout.EndToggleGroup();
                    break;
                case EditorZoneType.ScrollView:
                    EditorGUILayout.EndScrollView();
                    break;
            }
        }
    }
}