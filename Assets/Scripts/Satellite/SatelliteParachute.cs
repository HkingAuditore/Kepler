namespace Satellite
{        
    /// <summary>
    ///     卫星降落伞
    /// </summary>
    public class SatelliteParachute : SatellitePart
    {

        protected void Awake()
        {
            PartType = SatelliteType.Parachute;
        }
    }
}