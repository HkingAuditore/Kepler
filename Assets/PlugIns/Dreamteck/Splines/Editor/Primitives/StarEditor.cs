using Dreamteck.Splines.Editor;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
    public class StarEditor : PrimitiveEditor
    {
        public override string GetName()
        {
            return "Star";
        }

        public override void Open(DreamteckSplinesEditor editor)
        {
            base.Open(editor);
            primitive = new Star();
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            var star = (Star) primitive;
            star.radius = EditorGUILayout.FloatField("Radius", star.radius);
            star.depth  = EditorGUILayout.FloatField("Depth",  star.depth);
            star.sides  = EditorGUILayout.IntField("Sides", star.sides);
            if (star.sides < 3) star.sides = 3;
        }
    }
}