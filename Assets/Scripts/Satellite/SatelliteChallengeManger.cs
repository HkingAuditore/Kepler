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
        public  global::Satellite.Satellite satellite;
        public  AstralBody                  target;
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
                        _satelliteResultType = value;
                        isSuccess   = true;
                        isCheckEnd  = true;
                        break;
                    case SatelliteResultType.Crash:
                        _satelliteResultType = value;
                        isSuccess   = false;
                        isCheckEnd  = true;
                        break;
                    case SatelliteResultType.NotOrbit:
                        _satelliteResultType = value;
                        isSuccess   = false;
                        isCheckEnd  = true;
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
            GameManager.GetGameManager.globalTimer.countingDownEndEvent.AddListener((() =>
                                                                                     {
                                                                                         _checkDistance = Vector3.Distance(satellite.satelliteCore.transform.position, target.transform.position);
                                                                                         _isInCheck     = true;

                                                                                     }));
            GameManager.GetGameManager.globalTimer.StartCounting();
        }

        private float _checkTime = 5f;
        private float _timer     = 0f;
        private void FixedUpdate()
        {
            if (_isInCheck)
            {
                _timer += Time.fixedDeltaTime;
                if ( _timer >= _checkTime)
                {
                    this.satelliteResultType = SatelliteResultType.Success;
                    return;
                }
                CheckSatelliteOrbit();
            }
        }

        private float _checkDistance;
        private void CheckSatelliteOrbit()
        {
            float distance = Vector3.Distance(satellite.satelliteCore.transform.position, target.transform.position);
            if (Mathf.Abs(distance - _checkDistance) > (.001f * _checkDistance))
            {
                this.satelliteResultType = SatelliteResultType.NotOrbit;
            }
        }

        public void CallCheck()
        {
            _checkDistance = Vector3.Distance(satellite.satelliteCore.transform.position, target.transform.position);
            _isInCheck     = true;

        }
    }
}