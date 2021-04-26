using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Satellite
{
    /// <summary>
    /// 卫星整体
    /// </summary>
    public class Satellite : MonoBehaviour
    {
        /// <summary>
        ///     卫星核心
        /// </summary>
        public SatelliteCore satelliteCore;

        /// <summary>
        ///     卫星部件
        /// </summary>
        public List<SatellitePart> satelliteParts = new List<SatellitePart>();

        private void Start()
        {
            GenerateJoints();
        }

        /// <summary>
        ///     获取质量
        /// </summary>
        /// <returns></returns>
        public float GetMass()
        {
            return satelliteParts.Sum(part => part.Mass);
        }

        /// <summary>
        ///     获取坐标
        /// </summary>
        /// <returns></returns>
        public Transform GetTransform()
        {
            return satelliteCore.transform;
        }

        private void GenerateJoints()
        {
            foreach (var part in satelliteParts) part.GenerateJoint();
        }

        /// <summary>
        ///     获取速度
        /// </summary>
        /// <returns></returns>
        public Vector3 GetVelocity()
        {
            return satelliteCore.GetVelocity();
        }
    }
}