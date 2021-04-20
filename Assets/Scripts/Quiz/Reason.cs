namespace Quiz
{
    /// <summary>
    ///     问题结果
    /// </summary>
    public enum Reason
    {
        /// <summary>
        ///     正确
        /// </summary>
        Right,

        /// <summary>
        ///     非圆形轨道
        /// </summary>
        NonCircleOrbit,

        /// <summary>
        ///     撞毁
        /// </summary>
        Crash,

        /// <summary>
        ///     超时
        /// </summary>
        Overtime
    }
}