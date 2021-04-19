using System;
using UnityEngine.Rendering.PostProcessing;

namespace PostProcessing
{
    /// <summary>
    ///     A volume parameter holding a <see cref="KernelSize" /> value.
    /// </summary>
    [Serializable]
    public sealed class KernelSizeParameter : ParameterOverride<KernelSize>
    {
    }
}