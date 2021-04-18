using System;
using GameManagers;
using MathPlus;
using SpacePhysic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class OrbitPanelUI : MonoBehaviour
    {
        public  GameObject     contentPanel;
        public  GameObject     nullPanel;
        public  bool           isConicSection;
        public  AstralBody     _astralBody;
        public  Text           majorAxis;
        public  Text           minorAxis;
        public  Text           geoCenter;
        public  Text           eccentricity;
        public  Text           focalLength;
        public  Text           period;
        public  Text           angle;
        public  Text           k;
        public  OrbitGraphUI   orbitGraphUI;
        private GravityTracing _gravityTracing;
        public  ConicSection   orbit;

        public AstralBody astralBody
        {
            get => _astralBody;
            set => _astralBody = value;
        }

        private void Awake()
        {
            _gravityTracing = GameManager.GetGameManager.orbit;
        }

        private void FixedUpdate()
        {
            Init();
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
        }


        public void Init()
        {
            // Debug.Log(_gravityTracing);
            // Debug.Log(astralBody);

            try
            {
                orbit          = _gravityTracing.GetConicSection(astralBody);
                isConicSection = true;
            }
            catch (Exception e)
            {
                isConicSection = false;
            }


            if (isConicSection && !float.IsNaN(orbit.semiMajorAxis) && !float.IsNaN(orbit.semiMinorAxis))
            {
                contentPanel.SetActive(true);
                nullPanel.SetActive(false);
                majorAxis.text = "长轴:" + orbit.semiMajorAxis.ToString("f2") + " m";
                minorAxis.text = "短轴:" + orbit.semiMinorAxis.ToString("f2") + " m";
                geoCenter.text = "几何中心: (" + orbit.geoCenter.x.ToString("f2") + ", " + orbit.geoCenter.y.ToString("f2") +
                                 " )";
                eccentricity.text = "离心率: " + orbit.eccentricity.ToString("f2");
                focalLength.text  = "焦距: "  + orbit.focalLength.ToString("f2")                              + " m";
                period.text       = "周期: "  + orbit.GetT(astralBody.affectedPlanets[0].Mass).ToString("f2") + " s";
                angle.text        = "倾角: "  + orbit.angle.ToString("f2")                                    + " °";
                k.text = "T²/a³ :" + orbit.GetT(astralBody.affectedPlanets[0].Mass) *
                    orbit.GetT(astralBody.affectedPlanets[0].Mass) /
                    (orbit.semiMajorAxis * orbit.semiMajorAxis * orbit.semiMajorAxis);
                orbitGraphUI.astralBody = astralBody;
                orbitGraphUI.orbit      = orbit;
                orbitGraphUI.gameObject.SetActive(true);
                orbitGraphUI.Init();
                _gravityTracing.DrawMathOrbit(orbit, 20);
            }
            else
            {
                contentPanel.SetActive(false);
                nullPanel.SetActive(true);
                _gravityTracing.DrawMathOrbit(null, 0);
            }
        }
    }
}