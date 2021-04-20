using System.Collections.Generic;
using SpacePhysic;
using UnityEngine;

namespace Satellite
{
    /// <summary>
    ///     卫星部件
    /// </summary>
    public class SatellitePart : AstralBody
    {
        /// <summary>
        ///     连接的部件
        /// </summary>
        public List<SatellitePart> connectedPartList = new List<SatellitePart>();

        public           string                         satelliteName;
        private readonly Dictionary<string, FixedJoint> _connectedJoints = new Dictionary<string, FixedJoint>();

        // public float mass;
        /// <summary>
        ///     部件类型
        /// </summary>
        public SatelliteType PartType { get; set; }


        private protected override void SetMass()
        {
            astralBodyRigidbody.mass = Mass;
        }

        public void Push(Vector3 dir)
        {
            astralBodyRigidbody.AddForce(dir);
        }

        public void Rotate(Vector3 dir)
        {
            astralBodyRigidbody.AddTorque(dir);
        }

        public void GenerateJoint()
        {
            foreach (var part in connectedPartList)
            {
                var jointComponent = gameObject.AddComponent<FixedJoint>();
                _connectedJoints.Add(part.name, jointComponent);
                jointComponent.connectedBody = part.astralBodyRigidbody;
            }
        }

        [ContextMenu("GetPartName")]
        public void GetPartName()
        {
            satelliteName = gameObject.name;
        }


        /// <summary>
        ///     分离连接到该部分的特定关节
        /// </summary>
        /// <param name="partName"></param>
        public void Separate(string partName)
        {
            Destroy(_connectedJoints[partName]);
        }

        /// <summary>
        ///     分离
        /// </summary>
        /// <param name="separateAll">是否完全分离</param>
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