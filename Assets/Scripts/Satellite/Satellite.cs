using UnityEngine;

namespace Satellite
{
    public class Satellite : AstralBody
    {
        public void Push(Vector3 dir) => this.Rigidbody.AddForce(dir);

        public void Rotate(Vector3 dir) => this.Rigidbody.AddTorque(dir);
    }
}
