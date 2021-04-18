using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class SplineEditorBase
    {
        public delegate void EmptyHandler();

        public delegate void UndoHandler(string title);

        public EditorGUIEvents eventModule;
        public bool            open;
        public EmptyHandler    repaintHandler;

        public UndoHandler undoHandler;

        public SplineEditorBase()
        {
            Load();
            eventModule = new EditorGUIEvents();
        }

        protected bool gizmosEnabled { get; private set; } = true;

        public virtual void Destroy()
        {
            Save();
        }

        protected virtual void Load()
        {
            open = LoadBool("open");
        }

        protected virtual void Save()
        {
            SaveBool("open", open);
        }

        public virtual void DrawInspector()
        {
            if (SceneView.lastActiveSceneView != null)
            {
#if UNITY_2019_1_OR_NEWER
                gizmosEnabled = SceneView.lastActiveSceneView.drawGizmos;
#endif
            }

            eventModule.Update(Event.current);
        }

        public virtual void DrawScene(SceneView current)
        {
            eventModule.Update(Event.current);
        }

        protected virtual void RecordUndo(string title)
        {
            if (undoHandler != null) undoHandler(title);
        }

        protected virtual void Repaint()
        {
            if (repaintHandler != null) repaintHandler();
        }

        public virtual void UndoRedoPerformed()
        {
        }

        protected string GetSaveName(string valueName)
        {
            return GetType().FullName + "." + valueName;
        }

        protected void SaveBool(string variableName, bool value)
        {
            EditorPrefs.SetBool(GetType() + "." + variableName, value);
        }

        protected void SaveInt(string variableName, int value)
        {
            EditorPrefs.SetInt(GetType() + "." + variableName, value);
        }

        protected void SaveFloat(string variableName, float value)
        {
            EditorPrefs.SetFloat(GetType() + "." + variableName, value);
        }

        protected void SaveString(string variableName, string value)
        {
            EditorPrefs.SetString(GetType() + "." + variableName, value);
        }

        protected bool LoadBool(string variableName, bool defaultValue = false)
        {
            return EditorPrefs.GetBool(GetType() + "." + variableName, defaultValue);
        }

        protected int LoadInt(string variableName, int defaultValue = 0)
        {
            return EditorPrefs.GetInt(GetType() + "." + variableName, defaultValue);
        }

        protected float LoadFloat(string variableName, float d = 0f)
        {
            return EditorPrefs.GetFloat(GetType() + "." + variableName, d);
        }

        protected string LoadString(string variableName)
        {
            return EditorPrefs.GetString(GetType() + "." + variableName, "");
        }
    }
}