using System;
using System.Collections;
using System.Collections.Generic;
using MathPlus;
using UnityEngine;
using UnityEngine.UI;

public class OrbitPanelUI : MonoBehaviour
{
    public ConicSection orbit;
    public AstralBody astralBody;
    public Text majorAxis;
    public Text minorAxis;
    public Text geoCenter;
    public Text eccentricity;
    public Text focalLength;
    public Text period;
    public OrbitGraphUI orbitGraphUI;

    private void OnEnable()
    {
        
    }

    public void Init()
    {
        majorAxis.text = "长轴:" + orbit.semiMajorAxis.ToString("f2") + " m";
        minorAxis.text = "短轴:" + orbit.semiMinorAxis.ToString("f2") + " m";
        geoCenter.text = "几何中心: (" + orbit.geoCenter.x.ToString("f2") +", " +orbit.geoCenter.y.ToString("f2") + " )";
        eccentricity.text = "离心率: " + orbit.eccentricity.ToString("f2");
        focalLength.text = "焦距: " + orbit.focalLength.ToString("f2") + " m";
        period.text = "周期: " + orbit.GetT(astralBody.AffectedPlanets[0].mass).ToString("f2") + " s";
        orbitGraphUI.astralBody = astralBody;
        orbitGraphUI.orbit = orbit;
        orbitGraphUI.gameObject.SetActive(true);
        orbitGraphUI.Init();
    }
}
