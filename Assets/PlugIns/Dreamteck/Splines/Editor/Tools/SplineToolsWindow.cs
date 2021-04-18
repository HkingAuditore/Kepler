using System;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class SplineToolsWindow : EditorWindow
    {
        private const    float        menuWidth = 150f;
        private static   SplineTool[] tools;
        private readonly Vector2      scroll    = Vector2.zero;
        private          int          toolIndex = -1;

        private void Awake()
        {
            titleContent             = new GUIContent("Spline Tools");
            name                     = "Spline tools";
            autoRepaintOnSceneChange = true;

            var types = typeof(SplineTool).GetAllDerivedClasses();
            tools = new SplineTool[types.Count];
            var count = 0;
            foreach (var t in types)
            {
                tools[count] = (SplineTool) Activator.CreateInstance(t);
                count++;
            }

            if (toolIndex >= 0 && toolIndex < tools.Length) tools[toolIndex].Open(this);
        }

        private void OnDestroy()
        {
            if (toolIndex >= 0 && toolIndex < tools.Length) tools[toolIndex].Close();
        }

        private void OnGUI()
        {
            if (tools == null) Awake();
            GUI.color = new Color(0f, 0f, 0f, 0.15f);
            GUI.DrawTexture(new Rect(0, 0, menuWidth, position.height), SplineEditorGUI.white, ScaleMode.StretchToFill);
            GUI.color = Color.white;
            GUILayout.BeginHorizontal();
            GUILayout.BeginScrollView(scroll, GUILayout.Width(menuWidth), GUILayout.Height(position.height - 10));
            if (tools == null) Init();
            SplineEditorGUI.SetHighlightColors(SplinePrefs.highlightColor, SplinePrefs.highlightContentColor);
            for (var i = 0; i < tools.Length; i++)
                if (SplineEditorGUI.EditorLayoutSelectableButton(new GUIContent(tools[i].GetName()), true,
                                                                 toolIndex == i))
                {
                    if (toolIndex >= 0 && toolIndex < tools.Length) tools[toolIndex].Close();
                    toolIndex = i;
                    if (toolIndex < tools.Length) tools[toolIndex].Open(this);
                }

            GUILayout.EndScrollView();


            if (toolIndex >= 0 && toolIndex < tools.Length)
            {
                GUILayout.BeginVertical();
                tools[toolIndex].Draw(new Rect(menuWidth, 0, position.width - menuWidth - 5f, position.height - 10));
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
        }

        [MenuItem("Window/Dreamteck/Splines/Tools")]
        private static void Init()
        {
            var window = (SplineToolsWindow) GetWindow(typeof(SplineToolsWindow));
            window.Show();
        }
    }
}