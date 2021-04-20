using System;
using GameManagers;
using SpacePhysic;
using StaticClasses;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class VectorUI : MonoBehaviour
    {
        public  AstralBody   astralBody;
        public  string       header;
        public  float        showSize = .5f;
        public  vectorType   thisType;
        public  string       unit;
        public  LineRenderer vectorArrow;
        public  Text         vectorText;
        private Camera       _camera;
        private Vector3      _targetVector;


        private void Start()
        {
            _camera = GameManager.getGameManager.GetMainCameraController().GetMainCamera();
            Init();
        }

        private void FixedUpdate()
        {
            ShowVector();
            // int fontSize = (int)((_camera.orthographicSize / 185) * 12);
            // vectorText.fontSize = fontSize > 8 ? fontSize : 8;
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
            vectorArrow.positionCount = 0;
        } // ReSharper disable Unity.PerformanceAnalysis
        private void ShowVector()
        {
            switch (thisType)
            {
                case vectorType.Force:
                    _targetVector = astralBody.Force;
                    break;
                case vectorType.Velocity:
                    _targetVector = astralBody.GetVelocity();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_targetVector.magnitude <= 0)
            {
                vectorText.text = "";
                return;
            }


            transform.position = astralBody.transform.position;
            vectorArrow.SetPosition(0, astralBody.transform.position);
            vectorArrow.SetPosition(1, astralBody.transform.position + _targetVector * showSize);
            var tmpScreenPos = _camera.WorldToScreenPoint(astralBody.transform.position + showSize * _targetVector);
            // Debug.Log(this.gameObject.name + " : " + tmpScreenPos);
            transform.position = new Vector3(Mathf.Clamp(tmpScreenPos.x, 60, Screen.width  - 60),
                                             Mathf.Clamp(tmpScreenPos.y, 20, Screen.height - 20),
                                             0);
            var result = "";
            switch (thisType)
            {
                case vectorType.Force:
                    result = ((double) (_targetVector.magnitude * 1000)).ToSuperscript(2);
                    break;
                case vectorType.Velocity:
                    result = ((double) (_targetVector.magnitude * GameManager.getGameManager.GetK(PropertyUnit.M)))
                       .ToSuperscript(2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            vectorText.text = header + ":" + result + unit;
        }

        private void Init()
        {
            vectorArrow.positionCount = 2;
            ShowVector();
        }
    }
}