using System;
using Dreamteck.Splines.Editor;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
    [Serializable]
    public class PrimitiveEditor
    {
        [NonSerialized] protected DreamteckSplinesEditor editor;

        [NonSerialized] public Vector3 origin = Vector3.zero;

        protected SplinePrimitive primitive = new SplinePrimitive();

        public virtual string GetName()
        {
            return "Primitive";
        }

        public virtual void Open(DreamteckSplinesEditor editor)
        {
            this.editor    = editor;
            primitive.is2D = editor.is2D;
            primitive.Calculate();
        }

        public void Draw()
        {
            EditorGUI.BeginChangeCheck();
            OnGUI();
            if (EditorGUI.EndChangeCheck()) Update();
        }

        public void Update()
        {
            primitive.is2D = editor.is2D;
            primitive.Calculate();
            editor.points     = primitive.GetPoints();
            editor.splineType = primitive.GetSplineType();
            editor.isClosed   = primitive.GetIsClosed();
        }

        protected virtual void OnGUI()
        {
            primitive.is2D   = editor.is2D;
            primitive.offset = EditorGUILayout.Vector3Field("Offset", primitive.offset);
            if (editor.is2D)
            {
                var rot = primitive.rotation.z;
                rot                = EditorGUILayout.FloatField("Rotation", rot);
                primitive.rotation = new Vector3(0f, 0f, rot);
            }
            else
            {
                primitive.rotation = EditorGUILayout.Vector3Field("Rotation", primitive.rotation);
            }
        }
    }
}