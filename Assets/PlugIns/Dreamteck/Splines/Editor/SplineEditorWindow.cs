using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class SplineEditorWindow : EditorWindow
    {
        protected UnityEditor.Editor   editor;
        protected SplineComputerEditor splineEditor;

        public void Init(UnityEditor.Editor e, string inputTitle, Vector2 min, Vector2 max)
        {
            minSize = min;
            maxSize = max;
            Init(e, inputTitle);
        }

        public void Init(UnityEditor.Editor e, Vector2 min, Vector2 max)
        {
            minSize = min;
            maxSize = max;
            Init(e);
        }

        public void Init(UnityEditor.Editor e, Vector2 size)
        {
            minSize = maxSize = size;
            Init(e);
        }

        public void Init(UnityEditor.Editor e, string inputTitle)
        {
            Init(e);
            Title(inputTitle);
        }

        public void Init(UnityEditor.Editor e)
        {
            editor = e;
            if (editor is SplineComputerEditor) splineEditor = (SplineComputerEditor) editor;
            else splineEditor                                = null;
            Title(GetTitle());
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual string GetTitle()
        {
            return "Spline Editor Window";
        }

        private void Title(string inputTitle)
        {
            titleContent = new GUIContent(inputTitle);
        }
    }
}