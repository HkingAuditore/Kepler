namespace Satellite
{
    public class SatelliteParachute : SatellitePart
    {
        protected void Awake()
        {
            PartType = SatelliteType.Parachute;
        }
    }
}