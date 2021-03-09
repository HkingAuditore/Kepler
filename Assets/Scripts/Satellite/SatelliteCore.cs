using UnityEngine;

namespace Satellite
{
    public class SatelliteCore : SatelliteEngine
    {
        public Satellite satellite;

        protected override void Awake()
        {
            PartType      = SatelliteType.Core;
            PartRigidbody = GetComponent<Rigidbody>();
        }

        public override float GetMass()
        {
            return satellite.GetMass();
        }
    }
}