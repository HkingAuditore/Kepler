using System;
using GameManagers;
using SpacePhysic;
using StaticClasses;
using StaticClasses.MathPlus;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI.AstralBodyEditor
{
    public class OrbitPanelUI : MonoBehaviour
    {
        public  AstralBody     _astralBody;
        public  Text           angle;
        public  GameObject     contentPanel;
        public  Text           eccentricity;
        public  Text           focalLength;
        public  Text           geoCenter;
        public  bool           isConicSection;
        public  Text           k;
        public  Text           majorAxis;
        public  Text           minorAxis;
        public  GameObject     nullPanel;
        public  OrbitGraphUI   orbitGraphUI;
        public  Text           period;
        private GravityTracing _gravityTracing;
        private ConicSection   _orbit;

        public AstralBody astralBody
        {
            get => _astralBody;
            set => _astralBody = value;
        }

        private void Awake()
        {
            _gravityTracing = GameManager.getGameManager.orbit;
        }

        private void FixedUpdate()
        {
            Init();
        }

        private void OnEnable()
        {
            Init();
        }


        private void Init()
        {
            // Debug.Log(_gravityTracing);
            // Debug.Log(astralBody);

            try
            {
                _orbit         = _gravityTracing.GetConicSection(astralBody);
                isConicSection = true;
            }
            catch (Exception e)
            {
                isConicSection = false;
            }


            if (isConicSection && !float.IsNaN(_orbit.semiMajorAxis) && !float.IsNaN(_orbit.semiMinorAxis))
            {
                contentPanel.SetActive(true);
                nullPanel.SetActive(false);
                majorAxis.text = "长轴:" + _orbit.semiMajorAxis.GetMantissa().ToSuperscript(2,1 + GameManager.getGameManager.GetK(PropertyUnit.M)) + " m";
                minorAxis.text = "短轴:" + _orbit.semiMinorAxis.GetMantissa().ToSuperscript(2,1 + GameManager.getGameManager.GetK(PropertyUnit.M)) + " m";
                geoCenter.text = "几何中心: ("                         + _orbit.geoCenter.x.ToString("f2") + ", " +
                                 _orbit.geoCenter.y.ToString("f2") +
                                 " )";
                eccentricity.text = "离心率: " + _orbit.eccentricity.ToString("f2");
                focalLength.text  = "焦距: "  + _orbit.focalLength.GetMantissa().ToSuperscript(2,1 + GameManager.getGameManager.GetK(PropertyUnit.M))                              + " m";
                period.text       = "周期: "  + _orbit.GetT(astralBody.affectedPlanets[0].Mass).GetMantissa().ToSuperscript(2,GameManager.getGameManager.GetK(PropertyUnit.S)) + " s";
                angle.text        = "倾角: "  + _orbit.angle.ToString("f2")                                    + " °";
                k.text = "T²/a³ :" + _orbit.GetT(astralBody.affectedPlanets[0].Mass) *
                    _orbit.GetT(astralBody.affectedPlanets[0].Mass) /
                    (_orbit.semiMajorAxis * _orbit.semiMajorAxis * _orbit.semiMajorAxis);
                orbitGraphUI.astralBody = astralBody;
                orbitGraphUI.orbit      = _orbit;
                orbitGraphUI.gameObject.SetActive(true);
                orbitGraphUI.Init();
                _gravityTracing.DrawMathOrbit(_orbit, 20);
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