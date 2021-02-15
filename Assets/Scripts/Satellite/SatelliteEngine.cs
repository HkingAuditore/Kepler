namespace Satellite
{
    public class SatelliteEngine : SatellitePart
    {
        public int engineStage;
        protected override void Awake()
        {
            this.PartType = SatelliteType.Engine;
            base.Awake();
        }
    }
}
