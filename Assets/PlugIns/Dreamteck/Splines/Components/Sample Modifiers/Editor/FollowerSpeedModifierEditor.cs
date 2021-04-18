using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class FollowerSpeedModifierEditor : SplineSampleModifierEditor
    {
        private readonly float addTime        = 0f;
        public           bool  allowSelection = true;

        public FollowerSpeedModifierEditor(SplineUser user, SplineUserEditor editor, FollowerSpeedModifier input) :
            base(user, editor, input)
        {
            module = input;
            title  = "Speed Modifiers";
        }

        public void ClearSelection()
        {
            selected = -1;
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (!isOpen) return;
            if (GUILayout.Button("Add Speed Region"))
            {
                ((FollowerSpeedModifier) module).AddKey(addTime - 0.1, addTime + 0.1);
                user.Rebuild();
            }
        }

        protected override void KeyGUI(SplineSampleModifier.Key key)
        {
            var offsetKey = (FollowerSpeedModifier.SpeedKey) key;
            base.KeyGUI(key);
            offsetKey.speed = EditorGUILayout.FloatField("Add Speed", offsetKey.speed);
        }
    }
}