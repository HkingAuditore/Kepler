using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class RotationModifierEditor : SplineSampleModifierEditor
    {
        private readonly float addTime        = 0f;
        public           bool  allowSelection = true;

        public RotationModifierEditor(SplineUser user, SplineUserEditor parent, RotationModifier input) :
            base(user, parent, input)
        {
            title = "Rotation Modifiers";
        }

        public void ClearSelection()
        {
            selected = -1;
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (!isOpen) return;
            if (GUILayout.Button("Add New Rotation"))
            {
                ((RotationModifier) module).AddKey(Vector3.zero, addTime - 0.1, addTime + 0.1);
                user.Rebuild();
            }
        }

        protected override void KeyGUI(SplineSampleModifier.Key key)
        {
            var rotationKey = (RotationModifier.RotationKey) key;
            base.KeyGUI(key);
            if (!rotationKey.useLookTarget)
                rotationKey.rotation = EditorGUILayout.Vector3Field("Rotation", rotationKey.rotation);
            rotationKey.useLookTarget = EditorGUILayout.Toggle("Use Look Target", rotationKey.useLookTarget);
            if (rotationKey.useLookTarget)
                rotationKey.target =
                    (Transform) EditorGUILayout.ObjectField("Target", rotationKey.target, typeof(Transform), true);
        }

        protected override void KeyHandles(SplineSampleModifier.Key key, bool edit)
        {
            var rotationKey = (RotationModifier.RotationKey) key;
            var result      = new SplineSample();
            user.spline.Evaluate(rotationKey.position, result);
            if (rotationKey.useLookTarget)
            {
                if (rotationKey.target != null)
                {
                    Handles.DrawDottedLine(result.position, rotationKey.target.position, 5f);
                    if (edit)
                    {
                        var lastPos = rotationKey.target.position;
                        rotationKey.target.position =
                            Handles.PositionHandle(rotationKey.target.position, Quaternion.identity);
                        if (lastPos != rotationKey.target.position) user.Rebuild();
                    }
                }
            }
            else
            {
                var directionRot = Quaternion.LookRotation(result.forward, result.up);
                var rot          = directionRot * Quaternion.Euler(rotationKey.rotation);
                SplineEditorHandles.DrawArrowCap(result.position, rot, HandleUtility.GetHandleSize(result.position));

                if (edit)
                {
                    var lastEuler = rot.eulerAngles;
                    rot                  = Handles.RotationHandle(rot, result.position);
                    rot                  = Quaternion.Inverse(directionRot) * rot;
                    rotationKey.rotation = rot.eulerAngles;
                    if (rot.eulerAngles != lastEuler) user.Rebuild();
                }
            }

            base.KeyHandles(key, edit);
        }
    }
}