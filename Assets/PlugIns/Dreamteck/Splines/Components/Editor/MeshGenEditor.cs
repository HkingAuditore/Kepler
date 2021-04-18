using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(MeshGenerator))]
    [CanEditMultipleObjects]
    public class MeshGenEditor : SplineUserEditor
    {
        private BakeMeshWindow bakeWindow;
        private int            framesPassed;

        private   MeshGenerator[] generators      = new MeshGenerator[0];
        protected bool            showColor       = true;
        protected bool            showDoubleSided = true;
        protected bool            showFlipFaces   = true;
        protected bool            showInfo;
        protected bool            showNormalMethod = true;
        protected bool            showOffset       = true;
        protected bool            showRotation     = true;
        protected bool            showSize         = true;
        protected bool            showTangents     = true;

        private bool verticesFoldout;

        protected override void Awake()
        {
            var generator = (MeshGenerator) target;
            var rend      = generator.GetComponent<MeshRenderer>();
            if (rend == null) return;
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            generators = new MeshGenerator[targets.Length];
            for (var i = 0; i < targets.Length; i++) generators[i] = (MeshGenerator) targets[i];
            var user                                               = (MeshGenerator) target;
        }

        protected override void OnDestroy()
        {
            var generator = (MeshGenerator) target;
            base.OnDestroy();
            var gen = (MeshGenerator) target;
            if (gen                              == null) return;
            if (gen.GetComponent<MeshCollider>() != null) generator.UpdateCollider();
            if (bakeWindow                       != null) bakeWindow.Close();
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            var generator = (MeshGenerator) target;
            if (Application.isPlaying) return;
            framesPassed++;
            if (framesPassed >= 100)
            {
                framesPassed = 0;
                if (generator != null && generator.GetComponent<MeshCollider>() != null) generator.UpdateCollider();
            }
        }

        public override void OnInspectorGUI()
        {
            var generator = (MeshGenerator) target;
            if (generator.baked)
            {
                SplineEditorGUI.SetHighlightColors(SplinePrefs.highlightColor, SplinePrefs.highlightContentColor);
                if (SplineEditorGUI
                   .EditorLayoutSelectableButton(new GUIContent("Revert Bake", "Makes the mesh dynamic again and allows editing"),
                                                 true, true))
                    for (var i = 0; i < generators.Length; i++)
                    {
                        generators[i].Unbake();
                        EditorUtility.SetDirty(generators[i]);
                    }

                return;
            }

            base.OnInspectorGUI();
        }

        protected override void BodyGUI()
        {
            base.BodyGUI();
            var generator = (MeshGenerator) target;
            serializedObject.Update();
            var calculateTangents = serializedObject.FindProperty("_calculateTangents");
            var markDynamic       = serializedObject.FindProperty("_markDynamic");
            var size              = serializedObject.FindProperty("_size");
            var color             = serializedObject.FindProperty("_color");
            var normalMethod      = serializedObject.FindProperty("_normalMethod");
            var useSplineSize     = serializedObject.FindProperty("_useSplineSize");
            var useSplineColor    = serializedObject.FindProperty("_useSplineColor");
            var offset            = serializedObject.FindProperty("_offset");
            var rotation          = serializedObject.FindProperty("_rotation");
            var flipFaces         = serializedObject.FindProperty("_flipFaces");
            var doubleSided       = serializedObject.FindProperty("_doubleSided");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();

            verticesFoldout = EditorGUILayout.Foldout(verticesFoldout, "Vertices", foldoutHeaderStyle);
            if (verticesFoldout)
            {
                EditorGUI.indentLevel++;
                if (showSize) EditorGUILayout.PropertyField(size,                 new GUIContent("Size"));
                if (showColor) EditorGUILayout.PropertyField(color,               new GUIContent("Color"));
                if (showNormalMethod) EditorGUILayout.PropertyField(normalMethod, new GUIContent("Normal Method"));
                if (showOffset) EditorGUILayout.PropertyField(offset,             new GUIContent("Offset"));
                if (showRotation) EditorGUILayout.PropertyField(rotation,         new GUIContent("Rotation"));
                if (showTangents)
                    EditorGUILayout.PropertyField(calculateTangents, new GUIContent("Calculate Tangents"));
                EditorGUILayout.PropertyField(useSplineSize,  new GUIContent("Use Spline Size"));
                EditorGUILayout.PropertyField(useSplineColor, new GUIContent("Use Spline Color"));
                EditorGUILayout.PropertyField(markDynamic,
                                              new GUIContent("Mark Dynamic",
                                                             "Improves performance in situations where the mesh changes frequently"));
                EditorGUI.indentLevel--;
            }

            if (showDoubleSided || showFlipFaces)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Faces", EditorStyles.boldLabel);
                if (showDoubleSided) EditorGUILayout.PropertyField(doubleSided, new GUIContent("Double-sided"));
                if (!generator.doubleSided && showFlipFaces)
                    EditorGUILayout.PropertyField(flipFaces, new GUIContent("Flip Faces"));
            }

            if (generator.GetComponent<MeshCollider>() != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Mesh Collider", EditorStyles.boldLabel);
                generator.colliderUpdateRate =
                    EditorGUILayout.FloatField("Collider Update Iterval", generator.colliderUpdateRate);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                for (var i = 0; i < generators.Length; i++) generators[i].Rebuild();
            }
        }

        protected override void FooterGUI()
        {
            base.FooterGUI();
            showInfo = EditorGUILayout.Foldout(showInfo, "Info & Components");
            if (showInfo)
            {
                var generator = (MeshGenerator) target;
                var filter    = generator.GetComponent<MeshFilter>();
                if (filter == null) return;
                var renderer = generator.GetComponent<MeshRenderer>();
                var str      = "";
                if (filter.sharedMesh != null)
                    str = "Vertices: " + filter.sharedMesh.vertexCount + "\r\nTriangles: " +
                          filter.sharedMesh.triangles.Length / 3;
                else str = "No info available";
                EditorGUILayout.HelpBox(str, MessageType.Info);
                var showFilter = filter.hideFlags == HideFlags.None;
                var last       = showFilter;
                showFilter = EditorGUILayout.Toggle("Show Mesh Filter", showFilter);
                if (last != showFilter)
                {
                    if (showFilter) filter.hideFlags = HideFlags.None;
                    else filter.hideFlags            = HideFlags.HideInInspector;
                }

                var showRenderer = renderer.hideFlags == HideFlags.None;
                last         = showRenderer;
                showRenderer = EditorGUILayout.Toggle("Show Mesh Renderer", showRenderer);
                if (last != showRenderer)
                {
                    if (showRenderer) renderer.hideFlags = HideFlags.None;
                    else renderer.hideFlags              = HideFlags.HideInInspector;
                }
            }

            if (generators.Length == 1)
                if (GUILayout.Button("Bake Mesh"))
                {
                    var generator = (MeshGenerator) target;
                    bakeWindow = EditorWindow.GetWindow<BakeMeshWindow>();
                    bakeWindow.Init(generator);
                }
        }

        protected override void OnDelete()
        {
            base.OnDelete();
            var generator = (MeshGenerator) target;
            if (generator == null) return;
            var filter                               = generator.GetComponent<MeshFilter>();
            if (filter != null) filter.hideFlags     = HideFlags.None;
            var renderer                             = generator.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.hideFlags = HideFlags.None;
        }

        protected virtual void UVControls(MeshGenerator generator)
        {
            serializedObject.Update();
            var uvMode     = serializedObject.FindProperty("_uvMode");
            var uvOffset   = serializedObject.FindProperty("_uvOffset");
            var uvRotation = serializedObject.FindProperty("_uvRotation");
            var uvScale    = serializedObject.FindProperty("_uvScale");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Uv Coordinates", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(uvMode,     new GUIContent("UV Mode"));
            EditorGUILayout.PropertyField(uvOffset,   new GUIContent("UV Offset"));
            EditorGUILayout.PropertyField(uvRotation, new GUIContent("UV Rotation"));
            EditorGUILayout.PropertyField(uvScale,    new GUIContent("UV Scale"));
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}