using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class BakeTool : SplineTool
    {
        public enum BakeGroup
        {
            All,
            Selected,
            AllExcluding
        }

        private BakeGroup bakeGroup = BakeGroup.All;
        private bool      copy;

        private DirectoryInfo             dirInfo;
        private List<MeshGenerator>       excluded = new List<MeshGenerator>();
        private BakeMeshWindow.SaveFormat format   = BakeMeshWindow.SaveFormat.MeshAsset;
        private MeshGenerator[]           found    = new MeshGenerator[0];

        private bool isStatic = true;
        private bool permanent;
        private bool removeComputer;

        private string savePath = "";

        private Vector2             scroll1, scroll2;
        private List<MeshGenerator> selected = new List<MeshGenerator>();

        public override string GetName()
        {
            return "Bake Meshes";
        }

        public override void Draw(Rect windowRect)
        {
            bakeGroup = (BakeGroup) EditorGUILayout.EnumPopup("Bake Mode", bakeGroup);
            if (bakeGroup == BakeGroup.Selected)
                MeshGenSelector(ref selected,                                           "Selected");
            else if (bakeGroup == BakeGroup.AllExcluding) MeshGenSelector(ref excluded, "Excluded");


            format = (BakeMeshWindow.SaveFormat) EditorGUILayout.EnumPopup("Save Format", format);
            var saveMesh = format != BakeMeshWindow.SaveFormat.None;

            if (format != BakeMeshWindow.SaveFormat.None) copy = EditorGUILayout.Toggle("Save without baking", copy);
            var isCopy                                         = format != BakeMeshWindow.SaveFormat.None && copy;
            switch (format)
            {
                case BakeMeshWindow.SaveFormat.None:
                    EditorGUILayout.HelpBox("Saves the mesh inside the scene for lightmap", MessageType.Info);
                    break;
                case BakeMeshWindow.SaveFormat.MeshAsset:
                    EditorGUILayout
                       .HelpBox("Saves the mesh as an .asset file inside the project. This makes using the mesh in prefabs and across scenes possible.",
                                MessageType.Info);
                    break;
                case BakeMeshWindow.SaveFormat.OBJ:
                    EditorGUILayout
                       .HelpBox("Exports the mesh as an OBJ file which can be imported in a third-party modeling application.",
                                MessageType.Info);
                    break;
            }

            EditorGUILayout.Space();

            if (!isCopy)
            {
                isStatic  = EditorGUILayout.Toggle("Make Static", isStatic);
                permanent = EditorGUILayout.Toggle("Permanent",   permanent);
                if (permanent)
                {
                    removeComputer = EditorGUILayout.Toggle("Remove SplineComputer", removeComputer);
                    if (removeComputer)
                        EditorGUILayout
                           .HelpBox("WARNING: Removing Spline Computers may cause other SplineUsers to stop working. Select this if you are sure that no other SplineUser uses the selected Spline Computers.",
                                    MessageType.Warning);
                }
            }

            if (GUILayout.Button("Bake"))
            {
                if (saveMesh)
                {
                    savePath = EditorUtility.OpenFolderPanel("Save Directory", Application.dataPath, "folder");
                    if (!Directory.Exists(savePath) || savePath == "")
                    {
                        EditorUtility.DisplayDialog("Save error",
                                                    "Invalid save directory. Please select a valid save directory and try again",
                                                    "OK");
                        return;
                    }

                    if (format == BakeMeshWindow.SaveFormat.OBJ && !savePath.StartsWith(Application.dataPath) && !copy)
                    {
                        EditorUtility.DisplayDialog("Save error",
                                                    "OBJ files can be saved outside of the project folder only when \"Save without baking\" is selected. Please select a directory inside the project in order to save.",
                                                    "OK");
                        return;
                    }

                    if (format == BakeMeshWindow.SaveFormat.MeshAsset && !savePath.StartsWith(Application.dataPath))
                    {
                        EditorUtility.DisplayDialog("Save error",
                                                    "Asset files cannot be saved outside of the project directory. Please select a path inside the project directory.",
                                                    "OK");
                        return;
                    }
                }

                var suff                                      = "all";
                if (bakeGroup == BakeGroup.Selected) suff     = "selected";
                if (bakeGroup == BakeGroup.AllExcluding) suff = "all excluding";
                if (EditorUtility.DisplayDialog("Bake " + suff,
                                                "This operation cannot be undone. Are you sure you want to bake the meshes?",
                                                "Yes", "No"))
                    switch (bakeGroup)
                    {
                        case BakeGroup.All:
                            BakeAll();
                            break;
                        case BakeGroup.Selected:
                            BakeSelected();
                            break;
                        case BakeGroup.AllExcluding:
                            BakeExcluding();
                            break;
                    }
            }
        }

        private void BakeAll()
        {
            EditorUtility.ClearProgressBar();
            for (var i = 0; i < found.Length; i++)
            {
                var percent = (float) i / (found.Length - 1);
                EditorUtility.DisplayProgressBar("Baking progress", "Baking generator " + i, percent);
                Bake(found[i]);
            }

            EditorUtility.ClearProgressBar();
        }

        private void BakeSelected()
        {
            EditorUtility.ClearProgressBar();
            for (var i = 0; i < selected.Count; i++)
            {
                var percent = (float) i / (selected.Count - 1);
                EditorUtility.DisplayProgressBar("Baking progress", "Baking generator " + i, percent);
                Bake(selected[i]);
            }

            EditorUtility.ClearProgressBar();
        }

        private void BakeExcluding()
        {
            EditorUtility.ClearProgressBar();
            for (var i = 0; i < found.Length; i++)
            {
                var percent = (float) i / (found.Length - 1);
                EditorUtility.DisplayProgressBar("Baking progress", "Baking generator " + i, percent);
                Bake(found[i]);
            }

            EditorUtility.ClearProgressBar();
        }

        private void Bake(MeshGenerator gen)
        {
            var filter = gen.GetComponent<MeshFilter>();
            if (filter == null)
            {
                EditorUtility.DisplayDialog("Save error", "No mesh present in " + gen.name, "OK");
                return;
            }

            if (copy)
            {
                UnityEditor.MeshUtility.Optimize(filter.sharedMesh);
                Unwrapping.GenerateSecondaryUVSet(filter.sharedMesh);
            }
            else
            {
                gen.Bake(isStatic, true);
            }

            if (format == BakeMeshWindow.SaveFormat.OBJ)
            {
                var renderer = gen.GetComponent<MeshRenderer>();
                dirInfo = new DirectoryInfo(savePath);
                var files                      = dirInfo.GetFiles(filter.sharedMesh.name + "*.obj");
                var meshName                   = filter.sharedMesh.name;
                if (files.Length > 0) meshName += "_" + files.Length;
                var path                       = savePath + "/" + meshName + ".obj";
                var objString                  = MeshUtility.ToOBJString(filter.sharedMesh, renderer.sharedMaterials);
                File.WriteAllText(path, objString);
                if (copy)
                {
                    var relativepath = "Assets" + path.Substring(Application.dataPath.Length);
                    AssetDatabase.ImportAsset(relativepath, ImportAssetOptions.ForceSynchronousImport);
                    filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(relativepath);
                }
            }

            if (format == BakeMeshWindow.SaveFormat.MeshAsset)
            {
                dirInfo = new DirectoryInfo(savePath);
                var files                      = dirInfo.GetFiles(filter.sharedMesh.name + "*.asset");
                var meshName                   = filter.sharedMesh.name;
                if (files.Length > 0) meshName += "_" + files.Length;
                var path                       = savePath + "/" + meshName + ".asset";
                var relativepath               = "Assets" + path.Substring(Application.dataPath.Length);
                if (copy)
                {
                    var assetMesh = MeshUtility.Copy(filter.sharedMesh);
                    AssetDatabase.CreateAsset(assetMesh, relativepath);
                }
                else
                {
                    AssetDatabase.CreateAsset(filter.sharedMesh, relativepath);
                }
            }

            if (permanent && !copy)
            {
                var meshGenComputer = gen.spline;
                if (permanent)
                {
                    meshGenComputer.Unsubscribe(gen);
                    Object.DestroyImmediate(gen);
                }

                if (removeComputer)
                {
                    if (meshGenComputer.GetComponents<Component>().Length == 2)
                        Object.DestroyImmediate(meshGenComputer.gameObject);
                    else Object.DestroyImmediate(meshGenComputer);
                }
            }
        }

        private void Refresh()
        {
            found = Object.FindObjectsOfType<MeshGenerator>();
        }

        private void OnFocus()
        {
            Refresh();
        }

        public override void Open(EditorWindow window)
        {
            base.Open(window);
            isStatic       = LoadBool("isStatic", true);
            format         = (BakeMeshWindow.SaveFormat) LoadInt("format", 0);
            removeComputer = LoadBool("removeComputer", false);
            copy           = LoadBool("copy",           false);
            Refresh();
        }

        public override void Close()
        {
            base.Close();
            SaveBool("isStatic", isStatic);
            SaveInt("format", (int) format);
            SaveBool("copy",           copy);
            SaveBool("removeComputer", removeComputer);
        }

        protected override string GetPrefix()
        {
            return "BakeTool";
        }

        private void MeshGenSelector(ref List<MeshGenerator> list, string title)
        {
            var availalbe = new List<MeshGenerator>(found);
            for (var i = availalbe.Count - 1; i >= 0; i--)
            {
                for (var n = 0; n < list.Count; n++)
                    if (list[n] == availalbe[i])
                    {
                        availalbe.RemoveAt(i);
                        break;
                    }
            }

            GUILayout.Box("Available", GUILayout.Width(Screen.width - 15 - Screen.width / 3f), GUILayout.Height(100));
            var rect = GUILayoutUtility.GetLastRect();
            rect.y      += 15;
            rect.height -= 15;
            scroll1     =  GUI.BeginScrollView(rect, scroll1, new Rect(0, 0, rect.width, 22 * availalbe.Count));
            for (var i = 0; i < availalbe.Count; i++)
            {
                GUI.Label(new Rect(5, 22 * i, rect.width - 30, 22), availalbe[i].name);
                if (GUI.Button(new Rect(rect.width - 29, 22 * i, 22, 22), "+"))
                {
                    list.Add(availalbe[i]);
                    availalbe.RemoveAt(i);
                    break;
                }
            }

            GUI.EndScrollView();
            EditorGUILayout.Space();
            GUILayout.Box(title, GUILayout.Width(Screen.width - 15 - Screen.width / 3f), GUILayout.Height(100));

            rect        =  GUILayoutUtility.GetLastRect();
            rect.y      += 15;
            rect.height -= 15;
            scroll2     =  GUI.BeginScrollView(rect, scroll2, new Rect(0, 0, rect.width, 22 * list.Count));
            for (var i = list.Count - 1; i >= 0; i--)
            {
                GUI.Label(new Rect(5, 22 * i, rect.width - 30, 22), list[i].name);
                if (GUI.Button(new Rect(rect.width       - 29, 22 * i, 22, 22), "x")) list.RemoveAt(i);
            }

            GUI.EndScrollView();
        }
    }
}