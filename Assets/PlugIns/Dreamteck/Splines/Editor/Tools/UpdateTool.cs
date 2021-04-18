using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dreamteck.Splines
{
    public class UpdateTool : SplineTool
    {
        protected GameObject       obj;
        protected ObjectController spawner;
        private   string           updated = "";

        public override string GetName()
        {
            return "Update Components";
        }

        protected override string GetPrefix()
        {
            return "UpdateTool";
        }

        public override void Draw(Rect windowRect)
        {
            if (GUILayout.Button("Update All Spline Components"))
            {
                updated = "";
                UpdateComputers();
                UpdateNodes();
                UpdateUsers();
            }

            if (GUILayout.Button("Update SplineUsers"))
            {
                updated = "";
                UpdateUsers();
            }

            if (GUILayout.Button("Update MeshGenerators"))
            {
                updated = "";
                UpdateMeshGenerators();
            }

            if (GUILayout.Button("Update SplineComputers"))
            {
                updated = "";
                UpdateComputers();
            }

            if (GUILayout.Button("Update Nodes In Scene"))
            {
                updated = "";
                UpdateNodes();
            }

            EditorGUILayout.Space();
            GUILayout.Label(updated);
        }

        private void UpdateNodes()
        {
            var nodes = Object.FindObjectsOfType<Node>();
            EditorUtility.ClearProgressBar();
            for (var i = 0; i < nodes.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Updating nodes", "Updating node " + nodes[i].name,
                                                 (float) i / (nodes.Length - 1));
                nodes[i].UpdateConnectedComputers();
                updated += i + " - " + nodes[i].name + Environment.NewLine;
            }

            EditorUtility.ClearProgressBar();
            if (nodes.Length == 0) updated += Environment.NewLine + "No active Nodes found in the scene.";
        }

        private void UpdateUsers()
        {
            var users = Object.FindObjectsOfType<SplineUser>();
            EditorUtility.ClearProgressBar();
            for (var i = 0; i < users.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Updating users", "Updating user " + users[i].name,
                                                 (float) i / (users.Length - 1));
                users[i].EditorAwake();
                users[i].Rebuild();
                updated += i + " - " + users[i].name + Environment.NewLine;
            }

            EditorUtility.ClearProgressBar();
            if (users.Length == 0) updated += Environment.NewLine + "No active SplineUsers found in the scene.";
        }

        private void UpdateMeshGenerators()
        {
            var users = Object.FindObjectsOfType<MeshGenerator>();
            EditorUtility.ClearProgressBar();
            for (var i = 0; i < users.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Updating mesh generators", "Updating generator " + users[i].name,
                                                 (float) i / (users.Length - 1));
                users[i].EditorAwake();
                users[i].Rebuild();
                updated += i + " - " + users[i].name + Environment.NewLine;
            }

            EditorUtility.ClearProgressBar();
            if (users.Length == 0) updated += Environment.NewLine + "No active MeshGenerators found in the scene.";
        }

        private void UpdateComputers()
        {
            var computers = Object.FindObjectsOfType<SplineComputer>();
            EditorUtility.ClearProgressBar();
            for (var i = 0; i < computers.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Updating spline computers", "Updating computer " + computers[i].name,
                                                 (float) i / (computers.Length - 1));
                computers[i].RebuildImmediate();
                updated += i + " - " + computers[i].name + Environment.NewLine;
            }

            EditorUtility.ClearProgressBar();
            if (computers.Length == 0) updated += Environment.NewLine + "No active SplineComputers found in the scene.";
        }
    }
}