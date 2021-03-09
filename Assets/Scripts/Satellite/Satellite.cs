using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Satellite
{
    public class Satellite : MonoBehaviour
    {
        public List<SatellitePart> satelliteParts = new List<SatellitePart>();
        public SatelliteCore       satelliteCore;

        private void Start()
        {
            GenerateJoints();
        }

        public float GetMass()
        {
            return satelliteParts.Sum(part => part.GetMass());
        }

        public Transform GetTransform()
        {
            return satelliteCore.transform;
        }

        private void GenerateJoints()
        {
            foreach (var part in satelliteParts) part.GenerateJoint();
        }
    }
}