using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(Node), true)]
    [CanEditMultipleObjects]
    public class NodeEditor : UnityEditor.Editor
    {
        private SplineComputer   addComp;
        private int              addPoint;
        private int[]            availablePoints;
        private bool             connectionsOpen, settingsOpen;
        private Node             lastnode;
        private Node[]           nodes = new Node[0];
        private Vector3          position, scale;
        private Quaternion       rotation;
        private SerializedObject serializedNodes;

        private SerializedProperty transformNormals, transformSize, transformTangents, type;

        private void OnEnable()
        {
            lastnode = (Node) target;
            lastnode.EditorMaintainConnections();
            connectionsOpen = EditorPrefs.GetBool("Dreamteck.Splines.Editor.NodeEditor.connectionsOpen");
            settingsOpen    = EditorPrefs.GetBool("Dreamteck.Splines.Editor.NodeEditor.settingsOpen");
            nodes           = new Node[targets.Length];
            for (var i = 0; i < targets.Length; i++) nodes[i] = (Node) targets[i];
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool("Dreamteck.Splines.Editor.NodeEditor.connectionsOpen", connectionsOpen);
            EditorPrefs.SetBool("Dreamteck.Splines.Editor.NodeEditor.settingsOpen",    settingsOpen);
        }

        private void OnDestroy()
        {
            if (Application.isEditor && !Application.isPlaying)
                if ((Node) target == null)
                {
                    var connections = lastnode.GetConnections();
                    for (var i = 0; i < connections.Length; i++)
                    {
                        if (connections[i].spline == null) continue;
                        Undo.RecordObject(connections[i].spline, "Delete node connections");
                    }

                    lastnode.ClearConnections();
                }
        }

        protected virtual void OnSceneGUI()
        {
            var node        = (Node) target;
            var connections = node.GetConnections();
            for (var i = 0; i < connections.Length; i++)
                DSSplineDrawer.DrawSplineComputer(connections[i].spline, 0.0, 1.0, 0.5f);

            var update = false;
            if (position != node.transform.position)
            {
                position = node.transform.position;
                update   = true;
            }

            if (scale != node.transform.localScale)
            {
                scale  = node.transform.localScale;
                update = true;
            }

            if (rotation != node.transform.rotation)
            {
                rotation = node.transform.rotation;
                update   = true;
            }

            if (update) node.UpdateConnectedComputers();

            if (addComp == null)
            {
                if (connections.Length > 0)
                {
                    var bezier = false;
                    for (var i = 0; i < connections.Length; i++)
                    {
                        if (connections[i].spline      == null) continue;
                        if (connections[i].spline.type == Spline.Type.Bezier) bezier = true;
                    }

                    if (bezier && node.type == Node.Type.Smooth)
                        if (connections[0].spline != null)
                        {
                            var point = node.GetPoint(0, true);
                            Handles.DrawDottedLine(node.transform.position, point.tangent,  6f);
                            Handles.DrawDottedLine(node.transform.position, point.tangent2, 6f);
                            var lastPos  = point.tangent;
                            var setPoint = false;
                            point.SetTangentPosition(Handles.PositionHandle(point.tangent, node.transform.rotation));
                            if (lastPos != point.tangent) setPoint = true;
                            lastPos = point.tangent2;
                            point.SetTangent2Position(Handles.PositionHandle(point.tangent2, node.transform.rotation));
                            if (lastPos != point.tangent2) setPoint = true;

                            if (setPoint)
                            {
                                node.SetPoint(0, point, true);
                                node.UpdateConnectedComputers();
                            }
                        }
                }

                return;
            }

            var points       = addComp.GetPoints();
            var camTransform = SceneView.currentDrawingSceneView.camera.transform;
            DSSplineDrawer.DrawSplineComputer(addComp, 0.0, 1.0, 0.5f);
            var originalAlignment = GUI.skin.label.alignment;
            var originalColor     = GUI.skin.label.normal.textColor;

            GUI.skin.label.alignment        = TextAnchor.MiddleCenter;
            GUI.skin.label.normal.textColor = addComp.editorPathColor;
            for (var i = 0; i < availablePoints.Length; i++)
            {
                if (addComp.isClosed && i == points.Length - 1) break;

                Handles.Label(points[i].position + Camera.current.transform.up * HandleUtility.GetHandleSize(points[i].position) * 0.3f,
                              (i + 1).ToString());
                if (SplineEditorHandles.CircleButton(points[availablePoints[i]].position,
                                                     Quaternion.LookRotation(-camTransform.forward, camTransform.up),
                                                     HandleUtility.GetHandleSize(points[availablePoints[i]].position) *
                                                     0.1f, 2f, addComp.editorPathColor))
                {
                    AddConnection(addComp, availablePoints[i]);
                    break;
                }
            }

            GUI.skin.label.alignment        = originalAlignment;
            GUI.skin.label.normal.textColor = originalColor;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var node = (Node) target;
            if (nodes.Length == 1)
            {
                if (addComp != null)
                {
                    var pointNames                                            = new string[availablePoints.Length];
                    for (var i = 0; i < pointNames.Length; i++) pointNames[i] = "Point " + (availablePoints[i] + 1);
                    if (availablePoints.Length > 0)
                        addPoint = EditorGUILayout.Popup("Link point", addPoint, pointNames);
                    else EditorGUILayout.LabelField("No Points Available");

                    if (GUILayout.Button("Cancel"))
                    {
                        addComp  = null;
                        addPoint = 0;
                    }

                    if (addPoint >= 0 && availablePoints.Length > addPoint)
                    {
                        if (node.HasConnection(addComp, availablePoints[addPoint]))
                            EditorGUILayout
                               .HelpBox("Connection already exists (" + addComp.name + "," + availablePoints[addPoint],
                                        MessageType.Error);
                        else if (GUILayout.Button("Link")) AddConnection(addComp, availablePoints[addPoint]);
                    }
                }
                else
                {
                    SplineEditorGUI.BeginContainerBox(ref connectionsOpen, "Connections");
                    if (connectionsOpen) ConnectionsGUI();
                    SplineEditorGUI.EndContainerBox();

                    var              rect = GUILayoutUtility.GetLastRect();
                    SplineComputer[] addComps;
                    var              lastComp = addComp;
                    var              dragged  = DreamteckEditorGUI.DropArea(rect, out addComps);
                    if (dragged && addComps.Length > 0) SelectComputer(addComps[0]);

                    if (lastComp != addComp) SceneView.RepaintAll();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Connection UI not available when multiple Nodes are selected.",
                                        MessageType.Info);
            }

            SplineEditorGUI.BeginContainerBox(ref settingsOpen, "Settings");
            if (settingsOpen) SettingsGUI();
            SplineEditorGUI.EndContainerBox();
        }

        private void SettingsGUI()
        {
            var node = (Node) target;
            serializedNodes   = new SerializedObject(nodes);
            transformNormals  = serializedNodes.FindProperty("_transformNormals");
            transformSize     = serializedNodes.FindProperty("_transformSize");
            transformTangents = serializedNodes.FindProperty("_transformTangents");
            type              = serializedNodes.FindProperty("type");


            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(transformNormals,  new GUIContent("Transform Normals"));
            EditorGUILayout.PropertyField(transformSize,     new GUIContent("Transform Size"));
            EditorGUILayout.PropertyField(transformTangents, new GUIContent("Transform Tangents"));
            EditorGUILayout.PropertyField(type,              new GUIContent("Node Type"));

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
                serializedNodes.ApplyModifiedProperties();
                node.UpdatePoints();
                node.UpdateConnectedComputers();
            }
        }

        private void ConnectionsGUI()
        {
            var node        = (Node) target;
            var connections = node.GetConnections();
            EditorGUILayout.Space();

            if (connections.Length > 0)
                for (var i = 0; i < connections.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(connections[i].spline.name + " at point " +
                                               (connections[i].pointIndex + 1));
                    if (GUILayout.Button("Select", GUILayout.Width(70)))
                        Selection.activeGameObject = connections[i].spline.gameObject;
                    SplineEditorGUI.SetHighlightColors(SplinePrefs.highlightColor, SplinePrefs.highlightContentColor);
                    if (SplineEditorGUI.EditorLayoutSelectableButton(new GUIContent("Swap Tangents"),
                                                                     connections[i].spline.type == Spline.Type.Bezier,
                                                                     connections[i].invertTangents))
                    {
                        connections[i].invertTangents = !connections[i].invertTangents;
                        node.UpdateConnectedComputers();
                        SceneView.RepaintAll();
                    }

                    if (GUILayout.Button("x", GUILayout.Width(20)))
                    {
                        Undo.RecordObject(node,                  "Remove connection");
                        Undo.RecordObject(connections[i].spline, "Remove node");
                        connections[i].spline.DisconnectNode(connections[i].pointIndex);
                        node.RemoveConnection(connections[i].spline, connections[i].pointIndex);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            else EditorGUILayout.HelpBox("Drag & Drop SplineComputers here to link their points.", MessageType.Info);
        }

        private void SelectComputer(SplineComputer comp)
        {
            addComp = comp;
            if (addComp != null) availablePoints = GetAvailablePoints(addComp);
            SceneView.RepaintAll();
            Repaint();
        }

        private void AddConnection(SplineComputer computer, int pointIndex)
        {
            var node        = (Node) target;
            var connections = node.GetConnections();
            if (EditorUtility.DisplayDialog("Link point?", "Add point " + (pointIndex + 1) + " to connections?", "Yes",
                                            "No"))
            {
                Undo.RecordObject(addComp, "Add connection");
                Undo.RecordObject(node,    "Add Connection");
                if (connections.Length == 0)
                    switch (EditorUtility.DisplayDialogComplex("Align node to point?",
                                                               "This is the first connection for the node, would you like to snap or align the node's Transform the spline point.",
                                                               "No", "Snap", "Snap and Align"))
                    {
                        case 1:
                            var point = addComp.GetPoint(pointIndex);
                            node.transform.position = point.position;
                            break;
                        case 2:
                            var result = addComp.Evaluate(pointIndex);
                            node.transform.position = result.position;
                            node.transform.rotation = result.rotation;
                            break;
                    }

                computer.ConnectNode(node, pointIndex);
                addComp  = null;
                addPoint = 0;
                SceneView.RepaintAll();
                Repaint();
            }
        }

        private int[] GetAvailablePoints(SplineComputer computer)
        {
            var indices = new List<int>();
            for (var i = 0; i < computer.pointCount; i++)
            {
                if (computer.GetNode(i) != null) continue;
                indices.Add(i);
            }

            return indices.ToArray();
        }
    }
}