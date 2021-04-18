using Dreamteck.Splines.Editor;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
    public class EllipseEditor : PrimitiveEditor
    {
        public override string GetName()
        {
            return "Ellipse";
        }

        public override void Open(DreamteckSplinesEditor editor)
        {
            base.Open(editor);
            primitive        = new Ellipse();
            primitive.offset = origin;
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            var ellipse = (Ellipse) primitive;
            ellipse.xRadius = EditorGUILayout.FloatField("X Radius", ellipse.xRadius);
            ellipse.yRadius = EditorGUILayout.FloatField("Y Radius", ellipse.yRadius);
        }
    }
}