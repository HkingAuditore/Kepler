namespace Satellite
{
    /// <summary>
    ///     卫星结果
    /// </summary>
    public enum SatelliteResultType
    {
        /// <summary>
        ///     成功
        /// </summary>
        Success,

        /// <summary>
        ///     撞毁
        /// </summary>
        Crash,

        /// <summary>
        ///     未进入轨道
        /// </summary>
        NotOrbit,

        /// <summary>
        ///     无结果
        /// </summary>
        NonResult
    }
}