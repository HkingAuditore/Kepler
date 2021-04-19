using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace PostProcessing
{
    /// <summary>
    ///     This class holds settings for the Depth of Field effect.
    /// </summary>
    [Serializable]
    [PostProcess(typeof(DofRenderer), PostProcessEvent.AfterStack,
                 "Custom/Dof Custom")]
    public sealed class Dof : PostProcessEffectSettings
    {
        /// <summary>
        ///     The distance to the point of focus.
        /// </summary>
        [UnityEngine.Rendering.PostProcessing.Min(0.1f)] [Tooltip("Distance to the point of focus.")]
        public FloatParameter focusDistance = new FloatParameter {value = 10f};

        /// <summary>
        ///     The ratio of the aperture (known as f-stop or f-number). The smaller the value is, the
        ///     shallower the depth of field is.
        /// </summary>
        [Range(0.05f, 32f)]
        [Tooltip("Ratio of aperture (known as f-stop or f-number). The smaller the value is, the shallower the depth of field is.")]
        public FloatParameter aperture = new FloatParameter {value = 5.6f};

        /// <summary>
        ///     The distance between the lens and the film. The larger the value is, the shallower the
        ///     depth of field is.
        /// </summary>
        [Range(1f, 300f)]
        [Tooltip("Distance between the lens and the film. The larger the value is, the shallower the depth of field is.")]
        public FloatParameter focalLength = new FloatParameter {value = 50f};

        /// <summary>
        ///     The convolution kernel size of the bokeh filter, which determines the maximum radius of
        ///     bokeh. It also affects the performance (the larger the kernel is, the longer the GPU
        ///     time is required).
        /// </summary>
        [DisplayName("Max Blur Size")]
        [Tooltip("Convolution kernel size of the bokeh filter, which determines the maximum radius of bokeh. It also affects performances (the larger the kernel is, the longer the GPU time is required).")]
        public KernelSizeParameter kernelSize = new KernelSizeParameter {value = KernelSize.Medium};

        /// <summary>
        ///     Returns <c>true</c> if the effect is currently enabled and supported.
        /// </summary>
        /// <param name="context">The current post-processing render context</param>
        /// <returns><c>true</c> if the effect is currently enabled and supported</returns>
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value
                && SystemInfo.graphicsShaderLevel >= 35;
        }
    }
}