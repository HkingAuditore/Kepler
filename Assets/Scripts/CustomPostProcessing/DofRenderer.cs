using Arts.Shaders.CustomPostProcessing;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Scripting;

namespace PostProcessing
{
    [Preserve]
    // TODO: Doesn't play nice with alpha propagation, see if it can be fixed without killing performances
    internal sealed class DofRenderer : PostProcessEffectRenderer<Dof>
    {
        // Ping-pong between two history textures as we can't read & write the same target in the
        // same pass
        private const int k_NumEyes               = 2;
        private const int k_NumCoCHistoryTextures = 2;

        // Height of the 35mm full-frame format (36mm x 24mm)
        // TODO: Should be set by a physical camera
        private const    float             k_FilmHeight         = 0.024f;
        private readonly RenderTexture[][] m_CoCHistoryTextures = new RenderTexture[k_NumEyes][];
        private readonly int[]             m_HistoryPingPong    = new int[k_NumEyes];

        public DofRenderer()
        {
            for (var eye = 0; eye < k_NumEyes; eye++)
            {
                m_CoCHistoryTextures[eye] = new RenderTexture[k_NumCoCHistoryTextures];
                m_HistoryPingPong[eye]    = 0;
            }
        }

        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.Depth;
        }


        private RenderTextureFormat SelectFormat(RenderTextureFormat primary, RenderTextureFormat secondary)
        {
            if (primary.IsSupported())
                return primary;

            if (secondary.IsSupported())
                return secondary;

            return RenderTextureFormat.Default;
        }

        private float CalculateMaxCoCRadius(int screenHeight)
        {
            // Estimate the allowable maximum radius of CoC from the kernel
            // size (the equation below was empirically derived).
            var radiusInPixels = (float) settings.kernelSize.value * 4f + 6f;

            // Applying a 5% limit to the CoC radius to keep the size of
            // TileMax/NeighborMax small enough.
            return Mathf.Min(0.05f, radiusInPixels / screenHeight);
        }

        private RenderTexture CheckHistory(int                 eye, int id, PostProcessRenderContext context,
                                           RenderTextureFormat format)
        {
            var rt = m_CoCHistoryTextures[eye][id];

            if (m_ResetHistory || rt == null || !rt.IsCreated() || rt.width != context.width ||
                rt.height            != context.height)
            {
                RenderTexture.ReleaseTemporary(rt);

                rt            = context.GetScreenSpaceTemporaryRT(0, format, RenderTextureReadWrite.Linear);
                rt.name       = "CoC History, Eye: " + eye + ", ID: " + id;
                rt.filterMode = FilterMode.Bilinear;
                rt.Create();
                m_CoCHistoryTextures[eye][id] = rt;
            }

            return rt;
        }

        public override void Render(PostProcessRenderContext context)
        {
            // The coc is stored in alpha so we need a 4 channels target. Note that using ARGB32
            // will result in a very weak near-blur.
            var colorFormat = context.camera.allowHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
            var cocFormat   = SelectFormat(RenderTextureFormat.R8, RenderTextureFormat.RHalf);

            // Material setup
            var scaledFilmHeight = k_FilmHeight               * (context.height / 1080f);
            var f                = settings.focalLength.value / 1000f;
            var s1               = Mathf.Max(settings.focusDistance.value, f);
            var aspect           = context.screenWidth / (float) context.screenHeight;
            var coeff            = f * f               / (settings.aperture.value * (s1 - f) * scaledFilmHeight * 2f);
            var maxCoC           = CalculateMaxCoCRadius(context.screenHeight);

            var sheet = context.propertySheets.Get(Shader.Find("CustomPostProcessing/DepthOfField"));
            sheet.properties.Clear();
            sheet.properties.SetFloat(ShaderIDs.Distance,  s1);
            sheet.properties.SetFloat(ShaderIDs.LensCoeff, coeff);
            sheet.properties.SetFloat(ShaderIDs.MaxCoC,    maxCoC);
            sheet.properties.SetFloat(ShaderIDs.RcpMaxCoC, 1f / maxCoC);
            sheet.properties.SetFloat(ShaderIDs.RcpAspect, 1f / aspect);

            var cmd = context.command;
            cmd.BeginSample("DepthOfField");

            // CoC calculation pass
            context.GetScreenSpaceTemporaryRT(cmd, ShaderIDs.CoCTex, 0, cocFormat, RenderTextureReadWrite.Linear);
            cmd.BlitFullscreenTriangle(BuiltinRenderTextureType.None, ShaderIDs.CoCTex, sheet,
                                       (int) Pass.CoCCalculation);

            // CoC temporal filter pass when TAA is enabled
            if (context.IsTemporalAntialiasingActive())
            {
                var motionBlending = context.temporalAntialiasing.motionBlending;
                var blend          = m_ResetHistory ? 0f : motionBlending; // Handles first frame blending
                var jitter         = context.temporalAntialiasing.jitter;

                sheet.properties.SetVector(ShaderIDs.TaaParams, new Vector3(jitter.x, jitter.y, blend));

                var pp           = m_HistoryPingPong[context.xrActiveEye];
                var historyRead  = CheckHistory(context.xrActiveEye, ++pp % 2, context, cocFormat);
                var historyWrite = CheckHistory(context.xrActiveEye, ++pp % 2, context, cocFormat);
                m_HistoryPingPong[context.xrActiveEye] = ++pp % 2;

                cmd.BlitFullscreenTriangle(historyRead, historyWrite, sheet, (int) Pass.CoCTemporalFilter);
                cmd.ReleaseTemporaryRT(ShaderIDs.CoCTex);
                cmd.SetGlobalTexture(ShaderIDs.CoCTex, historyWrite);
            }

            // Downsampling and prefiltering pass
            context.GetScreenSpaceTemporaryRT(cmd, ShaderIDs.DepthOfFieldTex, 0, colorFormat,
                                              RenderTextureReadWrite.Default, FilterMode.Bilinear, context.width / 2,
                                              context.height                                                     / 2);
            cmd.BlitFullscreenTriangle(context.source, ShaderIDs.DepthOfFieldTex, sheet,
                                       (int) Pass.DownsampleAndPrefilter);

            // Bokeh simulation pass
            context.GetScreenSpaceTemporaryRT(cmd, ShaderIDs.DepthOfFieldTemp, 0, colorFormat,
                                              RenderTextureReadWrite.Default, FilterMode.Bilinear, context.width / 2,
                                              context.height                                                     / 2);
            cmd.BlitFullscreenTriangle(ShaderIDs.DepthOfFieldTex, ShaderIDs.DepthOfFieldTemp, sheet,
                                       (int) Pass.BokehSmallKernel + (int) settings.kernelSize.value);

            // Postfilter pass
            cmd.BlitFullscreenTriangle(ShaderIDs.DepthOfFieldTemp, ShaderIDs.DepthOfFieldTex, sheet,
                                       (int) Pass.PostFilter);
            cmd.ReleaseTemporaryRT(ShaderIDs.DepthOfFieldTemp);

            // Debug overlay pass
            if (context.IsDebugOverlayEnabled(DebugOverlay.DepthOfField))
                context.PushDebugOverlay(cmd, context.source, sheet, (int) Pass.DebugOverlay);

            // Combine pass
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, (int) Pass.Combine);
            cmd.ReleaseTemporaryRT(ShaderIDs.DepthOfFieldTex);

            if (!context.IsTemporalAntialiasingActive())
                cmd.ReleaseTemporaryRT(ShaderIDs.CoCTex);

            cmd.EndSample("DepthOfField");

            m_ResetHistory = false;
        }

        public override void Release()
        {
            for (var eye = 0; eye < k_NumEyes; eye++)
            {
                for (var i = 0; i < m_CoCHistoryTextures[eye].Length; i++)
                {
                    RenderTexture.ReleaseTemporary(m_CoCHistoryTextures[eye][i]);
                    m_CoCHistoryTextures[eye][i] = null;
                }

                m_HistoryPingPong[eye] = 0;
            }

            ResetHistory();
        }

        private enum Pass
        {
            CoCCalculation,
            CoCTemporalFilter,
            DownsampleAndPrefilter,
            BokehSmallKernel,
            BokehMediumKernel,
            BokehLargeKernel,
            BokehVeryLargeKernel,
            PostFilter,
            Combine,
            DebugOverlay
        }
    }
}