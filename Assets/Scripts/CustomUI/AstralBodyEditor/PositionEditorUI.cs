﻿using GameManagers;
using Quiz;
using SpacePhysic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI.AstralBodyEditor
{
    public class PositionEditorUI : MonoBehaviour
    {
        public  AstralBody editingTarget;
        public  bool       isQuizEditor;
        public  float      moveSpeed;
        public  Button     xAxis;
        public  Button     zAxis;
        private AstralBody _astralBody;

        private Camera _camera;


        private Vector3 oriMousePos;

        private void Start()
        {
            _camera = GameManager.getGameManager.GetMainCameraController().GetMainCamera();
        }


        private void Update()
        {
            transform.position = new Vector3(editingTarget.transform.position.x, editingTarget.transform.position.y,
                                             editingTarget.transform.position.z);
            transform.localScale = _camera.orthographicSize / 185 * new Vector3(1, 1, 1);
        }

        public void OnBeginDrag()
        {
            GameManager.getGameManager.GetMainCameraController().IsFollowing = false;
            oriMousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        }

        public void MoveAxis(bool isXAxis)
        {
            var mousePos   = _camera.ScreenToWorldPoint(Input.mousePosition);
            var deltaValue = mousePos - oriMousePos;
            oriMousePos = mousePos;
            editingTarget.transform.position +=
                new Vector3(deltaValue.x * (isXAxis ? 1 : 0), 0, deltaValue.z * (isXAxis ? 0 : 1));
        }

        public void MoveCenter()
        {
            var mousePos   = _camera.ScreenToWorldPoint(Input.mousePosition);
            var deltaValue = mousePos - oriMousePos;
            oriMousePos                      =  mousePos;
            editingTarget.transform.position += new Vector3(deltaValue.x, 0, deltaValue.z);
        }


        public void OnEndDrag()
        {
            GameManager.getGameManager.GetMainCameraController().IsFollowing = true;
            if (isQuizEditor) ((QuizAstralBody) editingTarget).UpdateHighCost();
        }
    }
}