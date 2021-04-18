using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class MeshScaleModifierEditor : SplineSampleModifierEditor
    {
        private readonly float addTime        = 0f;
        public           bool  allowSelection = true;

        public MeshScaleModifierEditor(MeshGenerator user, SplineUserEditor editor, MeshScaleModifier input) :
            base(user, editor, input)
        {
            module = input;
            title  = "Scale Modifiers";
        }

        public void ClearSelection()
        {
            selected = -1;
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (!isOpen) return;
            if (GUILayout.Button("Add New Scale"))
            {
                ((MeshScaleModifier) module).AddKey(addTime - 0.1, addTime + 0.1);
                user.Rebuild();
            }
        }

        protected override void KeyGUI(SplineSampleModifier.Key key)
        {
            var scaleKey = (MeshScaleModifier.ScaleKey) key;
            base.KeyGUI(key);
            scaleKey.scale = EditorGUILayout.Vector2Field("Scale", scaleKey.scale);
        }
    }
}