using UnityEditor;

namespace Dreamteck.Splines.Editor
{
    public class SplineUserSubEditor
    {
        public    bool             alwaysOpen = false;
        protected SplineUserEditor editor;
        private   bool             foldout;
        protected string           title = "";
        protected SplineUser       user;

        public SplineUserSubEditor(SplineUser user, SplineUserEditor editor)
        {
            this.editor = editor;
            this.user   = user;
        }

        public bool isOpen => foldout || alwaysOpen;

        public virtual void DrawInspector()
        {
            if (!alwaysOpen) foldout = EditorGUILayout.Foldout(foldout, title);
        }

        public virtual void DrawScene()
        {
        }
    }
}