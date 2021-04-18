using System;
using GameManagers;
using SpacePhysic;
using UnityEngine;

namespace Satellite
{
    public enum SatelliteResultType
    {
        Success,
        Crash,
        NotOrbit,
        NonResult
    }

    public class SatelliteChallengeManger : MonoBehaviour
    {
        public  Satellite  satellite;
        public  float      angleThreshold;
        public  AstralBody target;
        public  float      checkTime = 5f;
        private bool       _isInCheck;

        private SatelliteResultType _satelliteResultType = SatelliteResultType.NonResult;

        private float _timer;

        public bool isSuccess { get; set; } = true;

        public bool isCheckEnd { get; set; }

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
                        GameManager.GetGameManager.globalTimer.isPausing = true;
                        break;
                    case SatelliteResultType.Crash:
                        _satelliteResultType                             = value;
                        isSuccess                                        = false;
                        isCheckEnd                                       = true;
                        GameManager.GetGameManager.globalTimer.isPausing = true;
                        break;
                    case SatelliteResultType.NotOrbit:
                        _satelliteResultType                             = value;
                        isSuccess                                        = false;
                        isCheckEnd                                       = true;
                        GameManager.GetGameManager.globalTimer.isPausing = true;
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

            GameManager.GetGameManager.globalTimer.countingDownEndEvent.AddListener(() =>
                                                                                    {
                                                                                        // _checkDistance = Vector3.Distance(satellite.satelliteCore.transform.position, target.transform.position);
                                                                                        _isInCheck = true;
                                                                                    });
            GameManager.GetGameManager.globalTimer.StartCounting();
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


        public void CallCheck()
        {
            _isInCheck                                       = true;
            GameManager.GetGameManager.globalTimer.isPausing = true;
        }
    }
}