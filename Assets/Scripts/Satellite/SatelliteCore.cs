using UnityEngine;

namespace Satellite
{
    public class SatelliteCore : SatelliteEngine
    {
        public Satellite satellite;
        protected override void Awake()
        {
            this.PartType = SatelliteType.Core;
            this.PartRigidbody = GetComponent<Rigidbody>();
        }

        public override float GetMass()
        {
            return satellite.GetMass();
        }
    }
}
