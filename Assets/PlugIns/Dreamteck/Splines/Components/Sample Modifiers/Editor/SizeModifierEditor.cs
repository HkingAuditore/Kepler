using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class SizeModifierEditor : SplineSampleModifierEditor
    {
        private readonly float addTime        = 0f;
        public           bool  allowSelection = true;

        public SizeModifierEditor(SplineUser user, SplineUserEditor editor, SizeModifier input) :
            base(user, editor, input)
        {
            module = input;
            title  = "Size Modifiers";
        }

        public void ClearSelection()
        {
            selected = -1;
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (!isOpen) return;
            if (GUILayout.Button("Add New Size"))
            {
                ((SizeModifier) module).AddKey(addTime - 0.1, addTime + 0.1);
                user.Rebuild();
            }
        }

        protected override void KeyGUI(SplineSampleModifier.Key key)
        {
            var offsetKey = (SizeModifier.SizeKey) key;
            base.KeyGUI(key);
            offsetKey.size = EditorGUILayout.FloatField("Size", offsetKey.size);
        }
    }
}