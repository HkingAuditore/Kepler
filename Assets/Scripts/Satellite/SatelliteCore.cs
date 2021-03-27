using UnityEngine;

namespace Satellite
{
    public class SatelliteCore : SatelliteEngine
    {
        public Satellite satellite;

        protected void Awake()
        {
            PartType      = SatelliteType.Core;
        }

        public override float GetMass()
        {
            return satellite.GetMass();
        }
    }
}