namespace Satellite
{
    public class SatelliteParachute : SatellitePart
    {
        /// <summary>
        ///     卫星降落伞
        /// </summary>
        protected void Awake()
        {
            PartType = SatelliteType.Parachute;
        }
    }
}