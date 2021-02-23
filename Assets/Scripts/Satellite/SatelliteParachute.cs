namespace Satellite
{
    public class SatelliteParachute : SatellitePart
    {
        protected override void Awake()
        {
            PartType = SatelliteType.Parachute;
            base.Awake();
        }
    }
}