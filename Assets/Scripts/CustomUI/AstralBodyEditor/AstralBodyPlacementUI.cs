﻿using System.Collections;
using GameManagers;
using Quiz;
using SpacePhysic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI.AstralBodyEditor
{
    public class AstralBodyPlacementUI : MonoBehaviour
    {
        public  AstralBodyAddUI root;
        private Camera          _camera;
        private RectTransform   _horizontalLine;

        private bool           _inPlacing = true;
        private LineRenderer   _lineRenderer;
        private Transform      _orbitCore;
        private GravityTracing _orbits;
        private AstralBody     _placePrefab;
        private Text           _rangeText;
        private RectTransform  _verticalLine;


        public Vector3 Target { get; set; }

        private void Start()
        {
            _placePrefab    = root.placePrefab;
            _orbits         = root.orbits;
            _verticalLine   = transform.Find("Vertical").GetComponent<RectTransform>();
            _horizontalLine = transform.Find("Horizontal").GetComponent<RectTransform>();
            _rangeText      = transform.Find("RangeText").GetComponent<Text>();
            _lineRenderer   = GetComponent<LineRenderer>();
            _camera         = GameManager.getGameManager.GetMainCameraController().GetMainCamera();
            _orbitCore      = root.OrbitCore;
        }


        private void Update()
        {
            if (_inPlacing) Placing();
        }

        private void Placing()
        {
            Time.timeScale           = 0;
            _verticalLine.position   = new Vector3(Input.mousePosition.x,      _verticalLine.position.y, 0);
            _horizontalLine.position = new Vector3(_horizontalLine.position.x, Input.mousePosition.y,    0);
            _lineRenderer.SetPosition(0, _camera.ScreenToWorldPoint(Input.mousePosition));
            _lineRenderer.SetPosition(1, _orbitCore.position);
            _rangeText.transform.position = _camera.WorldToScreenPoint((_lineRenderer.GetPosition(0) +
                                                                        _lineRenderer.GetPosition(1)) * 0.5f);
            _rangeText.text =
                Vector3.Distance(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(1)).ToString("f2") + " m";
            if (Input.GetMouseButtonDown(0))
            {
                var mousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // Debug.Log("Mouse X: " + mousePosInWorld.x);
                // Debug.Log("Mouse Y: " + mousePosInWorld.y);
                var newAstralBody = Instantiate(_placePrefab, new Vector3(mousePosInWorld.x, 0, mousePosInWorld.z),
                                                Quaternion.LookRotation(new Vector3(0, 0, 0)), _orbits.transform);


                if (GameManager.getGameManager.isQuizEditMode)
                {
                    (GameManager.getGameManager.quizBase as QuizEditor)?.AddAstralBody((QuizAstralBody) newAstralBody);
                    newAstralBody.affectedPlanets.Add(GameManager.getGameManager.quizBase.target);
                }
                else
                {
                    (GameManager.getGameManager.sceneEditor)?.AddAstralBody((AstralBody) newAstralBody);
                    newAstralBody.affectedPlanets.Add(GameManager.getGameManager.sceneEditor.core);
                }

                _orbits.AddTracingTarget(newAstralBody);
                _inPlacing = false;
                root.Switch2Normal();
                Time.timeScale = 1;
                StartCoroutine(SetCircleVelocity(newAstralBody));
            }
            else if (Input.GetMouseButton(1))
            {
                _inPlacing = false;
                root.Switch2Normal();
                Time.timeScale = 1;
            }
        }


        private IEnumerator SetCircleVelocity(AstralBody astralBody)
        {
            yield return new WaitForSeconds(.2f);
            Debug.Log("Set Velocity!");
            astralBody.SetCircleVelocity();
        }


        public void SetPlacing()
        {
            _inPlacing = true;
        }
    }
}