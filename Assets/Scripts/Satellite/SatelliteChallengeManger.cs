using System;
using GameManagers;
using SpacePhysic;
using UnityEngine;

namespace Satellite
{
    /// <summary>
    /// 航天挑战管理
    /// </summary>
    public class SatelliteChallengeManger : MonoBehaviour
    {
        /// <summary>
        ///     角度容错
        /// </summary>
        public float angleThreshold;

        /// <summary>
        ///     检查等待时间
        /// </summary>
        public float checkTime = 5f;

        /// <summary>
        ///     操控卫星
        /// </summary>
        public Satellite satellite;

        /// <summary>
        ///     目标星球
        /// </summary>
        public AstralBody target;

        private bool                _isInCheck;
        private SatelliteResultType _satelliteResultType = SatelliteResultType.NonResult;
        private float               _timer;

        /// <summary>
        ///     是否成功
        /// </summary>
        public bool isSuccess { get; set; } = true;

        /// <summary>
        ///     是否检查完成
        /// </summary>
        public bool isCheckEnd { get; set; }

        /// <summary>
        ///     结果
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public SatelliteResultType satelliteResultType
        {
            get => _satelliteResultType;
            set
            {
                switch (value)
                {
                    case SatelliteResultType.Success:
                        _satelliteResultType                             = value;
                        isSuccess                                        = true;
                        isCheckEnd                                       = true;
                        GameManager.getGameManager.globalTimer.isPausing = true;
                        break;
                    case SatelliteResultType.Crash:
                        _satelliteResultType                             = value;
                        isSuccess                                        = false;
                        isCheckEnd                                       = true;
                        GameManager.getGameManager.globalTimer.isPausing = true;
                        break;
                    case SatelliteResultType.NotOrbit:
                        _satelliteResultType                             = value;
                        isSuccess                                        = false;
                        isCheckEnd                                       = true;
                        GameManager.getGameManager.globalTimer.isPausing = true;
                        break;
                    case SatelliteResultType.NonResult:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        private void Start()
        {
            switch (GlobalTransfer.getGlobalTransfer.difficulty)
            {
                case Difficulty.Easy:
                    target.realMass *= 100;
                    break;
                case Difficulty.Normal:
                    target.realMass *= 10;
                    break;
                case Difficulty.Difficult:
                    target.realMass *= 5;
                    break;
                case Difficulty.Real:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GameManager.getGameManager.globalTimer.countingDownEndEvent.AddListener(() =>
                                                                                    {
                                                                                        // _checkDistance = Vector3.Distance(satellite.satelliteCore.transform.position, target.transform.position);
                                                                                        _isInCheck = true;
                                                                                    });
            GameManager.getGameManager.globalTimer.StartCounting();
        }

        private void FixedUpdate()
        {
            if (_isInCheck)
            {
                _timer += Time.fixedDeltaTime;
                if (_timer >= checkTime)
                {
                    satelliteResultType = SatelliteResultType.Success;
                    return;
                }

                CheckSatelliteOrbit();
            }
        }


        private void CheckSatelliteOrbit()
        {
            Debug.DrawLine(satellite.satelliteCore.GetPosition(),
                           satellite.satelliteCore.GetPosition() + satellite.satelliteCore.CalculateForce(),
                           Color.magenta, 60);
            var force = satellite.satelliteCore.CalculateForce();
            var posVector = target.GetPosition() - satellite.satelliteCore.GetPosition();
            var angle = Vector3.Angle(force, posVector);
            if (angle > angleThreshold) satelliteResultType = SatelliteResultType.NotOrbit;
        }

        /// <summary>
        ///     检查
        /// </summary>
        public void CallCheck()
        {
            _isInCheck                                       = true;
            GameManager.getGameManager.globalTimer.isPausing = true;
        }
    }
}