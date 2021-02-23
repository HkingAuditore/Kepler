#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dreamteck.Splines
{
    public static class SplinePrefs
    {
        private static bool loaded = false;
        public static Spline.Direction duplicationDirection = Spline.Direction.Forward;
        public static bool defaultAlwaysDraw = false;
        public static bool defaultShowThickness = false;
        public static bool default2D = false;
        public static bool startInCreationMode = false;
        public static SplineComputer.Space pointEditSpace = SplineComputer.Space.Local;
        public static Color defaultColor = Color.white;
        public static Color highlightColor = Color.white;
        public static Color highlightContentColor = new Color(1f, 1f, 1f, 0.95f);
        public static bool showPointNumbers = false;
        public static SplineComputer.Space defaultComputerSpace = SplineComputer.Space.Local;
        public static Spline.Type defaultType = Spline.Type.CatmullRom;
        public static float createPointSize = 1f;
        public static Color createPointColor = Color.white;

        static SplinePrefs()
        {
            LoadPrefs();
        }

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider SplinesSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider("Dreamteck/Splines", SettingsScope.User)
            {
                label = "Splines",
                guiHandler = (searchContext) =>
                {
                    OnGUI();
                },
                keywords = new HashSet<string>(new[] { "Dreamteck", "Splines", "Path", "Curve"})
            };

            return provider;
        }
#else
        [PreferenceItem("DTK Splines")]
#endif
        public static void OnGUI()
        {
            if (!loaded) LoadPrefs();
            EditorGUILayout.LabelField("Newly created splines:", EditorStyles.boldLabel);
            startInCreationMode = EditorGUILayout.Toggle("Start in Creation Mode", startInCreationMode);
            defaultComputerSpace = (SplineComputer.Space)EditorGUILayout.EnumPopup("Space", defaultComputerSpace);
            defaultType = (Spline.Type)EditorGUILayout.EnumPopup("Type", defaultType);
            defaultAlwaysDraw = EditorGUILayout.Toggle("Always draw", defaultAlwaysDraw);
            defaultShowThickness = EditorGUILayout.Toggle("Show thickness", defaultShowThickness);
            default2D = EditorGUILayout.Toggle("2D Mode", default2D);
            defaultColor = EditorGUILayout.ColorField("Spline color", defaultColor);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Newly created points:", EditorStyles.boldLabel);
            createPointSize = EditorGUILayout.FloatField("Default Size", createPointSize);
            createPointColor = EditorGUILayout.ColorField("Default Color", createPointColor);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);
            highlightColor = EditorGUILayout.ColorField("Highlight color", highlightColor);
            highlightContentColor = EditorGUILayout.ColorField("Highlight content color", highlightContentColor);
            duplicationDirection = (Spline.Direction)EditorGUILayout.EnumPopup("Duplicate Direction", duplicationDirection);
            showPointNumbers = EditorGUILayout.Toggle("Show point numbers", showPointNumbers);

            if (GUILayout.Button("Use Defaults", GUILayout.Width(120)))
            {
                duplicationDirection = Spline.Direction.Forward;
                defaultAlwaysDraw = false;
                defaultShowThickness = false;
                default2D = false;
                startInCreationMode = true;
                defaultColor = Color.white;
                highlightColor = new Color(0f, 0.564f, 1f, 1f);
                highlightContentColor = new Color(1f, 1f, 1f, 0.95f);
                showPointNumbers = false;
                defaultComputerSpace = SplineComputer.Space.Local;
                defaultType = Spline.Type.CatmullRom;
                createPointSize = 1f;
                createPointColor = Color.white;
                SavePrefs();
            }
            if (GUI.changed) SavePrefs();
        }

        public static void LoadPrefs()
        {
            defaultAlwaysDraw = EditorPrefs.GetBool("Dreamteck.Splines.defaultAlwaysDraw", false);
            defaultShowThickness = EditorPrefs.GetBool("Dreamteck.Splines.defaultShowThickness", false);
            default2D = EditorPrefs.GetBool("Dreamteck.Splines.default2D", false);
            startInCreationMode = EditorPrefs.GetBool("Dreamteck.Splines.startInCreationMode", true);
            showPointNumbers = EditorPrefs.GetBool("Dreamteck.Splines.showPointNumbers", false);
            pointEditSpace = (SplineComputer.Space)EditorPrefs.GetInt("Dreamteck.Splines.pointEditSpace", 1);
            defaultColor = LoadColor("Dreamteck.Splines.defaultColor", Color.white);
            highlightColor = LoadColor("Dreamteck.Splines.highlightColor", new Color(0f, 0.564f, 1f, 1f));
            highlightContentColor = LoadColor("Dreamteck.Splines.highlightContentColor", new Color(1f, 1f, 1f, 0.95f));
            defaultComputerSpace = (SplineComputer.Space)EditorPrefs.GetInt("Dreamteck.Splines.defaultComputerSpace", 0);
            defaultType = (Spline.Type)EditorPrefs.GetInt("Dreamteck.Splines.defaultType", 0);
            duplicationDirection = (Spline.Direction)EditorPrefs.GetInt("Dreamteck.Splines.duplicationDirection", 0);
            createPointSize = EditorPrefs.GetFloat("Dreamteck.Splines.createPointSize", 1f);
            createPointColor = LoadColor("Dreamteck.Splines.createPointColor", Color.white);
            loaded = true;
        }

        private static Color LoadColor(string name, Color defaultValue)
        {
            Color col = Color.white;
            string colorString = EditorPrefs.GetString(name, defaultValue.r+":"+defaultValue.g+ ":" + defaultValue.b+ ":" + defaultValue.a);
            string[] elements = colorString.Split(':');
            if (elements.Length < 4) return col;
            float r = 0f, g = 0f, b = 0f, a = 0f;
            float.TryParse(elements[0], out r);
            float.TryParse(elements[1], out g);
            float.TryParse(elements[2], out b);
            float.TryParse(elements[3], out a);
            col = new Color(r, g, b, a);
            return col;
        }

        public static void SavePrefs()
        {
            EditorPrefs.SetBool("Dreamteck.Splines.defaultAlwaysDraw", defaultAlwaysDraw);
            EditorPrefs.SetBool("Dreamteck.Splines.defaultShowThickness", defaultShowThickness);
            EditorPrefs.SetBool("Dreamteck.Splines.default2D", default2D);
            EditorPrefs.SetBool("Dreamteck.Splines.showPointNumbers", showPointNumbers);
            EditorPrefs.SetInt("Dreamteck.Splines.pointEditSpace", (int)pointEditSpace);
            EditorPrefs.SetString("Dreamteck.Splines.defaultColor", defaultColor.r+ ":" + defaultColor.g+ ":" + defaultColor.b+ ":" + defaultColor.a);
            EditorPrefs.SetString("Dreamteck.Splines.highlightColor", highlightColor.r + ":" + highlightColor.g + ":" + highlightColor.b + ":" + highlightColor.a);
            EditorPrefs.SetString("Dreamteck.Splines.highlightContentColor", highlightContentColor.r + ":" + highlightContentColor.g + ":" + highlightContentColor.b + ":" + highlightContentColor.a);
            EditorPrefs.SetInt("Dreamteck.Splines.defaultComputerSpace", (int)defaultComputerSpace);
            EditorPrefs.SetInt("Dreamteck.Splines.defaultType", (int)defaultType);
            EditorPrefs.SetInt("Dreamteck.Splines.duplicationDirection", (int)duplicationDirection);
            EditorPrefs.SetFloat("Dreamteck.Splines.createPointSize", createPointSize);
            EditorPrefs.SetString("Dreamteck.Splines.createPointColor", createPointColor.r + ":" + createPointColor.g + ":" + createPointColor.b + ":" + createPointColor.a);
        }
    }
}
#endif
