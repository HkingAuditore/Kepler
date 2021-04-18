using UnityEngine;

namespace Satellite
{
    public class SatelliteEngine : SatellitePart
    {
        public int engineStage;

        protected void Awake()
        {
            PartType = SatelliteType.Engine;
        }

        public void SetCurDirVelocity(float speed)
        {
            Debug.Log(this.gameObject.name + " Changing Speed!");
            Debug.DrawLine(this.transform.position, this.transform.position+this.astralBodyRigidbody.velocity.normalized*speed,Color.green);
            this.ChangeVelocity(this.GetVelocity().normalized * speed);
        }
    }
}