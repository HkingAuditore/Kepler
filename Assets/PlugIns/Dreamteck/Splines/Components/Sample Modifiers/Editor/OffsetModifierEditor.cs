using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class OffsetModifierEditor : SplineSampleModifierEditor
    {
        private readonly float     addTime        = 0f;
        public           bool      allowSelection = true;
        private          Matrix4x4 matrix;

        public OffsetModifierEditor(SplineUser user, SplineUserEditor editor, OffsetModifier input) :
            base(user, editor, input)
        {
            module = input;
            title  = "Offset Modifiers";
        }

        public void ClearSelection()
        {
            selected = -1;
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (!isOpen) return;
            if (GUILayout.Button("Add New Offset"))
            {
                ((OffsetModifier) module).AddKey(Vector2.zero, addTime - 0.1, addTime + 0.1);
                user.Rebuild();
            }
        }

        protected override void KeyGUI(SplineSampleModifier.Key key)
        {
            var offsetKey = (OffsetModifier.OffsetKey) key;
            base.KeyGUI(key);
            offsetKey.offset = EditorGUILayout.Vector2Field("Offset", offsetKey.offset);
        }

        protected override void KeyHandles(SplineSampleModifier.Key key, bool edit)
        {
            if (!isOpen) return;
            var is2D      = user.spline != null && user.spline.is2D;
            var result    = new SplineSample();
            var offsetKey = (OffsetModifier.OffsetKey) key;
            user.spline.Evaluate(offsetKey.position, result);
            matrix.SetTRS(result.position, Quaternion.LookRotation(result.forward, result.up),
                          Vector3.one * result.size);
            var pos = matrix.MultiplyPoint(offsetKey.offset);
            if (is2D)
            {
                Handles.DrawLine(result.position, result.position + result.right * offsetKey.offset.x * result.size);
                Handles.DrawLine(result.position, result.position - result.right * offsetKey.offset.x * result.size);
            }
            else
            {
                Handles.DrawWireDisc(result.position, result.forward, offsetKey.offset.magnitude * result.size);
            }

            Handles.DrawLine(result.position, pos);

            if (edit)
            {
                var lastPos = pos;
                pos = SplineEditorHandles.FreeMoveRectangle(pos, HandleUtility.GetHandleSize(pos) * 0.1f);
                if (pos != lastPos)
                {
                    pos   = matrix.inverse.MultiplyPoint(pos);
                    pos.z = 0f;
                    if (is2D) offsetKey.offset = Vector2.right * pos.x;
                    else offsetKey.offset      = pos;
                    user.Rebuild();
                }
            }

            base.KeyHandles(key, edit);
        }
    }
}