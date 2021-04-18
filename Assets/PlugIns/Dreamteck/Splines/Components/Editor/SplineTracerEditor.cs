using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(SplineTracer), true)]
    public class SplineTracerEditor : SplineUserEditor
    {
        public delegate void DistanceReceiver(float distance);

        private Camera                cam;
        private bool                  cameraFoldout;
        private TransformModuleEditor motionEditor;
        private Texture2D             renderCanvas;
        private RenderTexture         rt;
        private SplineTracer[]        tracers = new SplineTracer[0];

        protected override void OnEnable()
        {
            base.OnEnable();
            var tracer = (SplineTracer) target;
            motionEditor = new TransformModuleEditor(tracer, this, tracer.motion);
            tracers      = new SplineTracer[targets.Length];
            for (var i = 0; i < tracers.Length; i++) tracers[i] = (SplineTracer) targets[i];
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyImmediate(rt);
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            var tracer = (SplineTracer) target;
        }

        private int GetRTWidth()
        {
            return Mathf.RoundToInt(EditorGUIUtility.currentViewWidth) - 50;
        }

        private int GetRTHeight()
        {
            return Mathf.RoundToInt(GetRTWidth() / cam.aspect);
        }

        private void CreateRT()
        {
            if (rt != null)
            {
                DestroyImmediate(rt);
                DestroyImmediate(renderCanvas);
            }

            rt = new RenderTexture(GetRTWidth(), GetRTHeight(), 16, RenderTextureFormat.Default,
                                   RenderTextureReadWrite.Default);
            renderCanvas = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        }

        protected override void BodyGUI()
        {
            base.BodyGUI();
            EditorGUILayout.LabelField("Tracing", EditorStyles.boldLabel);
            var tracer = (SplineTracer) target;
            serializedObject.Update();
            var useTriggers  = serializedObject.FindProperty("useTriggers");
            var triggerGroup = serializedObject.FindProperty("triggerGroup");
            var direction    = serializedObject.FindProperty("_direction");
            var physicsMode  = serializedObject.FindProperty("_physicsMode");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(useTriggers);
            if (useTriggers.boolValue) EditorGUILayout.PropertyField(triggerGroup);
            EditorGUILayout.PropertyField(direction, new GUIContent("Direction"));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(physicsMode, new GUIContent("Physics Mode"));
            if (EditorGUI.EndChangeCheck())
                for (var i = 0; i < tracers.Length; i++)
                    tracers[i].EditorAwake();

            if (tracer.physicsMode == SplineTracer.PhysicsMode.Rigidbody)
            {
                var rb = tracer.GetComponent<Rigidbody>();
                if (rb == null) EditorGUILayout.HelpBox("Assign a Rigidbody component.", MessageType.Error);
                else if (rb.interpolation    == RigidbodyInterpolation.None &&
                         tracer.updateMethod != SplineUser.UpdateMethod.FixedUpdate)
                    EditorGUILayout
                       .HelpBox("Switch to FixedUpdate mode to ensure smooth update for non-interpolated rigidbodies",
                                MessageType.Warning);
            }
            else if (tracer.physicsMode == SplineTracer.PhysicsMode.Rigidbody2D)
            {
                var rb = tracer.GetComponent<Rigidbody2D>();
                if (rb == null) EditorGUILayout.HelpBox("Assign a Rigidbody2D component.", MessageType.Error);
                else if (rb.interpolation    == RigidbodyInterpolation2D.None &&
                         tracer.updateMethod != SplineUser.UpdateMethod.FixedUpdate)
                    EditorGUILayout
                       .HelpBox("Switch to FixedUpdate mode to ensure smooth update for non-interpolated rigidbodies",
                                MessageType.Warning);
            }

            if (tracers.Length == 1)
            {
                motionEditor.DrawInspector();
                cameraFoldout = EditorGUILayout.Foldout(cameraFoldout, "Camera preview");
                if (cameraFoldout)
                {
                    if (cam == null) cam = tracer.GetComponentInChildren<Camera>();
                    if (cam != null)
                    {
                        if (rt == null || rt.width != GetRTWidth() || rt.height != GetRTHeight()) CreateRT();
                        GUILayout.Box("", GUILayout.Width(rt.width), GUILayout.Height(rt.height));
                        var prevTarget = cam.targetTexture;
                        var prevActive = RenderTexture.active;
                        var lastFlags  = cam.clearFlags;
                        var lastColor  = cam.backgroundColor;
                        cam.targetTexture   = rt;
                        cam.clearFlags      = CameraClearFlags.Color;
                        cam.backgroundColor = Color.black;
                        cam.Render();
                        RenderTexture.active = rt;
                        renderCanvas.SetPixels(new Color[renderCanvas.width * renderCanvas.height]);
                        renderCanvas.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                        renderCanvas.Apply();
                        RenderTexture.active = prevActive;
                        cam.targetTexture    = prevTarget;
                        cam.clearFlags       = lastFlags;
                        cam.backgroundColor  = lastColor;
                        GUI.DrawTexture(GUILayoutUtility.GetLastRect(), renderCanvas, ScaleMode.StretchToFill);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("There is no camera attached to the selected object or its children.",
                                                MessageType.Info);
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                for (var i = 0; i < tracers.Length; i++) tracers[i].Rebuild();
            }
        }

        protected void DrawResult(SplineSample result)
        {
            var tracer = (SplineTracer) target;
            Handles.color = Color.white;
            Handles.DrawLine(tracer.transform.position, result.position);
            SplineEditorHandles.DrawSolidSphere(result.position, HandleUtility.GetHandleSize(result.position) * 0.2f);
            Handles.color = Color.blue;
            Handles.DrawLine(result.position,
                             result.position + result.forward * HandleUtility.GetHandleSize(result.position) * 0.5f);
            Handles.color = Color.green;
            Handles.DrawLine(result.position,
                             result.position + result.up * HandleUtility.GetHandleSize(result.position) * 0.5f);
            Handles.color = Color.red;
            Handles.DrawLine(result.position,
                             result.position + result.right * HandleUtility.GetHandleSize(result.position) * 0.5f);
            Handles.color = Color.white;
        }
    }
}