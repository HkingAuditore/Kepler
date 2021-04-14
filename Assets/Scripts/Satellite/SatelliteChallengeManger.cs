using System;
using System.Collections;
using System.Collections.Generic;
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
        public global::Satellite.Satellite satellite;
        public float                       angleThreshold;
        public AstralBody                  target;
        public float                       checkTime = 5f;

        private SatelliteResultType         _satelliteResultType = SatelliteResultType.NonResult;
    
        private bool _isSuccess  = true;
        private bool _isInCheck  = false;
        private bool _isCheckEnd = false;

        public bool isSuccess
        {
            get => _isSuccess;
            set => _isSuccess = value;
        }

        public bool isCheckEnd
        {
            get => _isCheckEnd;
            set => _isCheckEnd = value;
        }

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
                    target.Mass *= 100;
                    break;
                case Difficulty.Normal:
                    target.Mass *= 10;
                    break;
                case Difficulty.Difficult:
                    target.Mass *= 5;
                    break;
                case Difficulty.Real:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            GameManager.GetGameManager.globalTimer.countingDownEndEvent.AddListener((() =>
                                                                                     {
                                                                                         // _checkDistance = Vector3.Distance(satellite.satelliteCore.transform.position, target.transform.position);
                                                                                         _isInCheck     = true;

                                                                                     }));
            GameManager.GetGameManager.globalTimer.StartCounting();
        }

        private float _timer     = 0f;
        private void FixedUpdate()
        {
            if (_isInCheck)
            {
                _timer += Time.fixedDeltaTime;
                if ( _timer >= checkTime)
                {
                    this.satelliteResultType = SatelliteResultType.Success;
                    return;
                }
                CheckSatelliteOrbit();
            }
        }


        private void CheckSatelliteOrbit()
        {
            Debug.DrawLine(satellite.satelliteCore.GetPosition(), satellite.satelliteCore.GetPosition() + satellite.satelliteCore.CalculateForce(),Color.magenta,60);
            Vector3 force     = satellite.satelliteCore.CalculateForce();
            Vector3 posVector = target.GetPosition() - satellite.satelliteCore.GetPosition();
            float   angle     = Vector3.Angle(force, posVector);
            if (angle > angleThreshold)
            {
                this.satelliteResultType = SatelliteResultType.NotOrbit;
            }
        }


        public void CallCheck()
        {
            _isInCheck                                       = true;
            GameManager.GetGameManager.globalTimer.isPausing = true;
        }
    }
}