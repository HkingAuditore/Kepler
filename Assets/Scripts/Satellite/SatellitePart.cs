using System.Collections.Generic;
using UnityEngine;

namespace Satellite
{
    public enum SatelliteType
    {
        Core,
        Engine,
        Parachute
    }

    public class SatellitePart : AstralBody
    {
        public string              satelliteName;
        public List<SatellitePart> connectedPartList = new List<SatellitePart>();

        private readonly Dictionary<string, FixedJoint> _connectedJoints = new Dictionary<string, FixedJoint>();

        // public float mass;
        public SatelliteType PartType      { get; set; }
        public Rigidbody     PartRigidbody { get; protected set; }

        protected virtual void Awake()
        {
            PartRigidbody = GetComponent<Rigidbody>();
        }

        public override void SetMass()
        {
            AstralBodyRigidbody.mass = Mass;
        }

        public void Push(Vector3 dir)
        {
            AstralBodyRigidbody.AddForce(dir);
        }

        public void Rotate(Vector3 dir)
        {
            AstralBodyRigidbody.AddTorque(dir);
        }

        public void GenerateJoint()
        {
            foreach (var part in connectedPartList)
            {
                var jointComponent = gameObject.AddComponent<FixedJoint>();
                _connectedJoints.Add(part.name, jointComponent);
                jointComponent.connectedBody = part.PartRigidbody;
            }
        }

        [ContextMenu("GetPartName")]
        public void GetPartName()
        {
            satelliteName = gameObject.name;
        }

        //分离连接到该部分的特定关节
        public void Separate(string partName)
        {
            Destroy(_connectedJoints[partName]);
        }

        public void Separate(bool separateAll)
        {
            if (separateAll)
            {
                connectedPartList.ForEach(part => Separate(part.name));
            }
            else
            {
                if (connectedPartList.Count > 0)
                    Separate(connectedPartList[0].name);
            }
        }
    }
}