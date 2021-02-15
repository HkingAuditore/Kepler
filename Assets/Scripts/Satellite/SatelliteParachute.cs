namespace Satellite
{
    public class SatelliteParachute : SatellitePart
    {
        protected override void Awake()
        {
            this.PartType = SatelliteType.Parachute;
            base.Awake();
        }
    }
}
