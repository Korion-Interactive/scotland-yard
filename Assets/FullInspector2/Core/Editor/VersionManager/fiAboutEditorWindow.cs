using FullInspector.Internal.Versioning;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Internal {
    public class fiAboutEditorWindow : EditorWindow {
        private static Texture2D _cachedLogoTexture;
        private static Vector2 _logoSize;

        [MenuItem("Window/Full Inspector/About")]
        public static void ShowWindow() {
            var window = EditorWindow.GetWindow<fiAboutEditorWindow>(/*utility:*/ true);

            window.minSize = new Vector2(883, 365);
            window.maxSize = new Vector2(883, 365);
            window.position = new Rect(window.position.x + 20, window.position.y + 20, 883, 365);

			window.titleContent.text = "About Full Inspector";
        }

        public void OnGUI() {
            EditorGUILayout.Space();

            LoadResourceAssets();
            var logoRect = EditorGUILayout.GetControlRect(/*hasLabel:*/ false, /*height:*/ _logoSize.y);
            GUI.DrawTexture(logoRect, _cachedLogoTexture, ScaleMode.ScaleToFit);

            EditorGUILayout.Space();

            var linksRect = EditorGUILayout.BeginHorizontal();
            linksRect.x += 4;
            linksRect.width -= 4;
            GUI.Box(linksRect, GUIContent.none);
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("View Product Website", EditorStyles.boldLabel)) {
                Application.OpenURL("http://jacobdufault.github.io/fullinspector/");
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("View Documentation", EditorStyles.boldLabel)) {
                Application.OpenURL("http://jacobdufault.github.io/fullinspector/guide/");
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Report an Issue", EditorStyles.boldLabel)) {
                Application.OpenURL("http://www.github.com/jacobdufault/fullinspector/issues");
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Forum Topic", EditorStyles.boldLabel)) {
                Application.OpenURL("http://forum.unity3d.com/threads/full-inspector-inspector-and-serialization-for-structs-dicts-generics-interfaces.224270/");
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Download Samples", EditorStyles.boldLabel)) {
                Application.OpenURL("http://www.github.com/jacobdufault/fullinspectorsamples");
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Full Inspector is property of Jacob Dufault");
            GUILayout.FlexibleSpace();
            GUILayout.Label("Current Version: " + fiVersionManager.CurrentVersion);
            GUILayout.EndHorizontal();
        }


        // note: from Rotorz
        private static void LoadResourceAssets() {
            if (_cachedLogoTexture != null) return;

            byte[] imageData = File.ReadAllBytes(Path.Combine(fiSettings.RootDirectory, "Core/Editor/VersionManager/fi-logo.png"));

            // Gather image size from image data.
            int texWidth, texHeight;
            GetImageSize(imageData, out texWidth, out texHeight);
            _logoSize = new Vector2(texWidth, texHeight);

            // Generate texture asset.
            var tex = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
            tex.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            tex.name = "FullInspector2 Logo";
            tex.filterMode = FilterMode.Point;
            tex.LoadImage(imageData);

            UpdateColorSpace(tex);
            _cachedLogoTexture = tex;
        }

        /// <summary>
        /// Converts the color space of the texture if necessary so that it
        /// will display properly.
        /// </summary>
        private static void UpdateColorSpace(Texture2D texture) {
            if (QualitySettings.activeColorSpace == ColorSpace.Linear) {
                Color[] pixels = texture.GetPixels();

                for (int i = 0; i < pixels.Length; ++i) {
                    Color pixel = pixels[i];
                    pixel.r = Mathf.LinearToGammaSpace(pixel.r);
                    pixel.g = Mathf.LinearToGammaSpace(pixel.g);
                    pixel.b = Mathf.LinearToGammaSpace(pixel.b);
                    pixel.a = Mathf.LinearToGammaSpace(pixel.a);
                    pixels[i] = pixel;
                }

                texture.SetPixels(pixels);
                texture.Apply();
            }
        }

        // note: from Rotorz
        private static void GetImageSize(byte[] imageData, out int width, out int height) {
            width = ReadInt(imageData, 3 + 15);
            height = ReadInt(imageData, 3 + 15 + 2 + 2);
        }

        // note: from Rotorz
        private static int ReadInt(byte[] imageData, int offset) {
            return (imageData[offset] << 8) | imageData[offset + 1];
        }
    }
}