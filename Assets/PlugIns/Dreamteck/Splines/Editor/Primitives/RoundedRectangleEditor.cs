using Dreamteck.Splines.Editor;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
    public class RoundedRectangleEditor : PrimitiveEditor
    {
        public override string GetName()
        {
            return "Rounded Rect";
        }

        public override void Open(DreamteckSplinesEditor editor)
        {
            base.Open(editor);
            primitive        = new RoundedRectangle();
            primitive.offset = origin;
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            var rect = (RoundedRectangle) primitive;
            rect.size    = EditorGUILayout.Vector2Field("Size", rect.size);
            rect.xRadius = EditorGUILayout.FloatField("X Radius", rect.xRadius);
            rect.yRadius = EditorGUILayout.FloatField("Y Radius", rect.yRadius);
        }
    }
}