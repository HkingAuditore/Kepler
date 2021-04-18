using System.Collections.Generic;
using System.IO;
using Dreamteck.Splines.IO;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class ImportExportTool : SplineTool
    {
        private bool alwaysDraw = true;
        private Axis exportAxis = Axis.Z;

        private List<CSV.ColumnType> exportColumns = new List<CSV.ColumnType>();
        private List<SplineComputer> exported      = new List<SplineComputer>();
        private string               exportPath    = "";
        private bool                 flatCSV;
        private ExportFormat         format        = ExportFormat.SVG;
        private Axis                 importAxis    = Axis.Z;
        private List<CSV.ColumnType> importColumns = new List<CSV.ColumnType>();
        private List<SplineComputer> imported      = new List<SplineComputer>();
        private GameObject           importedParent;

        private bool   importOptions;
        private string importPath = "";

        private          Mode                mode           = Mode.None;
        private readonly List<SplinePoint[]> originalPoints = new List<SplinePoint[]>();
        private          float               scaleFactor    = 1f;

        public override string GetName()
        {
            return "Import/Export";
        }

        protected override string GetPrefix()
        {
            return "ImportExport";
        }

        public override void Open(EditorWindow window)
        {
            base.Open(window);
            importPath = LoadString("importPath", "");
            exportPath = LoadString("exportPath", "");
            alwaysDraw = LoadBool("alwaysDraw", true);
            flatCSV    = LoadBool("flatCSV",    false);
            importAxis = (Axis) LoadInt("importAxis", 2);
            exportAxis = (Axis) LoadInt("exportAxis", 2);
            LoadColumns("importColumns", ref importColumns);
            LoadColumns("exportColumns", ref exportColumns);
        }

        private void LoadColumns(string name, ref List<CSV.ColumnType> destination)
        {
            var text = LoadString(name, "");
            destination = new List<CSV.ColumnType>();
            if (text == "")
            {
                destination.Add(CSV.ColumnType.Position);
                destination.Add(CSV.ColumnType.Tangent);
                destination.Add(CSV.ColumnType.Tangent2);
                destination.Add(CSV.ColumnType.Normal);
                destination.Add(CSV.ColumnType.Size);
                destination.Add(CSV.ColumnType.Color);
                return;
            }

            var elements = text.Split(',');
            foreach (var element in elements)
            {
                var i = 0;
                if (int.TryParse(element, out i)) destination.Add((CSV.ColumnType) i);
            }
        }

        public override void Close()
        {
            base.Close();
            if (importPath != "") SaveString("importPath", Path.GetDirectoryName(importPath));
            if (exportPath != "") SaveString("exportPath", Path.GetDirectoryName(exportPath));
            var columnString = "";
            foreach (var col in importColumns)
            {
                if (columnString != "") columnString += ",";
                columnString += ((int) col).ToString();
            }

            SaveString("importColumns", columnString);
            columnString = "";
            foreach (var col in exportColumns)
            {
                if (columnString != "") columnString += ",";
                columnString += ((int) col).ToString();
            }

            SaveString("exportColumns", columnString);
            SaveBool("alwaysDraw", alwaysDraw);
            SaveBool("flatCSV",    flatCSV);
            SaveInt("importAxis", (int) importAxis);
            SaveInt("exportAxis", (int) exportAxis);

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif
        }

        protected override void Save()
        {
            base.Save();
            if (importedParent != null)
            {
                Selection.activeGameObject = importedParent;
                importedParent             = null;
            }
            else
            {
                foreach (var comp in imported)
                    if (comp != null)
                    {
                        Selection.activeGameObject = comp.gameObject;
                        break;
                    }
            }

            imported.Clear();
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif

            mode = Mode.None;
        }

        protected override void Cancel()
        {
            base.Cancel();
            foreach (var spline in imported) Object.DestroyImmediate(spline.gameObject);
            Object.DestroyImmediate(importedParent);
            imported.Clear();
            SceneView.RepaintAll();
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif

            mode = Mode.None;
        }

        private void CSVColumnUI(List<CSV.ColumnType> columns)
        {
            EditorGUILayout.LabelField("Dataset Columns");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.MaxWidth(30)) && columns.Count > 0) columns.RemoveAt(columns.Count - 1);
            for (var i = 0; i < columns.Count; i++) columns[i] = (CSV.ColumnType) EditorGUILayout.EnumPopup(columns[i]);
            if (GUILayout.Button("+", GUILayout.MaxWidth(30)) && columns.Count > 0)
                columns.Add(CSV.ColumnType.Position);
            EditorGUILayout.EndHorizontal();
        }

        private void OnScene(SceneView current)
        {
            for (var i = 0; i < imported.Count; i++) DSSplineDrawer.DrawSplineComputer(imported[i]);
        }

        private void ImportUI()
        {
            EditorGUI.BeginChangeCheck();
            scaleFactor = EditorGUILayout.FloatField("Scale Factor", scaleFactor);
            importAxis  = (Axis) EditorGUILayout.EnumPopup("Facing Axis", importAxis);
            alwaysDraw  = EditorGUILayout.Toggle("Always Draw", alwaysDraw);
            if (EditorGUI.EndChangeCheck()) ApplyPoints();
            SaveCancelUI();
        }

        private void ExportUI()
        {
            if (exported.Count == 0)
            {
                mode = Mode.None;
                return;
            }

            EditorGUILayout.Space();
            format = (ExportFormat) EditorGUILayout.EnumPopup("Format", format);
            if (format == ExportFormat.SVG)
            {
                exportAxis = (Axis) EditorGUILayout.EnumPopup("Projection Axis", exportAxis);
                EditorGUILayout
                   .HelpBox("The SVG is a 2D vector format so the exported spline will be flattened along the selected axis",
                            MessageType.Info);
            }
            else
            {
                CSVColumnUI(exportColumns);
                flatCSV = EditorGUILayout.Toggle("Flat", flatCSV);
                if (flatCSV) exportAxis = (Axis) EditorGUILayout.EnumPopup("Projection Axis", exportAxis);
                EditorGUILayout.HelpBox("The exported splined will be flattened along the selected axis.",
                                        MessageType.Info);
            }

            if (GUILayout.Button("Save File"))
            {
                var extension = "*";
                switch (format)
                {
                    case ExportFormat.SVG:
                        extension = "svg";
                        break;
                    case ExportFormat.CSV:
                        extension = "csv";
                        break;
                }

                exportPath = EditorUtility.SaveFilePanel("Export splines", exportPath, "spline", extension);
                if (exportPath != "")
                    if (Directory.Exists(Path.GetDirectoryName(exportPath)))
                        switch (format)
                        {
                            case ExportFormat.SVG:
                                ExportSVG(exportPath);
                                break;
                            case ExportFormat.CSV:
                                ExportCSV(exportPath);
                                break;
                        }
            }
        }

        public override void Draw(Rect windowRect)
        {
            if (mode == Mode.Import)
            {
                ImportUI();
            }
            else
            {
                importOptions = EditorGUILayout.Foldout(importOptions, "Import Options");
                if (importOptions) CSVColumnUI(importColumns);
                if (GUILayout.Button("Import"))
                {
                    importPath = EditorUtility.OpenFilePanel("Browse File", importPath, "svg,csv");
                    if (File.Exists(importPath))
                    {
                        splines.Clear();
                        var ext = Path.GetExtension(importPath).ToLower();
                        switch (ext)
                        {
                            case ".svg":
                                ImportSVG(importPath);
                                break;
                            case ".csv":
                                ImportCSV(importPath);
                                break;
                            case ".xml":
                                ImportSVG(importPath);
                                break;
                        }
                    }
                }

                exported = GetSelectedSplines();
                if (exported.Count == 0) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                if (mode           == Mode.Export) ExportUI();
                if (mode != Mode.Export)
                    if (GUILayout.Button("Export") && exported.Count > 0)
                        mode = Mode.Export;
            }
        }

        private List<SplineComputer> GetSelectedSplines()
        {
            var selected = new List<SplineComputer>();
            foreach (var obj in Selection.gameObjects)
            {
                var comp = obj.GetComponent<SplineComputer>();
                if (comp != null) selected.Add(comp);
            }

            return selected;
        }

        private void ApplyPoints()
        {
            if (originalPoints.Count != imported.Count) return;
            if (imported.Count       == 0) return;
            var lookRot = Quaternion.identity;
            switch (importAxis)
            {
                case Axis.X:
                    lookRot = Quaternion.LookRotation(Vector3.right);
                    break;
                case Axis.Y:
                    lookRot = Quaternion.LookRotation(Vector3.down);
                    break;
                case Axis.Z:
                    lookRot = Quaternion.LookRotation(Vector3.forward);
                    break;
            }

            for (var i = 0; i < imported.Count; i++)
            {
                var transformed = new SplinePoint[originalPoints[i].Length];
                originalPoints[i].CopyTo(transformed, 0);
                for (var j = 0; j < transformed.Length; j++)
                {
                    transformed[j].position *= scaleFactor;
                    transformed[j].tangent  *= scaleFactor;
                    transformed[j].tangent2 *= scaleFactor;

                    transformed[j].position = lookRot * transformed[j].position;
                    transformed[j].tangent  = lookRot * transformed[j].tangent;
                    transformed[j].tangent2 = lookRot * transformed[j].tangent2;
                    transformed[j].normal   = lookRot * transformed[j].normal;
                }

                imported[i].SetPoints(transformed);
                if (alwaysDraw)
                    DSSplineDrawer.RegisterComputer(imported[i]);
                else
                    DSSplineDrawer.UnregisterComputer(imported[i]);
            }

            SceneView.RepaintAll();
        }

        private void GetImportedPoints()
        {
            foreach (var comp in imported)
                if (comp != null)
                {
                    originalPoints.Add(comp.GetPoints(SplineComputer.Space.Local));
                    mode = Mode.Import;
                }
                else
                {
                    imported.Remove(comp);
                }
        }

        private void ImportSVG(string file)
        {
            var svg = new SVG(file);
            originalPoints.Clear();
            imported = svg.CreateSplineComputers(Vector3.zero, Quaternion.identity);
            if (imported.Count == 0) return;
            importedParent = new GameObject(svg.name);
            foreach (var comp in imported) comp.transform.parent = importedParent.transform;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnScene;
#else
            SceneView.onSceneGUIDelegate += OnScene;
#endif

            GetImportedPoints();
            ApplyPoints();
            promptSave = true;
        }

        private void ExportSVG(string file)
        {
            var svg = new SVG(exported);
            svg.Write(file, (SVG.Axis) (int) exportAxis);
        }

        private void ExportCSV(string file)
        {
            var csv = new CSV(exported[0]);
            csv.columns = exportColumns;
            if (flatCSV)
                switch (exportAxis)
                {
                    case Axis.X:
                        csv.FlatX();
                        break;
                    case Axis.Y:
                        csv.FlatY();
                        break;
                    case Axis.Z:
                        csv.FlatZ();
                        break;
                }

            csv.Write(file);
        }


        private void ImportCSV(string file)
        {
            var csv = new CSV(file, importColumns);
            originalPoints.Clear();
            imported.Clear();
            imported.Add(csv.CreateSplineComputer(Vector3.zero, Quaternion.identity));
            if (imported.Count == 0) return;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnScene;
#else
            SceneView.onSceneGUIDelegate += OnScene;
#endif

            GetImportedPoints();
            ApplyPoints();
            promptSave = true;
        }

        private enum Mode
        {
            None,
            Import,
            Export
        }

        private enum ExportFormat
        {
            SVG,
            CSV
        }

        private enum Axis
        {
            X,
            Y,
            Z
        }
    }
}