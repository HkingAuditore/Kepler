﻿using System;
using Dreamteck.Splines;
using GameManagers;
using SpacePhysic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class LengthUI : MonoBehaviour
    {
        public  AstralBody       astralBody;
        public  LengthCalculator lengthCalculator;
        public  SplineComputer   lengthSpline;
        public  Text             lengthText;
        public  AstralBody       targetAstralBody;
        private Camera           _camera;

        private void Update()
        {
            ShowLength();
        }

        private void Start()
        {
            _camera = GameManager.getGameManager.mainCamera;
        }

        private void OnEnable()
        {
            InitLength();
        }

        private void OnDisable()
        {
            RemoveLength();
        }

        private void InitLength()
        {
            _camera = GameManager.getGameManager.mainCamera;
            var nodeOri = astralBody.gameObject.GetComponent<Node>();
            if (nodeOri == null)
                nodeOri = astralBody.gameObject.AddComponent<Node>();
            nodeOri.transformNormals  = false;
            nodeOri.transformSize     = false;
            nodeOri.transformTangents = false;
            var nodeTarget = targetAstralBody.gameObject.AddComponent<Node>();
            nodeTarget.transformNormals  = false;
            nodeTarget.transformSize     = false;
            nodeTarget.transformTangents = false;
            lengthSpline.SetPoints(new[]
                                   {
                                       new SplinePoint(new Vector3(0, 0, 0)),
                                       new SplinePoint(new Vector3(0, 0, 0))
                                   });
            nodeOri.AddConnection(lengthSpline, 0);
            // Debug.Log(nodeOri.HasConnection(lengthSpline, 0));
            nodeTarget.AddConnection(lengthSpline, 1);
            lengthSpline.Rebuild();
        }

        private void RemoveLength()
        {
            lengthSpline.SetPoints(new SplinePoint[] { });
            Destroy(astralBody.gameObject.GetComponent<Node>());
            Destroy(targetAstralBody.gameObject.GetComponent<Node>());
        }

        private void ShowLength()
        {
            var tmpScreenPos =
                _camera.WorldToScreenPoint((astralBody.transform.position + targetAstralBody.transform.position) * .5f);
            // Debug.Log(this.gameObject.name + " : " + tmpScreenPos);

            lengthText.transform.position = new Vector3(Mathf.Clamp(tmpScreenPos.x, 60, Screen.width  - 60),
                                                        Mathf.Clamp(tmpScreenPos.y, 20, Screen.height - 20),
                                                        0);
            lengthText.text = "距离:" + (lengthCalculator.length * 1000).ToString("f2") + " km";
        }
    }
}