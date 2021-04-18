using GameManagers;
using UnityEngine;

namespace Satellite
{
    public class SatelliteCore : SatelliteEngine
    {
        public Satellite satellite;

        protected void Awake()
        {
            PartType = SatelliteType.Core;
        }

        private void OnCollisionEnter(Collision other)
        {
            GameManager.GetGameManager.satelliteChallengeManger.satelliteResultType = SatelliteResultType.Crash;
            Debug.Log("Crash!");
        }

        public override float GetMass()
        {
            return satellite.GetMass();
        }
    }
}