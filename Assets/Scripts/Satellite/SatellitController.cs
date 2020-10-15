using System;
using UnityEngine;

namespace Satellite
{
    public class SatellitController : MonoBehaviour
    {
        public float speed;
        public float angularSpeed;
        private Satellite _satellite;
        

        private void Start()
        {
            _satellite = this.GetComponent<Satellite>();
        }

        private void FixedUpdate()
        {
            Rotate();
            Push();
        }

        public void Rotate()
        {
            if (Input.GetKey(KeyCode.A))
            {
                this._satellite.Rotate(-this.transform.up * angularSpeed);
            }

            if (Input.GetKey(KeyCode.D))
            {
                this._satellite.Rotate(this.transform.up  * angularSpeed);
            }
            
        }

        public void Push()
        {
            if (Input.GetKey(KeyCode.W))
            {
                this._satellite.Push(this.transform.forward * speed);
            }

            if (Input.GetKey(KeyCode.S))
            {
                this._satellite.Push(-this.transform.forward * speed);
            }

            
        }
    }
}
