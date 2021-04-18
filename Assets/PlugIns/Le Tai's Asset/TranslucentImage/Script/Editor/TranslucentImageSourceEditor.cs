using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LeTai.Asset.TranslucentImage.Editor
{
    [CustomEditor(typeof(TranslucentImageSource))]
    [CanEditMultipleObjects]
    public class TranslucentImageSourceEditor : UnityEditor.Editor
    {
        private UnityEditor.Editor configEditor;

        public UnityEditor.Editor ConfigEditor
        {
            get
            {
                if (configEditor == null)
                {
                    var config = ((TranslucentImageSource) target).BlurConfig;
                    if (config != null)
                        configEditor = CreateEditor(config);
                }

                return configEditor;
            }
        }

        public override void OnInspectorGUI()
        {
            var tiSource = (TranslucentImageSource) target;

            tiSource.BlurConfig = (BlurConfig) EditorGUILayout.ObjectField("Config file",
                                                                           tiSource.BlurConfig,
                                                                           typeof(BlurConfig),
                                                                           false);

            if (tiSource.BlurConfig == null)
            {
                EditorGUILayout.HelpBox("Assign a Blur configuration asset, or create a new one", MessageType.Warning);
                if (GUILayout.Button("Create New Configuration File"))
                {
                    var config = CreateInstance<ScalableBlurConfig>();

                    var path =
                        AssetDatabase.GenerateUniqueAssetPath("Assets/"                          +
                                                              SceneManager.GetActiveScene().name +
                                                              " Blur Config.asset");
                    AssetDatabase.CreateAsset(config, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorGUIUtility.PingObject(config);
                    tiSource.BlurConfig = config;
                }
            }
            else
            {
                ConfigEditor.OnInspectorGUI();
            }

            EditorGUILayout.Space();

            //Common properties
            tiSource.Downsample = EditorGUILayout.IntSlider(new GUIContent(downsampleLabel),
                                                            tiSource.Downsample,
                                                            Min,
                                                            MaxDownsample);
            tiSource.BlurRegion    = EditorGUILayout.RectField(regionLabel, tiSource.BlurRegion);
            tiSource.maxUpdateRate = EditorGUILayout.FloatField(updateRateLabel, tiSource.maxUpdateRate);
            tiSource.preview       = EditorGUILayout.Toggle(previewLabel, tiSource.preview);

            EditorUtility.SetDirty(target);
            Undo.RecordObject(target, "Change Translucent Image Source property");
        }

        #region constants

        private const int Min           = 0;
        private const int MaxDownsample = 6;

        private readonly GUIContent downsampleLabel = new GUIContent("Downsample",
                                                                     "Reduce the size of the screen before processing. Increase will improve performance but create more artifact.");

        private readonly GUIContent regionLabel = new GUIContent("Blur Region",
                                                                 "Choose which part of the screen to blur. Blur smaller region is faster.");

        private readonly GUIContent updateRateLabel = new GUIContent("Max Update Rate",
                                                                     "How many time to blur per second. Reduce to increase performance and save battery for slow moving background");

        private readonly GUIContent previewLabel = new GUIContent("Preview",
                                                                  "Preview the effect over the entire screen");

        #endregion
    }
}