using GameManagers;
using UnityEngine;

namespace Satellite
{
    /// <summary>
    ///     卫星核心
    /// </summary>
    public class SatelliteCore : SatelliteEngine
    {
        public Satellite satellite;

        protected void Awake()
        {
            PartType = SatelliteType.Core;
        }

        private void OnCollisionEnter(Collision other)
        {
            GameManager.getGameManager.satelliteChallengeManger.satelliteResultType = SatelliteResultType.Crash;
            Debug.Log("Crash!");
        }

        public override float GetMass()
        {
            return satellite.GetMass();
        }
    }
}