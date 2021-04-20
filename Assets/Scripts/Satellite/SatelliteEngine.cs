using UnityEngine;

namespace Satellite
{
    /// <summary>
    ///     卫星引擎
    /// </summary>
    public class SatelliteEngine : SatellitePart
    {
        public int engineStage;

        protected void Awake()
        {
            PartType = SatelliteType.Engine;
        }

        public void SetCurDirVelocity(float speed)
        {
            Debug.Log(gameObject.name + " Changing Speed!");
            Debug.DrawLine(transform.position, transform.position + astralBodyRigidbody.velocity.normalized * speed,
                           Color.green);
            ChangeVelocity(GetVelocity().normalized * speed);
        }
    }
}