using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class StylizedSurfaceEditor : ShaderGUI {
    private Material _target;
    private MaterialEditor _editor;
    private MaterialProperty[] _properties;
    private int _celShadingNumSteps = 0;
    private AnimationCurve _gradient = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    private static readonly Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();
    private static readonly Color hashColor = new Color(0.85023f, 0.85034f, 0.85045f, 0.85056f);
    private static readonly GUIContent staticLabel = new GUIContent();
    private static readonly int ColorPropertyName = Shader.PropertyToID("_Color");

    void DrawStandard(MaterialProperty property) {
        string displayName = property.displayName;
        // Remove everything in square brackets.
        displayName = Regex.Replace(displayName, @" ?\[.*?\]", string.Empty);
        _editor.ShaderProperty(property, displayName);
    }

    MaterialProperty FindProperty(string name) {
        return FindProperty(name, _properties);
    }

    static GUIContent MakeLabel(string text, string tooltip = null) {
        staticLabel.text = text;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        _editor = materialEditor;
        _properties = properties;
        _target = materialEditor.target as Material;
        Debug.Assert(_target != null, "_target != null");

        int originalIntentLevel = EditorGUI.indentLevel;
        int foldoutRemainingItems = 0;
        bool latestFoldoutState = false;

        foreach (MaterialProperty property in properties) {
            bool skipProperty = false;
            string displayName = property.displayName;
            
            if (displayName.Contains("[_CELPRIMARYMODE_SINGLE]")) {
                skipProperty = !_target.IsKeywordEnabled("_CELPRIMARYMODE_SINGLE");
                EditorGUI.indentLevel += 1;
            }

            if (displayName.Contains("[_CELPRIMARYMODE_STEPS]")) {
                skipProperty = !_target.IsKeywordEnabled("_CELPRIMARYMODE_STEPS");
                EditorGUI.indentLevel += 1;
            }

            if (displayName.Contains("[_CELPRIMARYMODE_CURVE]")) {
                skipProperty = !_target.IsKeywordEnabled("_CELPRIMARYMODE_CURVE");
                EditorGUI.indentLevel += 1;
            }

            if (displayName.Contains("[DR_CEL_EXTRA_ON]") && !property.name.Equals("_CelExtraEnabled")) {
                skipProperty = !_target.IsKeywordEnabled("DR_CEL_EXTRA_ON");
                EditorGUI.indentLevel += 1;
            }

            if (displayName.Contains("[DR_SPECULAR_ON]") && !property.name.Equals("_SpecularEnabled")) {
                skipProperty = !_target.IsKeywordEnabled("DR_SPECULAR_ON");
                EditorGUI.indentLevel += 1;
            }

            if (displayName.Contains("[DR_RIM_ON]") && !property.name.Equals("_RimEnabled")) {
                skipProperty = !_target.IsKeywordEnabled("DR_RIM_ON");
                EditorGUI.indentLevel += 1;
            }

            if (displayName.Contains("[DR_GRADIENT_ON]") && !property.name.Equals("_GradientEnabled")) {
                skipProperty = !_target.IsKeywordEnabled("DR_GRADIENT_ON");
                EditorGUI.indentLevel += 1;
            }

            if (displayName.Contains("[_UNITYSHADOWMODE_MULTIPLY]")) {
                skipProperty = !_target.IsKeywordEnabled("_UNITYSHADOWMODE_MULTIPLY");
                EditorGUI.indentLevel += 1;
            }

            if (displayName.Contains("[_UNITYSHADOWMODE_COLOR]")) {
                skipProperty = !_target.IsKeywordEnabled("_UNITYSHADOWMODE_COLOR");
                EditorGUI.indentLevel += 1;
            }

            if (displayName.Contains("FOLDOUT")) {
                string foldoutName = displayName.Split('(', ')')[1];
                string foldoutItemCount = displayName.Split('{', '}')[1];
                foldoutRemainingItems = Convert.ToInt32(foldoutItemCount);
                if (!_foldoutStates.ContainsKey(property.name)) {
                    _foldoutStates.Add(property.name, false);
                }

                EditorGUILayout.Space();
                _foldoutStates[property.name] =
                    EditorGUILayout.Foldout(_foldoutStates[property.name], foldoutName);
                latestFoldoutState = _foldoutStates[property.name];
            }

            if (foldoutRemainingItems > 0) {
                skipProperty = skipProperty || !latestFoldoutState;
                EditorGUI.indentLevel += 1;
                --foldoutRemainingItems;
            }

            if (_target.IsKeywordEnabled("_CELPRIMARYMODE_STEPS") && displayName.Contains("[LAST_PROP_STEPS]")) {
                EditorGUILayout.HelpBox(
                    "This mode creates a step texture that control the light/shadow transition. To use:\n" +
                    "1. Set the number of steps (e.g. 3 means three steps between lit and shaded regions), \n" +
                    "2. Save the steps as a texture - 'Save Ramp Texture' button",
                    MessageType.Info);
                int currentNumSteps = _target.GetInt("_CelNumSteps");
                if (currentNumSteps != _celShadingNumSteps) {
                    if (GUILayout.Button("Save Ramp Texture")) {
                        _celShadingNumSteps = currentNumSteps;
                        PromptTextureSave(materialEditor, GenerateStepTexture, "_CelStepTexture", FilterMode.Point);
                    }
                }
            }

            if (_target.IsKeywordEnabled("_CELPRIMARYMODE_CURVE") && displayName.Contains("[LAST_PROP_CURVE]")) {
                EditorGUILayout.HelpBox(
                    "This mode uses arbitrary curves to control the light/shadow transition. How to use:\n" +
                    "1. Set shading curve (generally from 0.0 to 1.0)\n" +
                    "2. [Optional] Save the curve preset\n" +
                    "3. Save the curve as a texture.",
                    MessageType.Info);
                _gradient = EditorGUILayout.CurveField("Shading curve", _gradient);

                if (GUILayout.Button("Save Ramp Texture")) {
                    PromptTextureSave(materialEditor, GenerateCurveTexture, "_CelCurveTexture",
                        FilterMode.Trilinear);
                }
            }

            if (!skipProperty && property.type == MaterialProperty.PropType.Color && property.colorValue == hashColor) {
                property.colorValue = _target.GetColor(ColorPropertyName);
            }

            bool hideInInspector = (property.flags & MaterialProperty.PropFlags.HideInInspector) != 0;
            if (!hideInInspector && !skipProperty) {
                DrawStandard(property);
            }

            EditorGUI.indentLevel = originalIntentLevel;
        }
    }

    private void PromptTextureSave(MaterialEditor materialEditor, Func<Texture2D> generate, string propertyName,
        FilterMode filterMode) {
        var rampTexture = generate();
        var pngNameNoExtension = string.Format("{0}{1}-ramp", materialEditor.target.name, propertyName);
        var fullPath =
            EditorUtility.SaveFilePanel("Save Ramp Texture", "Assets", pngNameNoExtension, "png");
        if (fullPath.Length > 0) {
            SaveTextureAsPng(rampTexture, fullPath, filterMode);
            var loadedTexture = LoadTexture(fullPath);
            _target.SetTexture(propertyName, loadedTexture);
        }
    }

    private Texture2D GenerateStepTexture() {
        int numSteps = _celShadingNumSteps;
        var t2d = new Texture2D(numSteps + 1, /*height=*/1, TextureFormat.R8, /*mipChain=*/false) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int i = 0; i < numSteps + 1; i++) {
            var color = Color.white * i / numSteps;
            t2d.SetPixel(i, 0, color);
        }

        t2d.Apply();
        return t2d;
    }

    private Texture2D GenerateCurveTexture() {
        const int width = 256;
        const int height = 1;
        var lut = new Texture2D(width, height, TextureFormat.R8, /*mipChain=*/false) {
            alphaIsTransparency = false,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Trilinear
        };

        for (float x = 0; x < width; x++) {
            float value = _gradient.Evaluate(x / width);
            for (float y = 0; y < height; y++) {
                lut.SetPixel(Mathf.CeilToInt(x), Mathf.CeilToInt(y), Color.white * value);
            }
        }

        return lut;
    }

    private void SaveTextureAsPng(Texture2D texture, string fullPath, FilterMode filterMode) {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);
        AssetDatabase.Refresh();
        Debug.Log(string.Format("Texture saved as: {0}", fullPath));

        string pathRelativeToAssets = ConvertFullPathToAssetPath(fullPath);
        TextureImporter importer = (TextureImporter) TextureImporter.GetAtPath(pathRelativeToAssets);
        if (importer != null) {
            importer.filterMode = filterMode;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var textureSettings = new TextureImporterPlatformSettings {format = TextureImporterFormat.R8};
            importer.SetPlatformTextureSettings(textureSettings);
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        //22b5f7ed-989d-49d1-90d9-c62d76c3081a
        
        Debug.Assert(importer,
            string.Format("Could not change import settings of {0} [{1}]", fullPath, pathRelativeToAssets));
    }

    private static Texture2D LoadTexture(string fullPath) {
        string pathRelativeToAssets = ConvertFullPathToAssetPath(fullPath);
        var loadedTexture = AssetDatabase.LoadAssetAtPath(pathRelativeToAssets, typeof(Texture2D)) as Texture2D;
        if (loadedTexture == null) {
            Debug.LogError(string.Format("[FlatKit] Could not load texture from {0} [{1}].", fullPath,
                pathRelativeToAssets));
            return null;
        }

        loadedTexture.filterMode = FilterMode.Point;
        loadedTexture.wrapMode = TextureWrapMode.Clamp;

        return loadedTexture;
    }

    private static string ConvertFullPathToAssetPath(string fullPath) {
        return fullPath.Remove(0, fullPath.IndexOf("Assets", StringComparison.Ordinal));
    }
}