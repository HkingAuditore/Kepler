using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Editor
{
    public class MyLightingShaderGUI : ShaderGUI
    {
        private static readonly GUIContent staticLabel = new GUIContent();

        private static readonly ColorPickerHDRConfig emissionConfig =
            new ColorPickerHDRConfig(0f, 99f, 1f / 99f, 3f);

        private MaterialEditor     editor;
        private MaterialProperty[] properties;
        private bool               shouldShowAlphaCutoff;

        private Material target;

        public override void OnGUI(
            MaterialEditor editor, MaterialProperty[] properties
        )
        {
            target          = editor.target as Material;
            this.editor     = editor;
            this.properties = properties;
            DoRenderingMode();
            if (target.HasProperty("_TessellationUniform")) DoTessellation();
            if (target.HasProperty("_WireframeColor")) DoWireframe();
            DoMain();
            DoSecondary();
            DoAdvanced();
        }

        private void DoRenderingMode()
        {
            var mode = RenderingMode.Opaque;
            shouldShowAlphaCutoff = false;
            if (IsKeywordEnabled("_RENDERING_CUTOUT"))
            {
                mode                  = RenderingMode.Cutout;
                shouldShowAlphaCutoff = true;
            }
            else if (IsKeywordEnabled("_RENDERING_FADE"))
            {
                mode = RenderingMode.Fade;
            }
            else if (IsKeywordEnabled("_RENDERING_TRANSPARENT"))
            {
                mode = RenderingMode.Transparent;
            }

            EditorGUI.BeginChangeCheck();
            mode = (RenderingMode) EditorGUILayout.EnumPopup(
                                                             MakeLabel("Rendering Mode"), mode
                                                            );
            if (EditorGUI.EndChangeCheck())
            {
                RecordAction("Rendering Mode");
                SetKeyword("_RENDERING_CUTOUT", mode == RenderingMode.Cutout);
                SetKeyword("_RENDERING_FADE",   mode == RenderingMode.Fade);
                SetKeyword(
                           "_RENDERING_TRANSPARENT", mode == RenderingMode.Transparent
                          );

                var settings = RenderingSettings.modes[(int) mode];
                foreach (Material m in editor.targets)
                {
                    m.renderQueue = (int) settings.queue;
                    m.SetOverrideTag("RenderType", settings.renderType);
                    m.SetInt("_SrcBlend", (int) settings.srcBlend);
                    m.SetInt("_DstBlend", (int) settings.dstBlend);
                    m.SetInt("_ZWrite",   settings.zWrite ? 1 : 0);
                }
            }

            if (mode == RenderingMode.Fade || mode == RenderingMode.Transparent) DoSemitransparentShadows();
        }

        private void DoSemitransparentShadows()
        {
            EditorGUI.BeginChangeCheck();
            var semitransparentShadows =
                EditorGUILayout.Toggle(
                                       MakeLabel("Semitransp. Shadows", "Semitransparent Shadows"),
                                       IsKeywordEnabled("_SEMITRANSPARENT_SHADOWS")
                                      );
            if (EditorGUI.EndChangeCheck()) SetKeyword("_SEMITRANSPARENT_SHADOWS", semitransparentShadows);
            if (!semitransparentShadows) shouldShowAlphaCutoff = true;
        }

        private void DoMain()
        {
            GUILayout.Label("Main Maps", EditorStyles.boldLabel);

            var mainTex = FindProperty("_MainTex");
            editor.TexturePropertySingleLine(
                                             MakeLabel(mainTex, "Albedo (RGB)"), mainTex, FindProperty("_Color")
                                            );

            if (shouldShowAlphaCutoff) DoAlphaCutoff();
            DoMetallic();
            DoSmoothness();
            DoNormals();
            DoParallax();
            DoOcclusion();
            DoEmission();
            DoDetailMask();
            editor.TextureScaleOffsetProperty(mainTex);
        }

        private void DoAlphaCutoff()
        {
            var slider = FindProperty("_Cutoff");
            EditorGUI.indentLevel += 2;
            editor.ShaderProperty(slider, MakeLabel(slider));
            EditorGUI.indentLevel -= 2;
        }

        private void DoNormals()
        {
            var map = FindProperty("_NormalMap");
            var tex = map.textureValue;
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(
                                             MakeLabel(map), map,
                                             tex ? FindProperty("_BumpScale") : null
                                            );
            if (EditorGUI.EndChangeCheck() && tex != map.textureValue) SetKeyword("_NORMAL_MAP", map.textureValue);
        }

        private void DoMetallic()
        {
            var map = FindProperty("_MetallicMap");
            var tex = map.textureValue;
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(
                                             MakeLabel(map, "Metallic (R)"), map,
                                             tex ? null : FindProperty("_Metallic")
                                            );
            if (EditorGUI.EndChangeCheck() && tex != map.textureValue) SetKeyword("_METALLIC_MAP", map.textureValue);
        }

        private void DoSmoothness()
        {
            var source = SmoothnessSource.Uniform;
            if (IsKeywordEnabled("_SMOOTHNESS_ALBEDO"))
                source                                                = SmoothnessSource.Albedo;
            else if (IsKeywordEnabled("_SMOOTHNESS_METALLIC")) source = SmoothnessSource.Metallic;
            var slider = FindProperty("_Smoothness");
            EditorGUI.indentLevel += 2;
            editor.ShaderProperty(slider, MakeLabel(slider));
            EditorGUI.indentLevel += 1;
            EditorGUI.BeginChangeCheck();
            source = (SmoothnessSource) EditorGUILayout.EnumPopup(
                                                                  MakeLabel("Source"), source
                                                                 );
            if (EditorGUI.EndChangeCheck())
            {
                RecordAction("Smoothness Source");
                SetKeyword("_SMOOTHNESS_ALBEDO", source == SmoothnessSource.Albedo);
                SetKeyword(
                           "_SMOOTHNESS_METALLIC", source == SmoothnessSource.Metallic
                          );
            }

            EditorGUI.indentLevel -= 3;
        }

        private void DoParallax()
        {
            var map = FindProperty("_ParallaxMap");
            var tex = map.textureValue;
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(
                                             MakeLabel(map, "Parallax (G)"), map,
                                             tex ? FindProperty("_ParallaxStrength") : null
                                            );
            if (EditorGUI.EndChangeCheck() && tex != map.textureValue) SetKeyword("_PARALLAX_MAP", map.textureValue);
        }

        private void DoOcclusion()
        {
            var map = FindProperty("_OcclusionMap");
            var tex = map.textureValue;
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(
                                             MakeLabel(map, "Occlusion (G)"), map,
                                             tex ? FindProperty("_OcclusionStrength") : null
                                            );
            if (EditorGUI.EndChangeCheck() && tex != map.textureValue) SetKeyword("_OCCLUSION_MAP", map.textureValue);
        }

        private void DoEmission()
        {
            var map = FindProperty("_EmissionMap");
            var tex = map.textureValue;
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertyWithHDRColor(
                                               MakeLabel(map, "Emission (RGB)"), map, FindProperty("_Emission"),
                                               emissionConfig, false
                                              );
            editor.LightmapEmissionProperty(2);
            if (EditorGUI.EndChangeCheck())
            {
                if (tex != map.textureValue) SetKeyword("_EMISSION_MAP", map.textureValue);

                foreach (Material m in editor.targets)
                    m.globalIlluminationFlags &=
                        ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }

        private void DoDetailMask()
        {
            var mask = FindProperty("_DetailMask");
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(
                                             MakeLabel(mask, "Detail Mask (A)"), mask
                                            );
            if (EditorGUI.EndChangeCheck()) SetKeyword("_DETAIL_MASK", mask.textureValue);
        }

        private void DoSecondary()
        {
            GUILayout.Label("Secondary Maps", EditorStyles.boldLabel);

            var detailTex = FindProperty("_DetailTex");
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(
                                             MakeLabel(detailTex, "Albedo (RGB) multiplied by 2"), detailTex
                                            );
            if (EditorGUI.EndChangeCheck()) SetKeyword("_DETAIL_ALBEDO_MAP", detailTex.textureValue);
            DoSecondaryNormals();
            editor.TextureScaleOffsetProperty(detailTex);
        }

        private void DoSecondaryNormals()
        {
            var map = FindProperty("_DetailNormalMap");
            var tex = map.textureValue;
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(
                                             MakeLabel(map), map,
                                             tex ? FindProperty("_DetailBumpScale") : null
                                            );
            if (EditorGUI.EndChangeCheck() && tex != map.textureValue) SetKeyword("_DETAIL_NORMAL_MAP", map.textureValue);
        }

        private void DoAdvanced()
        {
            GUILayout.Label("Advanced Options", EditorStyles.boldLabel);

            editor.EnableInstancingField();
        }

        private void DoTessellation()
        {
            GUILayout.Label("Tessellation", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 2;

            var mode                                         = TessellationMode.Uniform;
            if (IsKeywordEnabled("_TESSELLATION_EDGE")) mode = TessellationMode.Edge;
            EditorGUI.BeginChangeCheck();
            mode = (TessellationMode) EditorGUILayout.EnumPopup(
                                                                MakeLabel("Mode"), mode
                                                               );
            if (EditorGUI.EndChangeCheck())
            {
                RecordAction("Tessellation Mode");
                SetKeyword("_TESSELLATION_EDGE", mode == TessellationMode.Edge);
            }

            if (mode == TessellationMode.Uniform)
                editor.ShaderProperty(
                                      FindProperty("_TessellationUniform"),
                                      MakeLabel("Uniform")
                                     );
            else
                editor.ShaderProperty(
                                      FindProperty("_TessellationEdgeLength"),
                                      MakeLabel("Edge Length")
                                     );
            EditorGUI.indentLevel -= 2;
        }

        private void DoWireframe()
        {
            GUILayout.Label("Wireframe", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 2;
            editor.ShaderProperty(
                                  FindProperty("_WireframeColor"),
                                  MakeLabel("Color")
                                 );
            editor.ShaderProperty(
                                  FindProperty("_WireframeSmoothing"),
                                  MakeLabel("Smoothing", "In screen space.")
                                 );
            editor.ShaderProperty(
                                  FindProperty("_WireframeThickness"),
                                  MakeLabel("Thickness", "In screen space.")
                                 );
            EditorGUI.indentLevel -= 2;
        }

        private MaterialProperty FindProperty(string name)
        {
            return FindProperty(name, properties);
        }

        private static GUIContent MakeLabel(string text, string tooltip = null)
        {
            staticLabel.text    = text;
            staticLabel.tooltip = tooltip;
            return staticLabel;
        }

        private static GUIContent MakeLabel(
            MaterialProperty property, string tooltip = null
        )
        {
            staticLabel.text    = property.displayName;
            staticLabel.tooltip = tooltip;
            return staticLabel;
        }

        private void SetKeyword(string keyword, bool state)
        {
            if (state)
                foreach (Material m in editor.targets)
                    m.EnableKeyword(keyword);
            else
                foreach (Material m in editor.targets)
                    m.DisableKeyword(keyword);
        }

        private bool IsKeywordEnabled(string keyword)
        {
            return target.IsKeywordEnabled(keyword);
        }

        private void RecordAction(string label)
        {
            editor.RegisterPropertyChangeUndo(label);
        }

        private enum SmoothnessSource
        {
            Uniform,
            Albedo,
            Metallic
        }

        private enum RenderingMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }

        private enum TessellationMode
        {
            Uniform,
            Edge
        }

        private struct RenderingSettings
        {
            public RenderQueue queue;
            public string      renderType;
            public BlendMode   srcBlend, dstBlend;
            public bool        zWrite;

            public static readonly RenderingSettings[] modes =
            {
                new RenderingSettings
                {
                    queue      = RenderQueue.Geometry,
                    renderType = "",
                    srcBlend   = BlendMode.One,
                    dstBlend   = BlendMode.Zero,
                    zWrite     = true
                },
                new RenderingSettings
                {
                    queue      = RenderQueue.AlphaTest,
                    renderType = "TransparentCutout",
                    srcBlend   = BlendMode.One,
                    dstBlend   = BlendMode.Zero,
                    zWrite     = true
                },
                new RenderingSettings
                {
                    queue      = RenderQueue.Transparent,
                    renderType = "Transparent",
                    srcBlend   = BlendMode.SrcAlpha,
                    dstBlend   = BlendMode.OneMinusSrcAlpha,
                    zWrite     = false
                },
                new RenderingSettings
                {
                    queue      = RenderQueue.Transparent,
                    renderType = "Transparent",
                    srcBlend   = BlendMode.One,
                    dstBlend   = BlendMode.OneMinusSrcAlpha,
                    zWrite     = false
                }
            };
        }
    }
}