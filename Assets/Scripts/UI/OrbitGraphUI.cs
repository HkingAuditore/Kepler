﻿using MathPlus;
using SpacePhysic;
using UnityEngine;
using UnityEngine.UI;

public class OrbitGraphUI : MonoBehaviour
{
    public Vector3   angularMomentum
    {
        get
        {
            Vector3 vm = astralBody.GetVelocity() * astralBody.Mass;
            Vector3 r  = astralBody.transform.position - astralBody.affectedPlanets[0].transform.position;
            return Vector3.Cross(r, vm);
        }
    }

    public AstralBody astralBody;
    public float      conHeight;
    public Text       angularVelocity;
    public Text       distance;
    public Image      ellipseImage;
    public Image      oriImage;
    public Image      targetImage;

    [Header("Fill")] public RectTransform fillMask;
    public                  Image         fillImage;


    private float _angularVelocity;

    private float         _curAngle;
    private RectTransform _ellipseImageRect;


    private GraphOrbit    _graphOrbit;
    private float         _lastAngle;
    private RectTransform _oriImageRect;
    private RectTransform _targetImageRect;
    private RectTransform _fillImageRect;
    public  ConicSection  orbit;


    private void Awake()
    {
        _ellipseImageRect = ellipseImage.gameObject.GetComponent<RectTransform>();
        _oriImageRect     = oriImage.gameObject.GetComponent<RectTransform>();
        _targetImageRect  = targetImage.gameObject.GetComponent<RectTransform>();
        _fillImageRect    = fillImage.gameObject.GetComponent<RectTransform>();
    }

    private void FixedUpdate()
    {
        PosOri();
        CalculateAngularVelocity();
        ShowAngularVelocity();
        ShowDistance();
    }

    private void OnEnable()
    {
    }

    public void Init()
    {
        _graphOrbit = new GraphOrbit(orbit, conHeight);
        PosOri();
        _lastAngle                  = _curAngle;
        _ellipseImageRect.sizeDelta = new Vector2(2 * _graphOrbit.a, 2 * _graphOrbit.b);
        fillMask.sizeDelta          = _ellipseImageRect.sizeDelta;

        // PosOri();
        PosTar();
    }

    private float GetOriAngle()
    {
        var tar2Ori = astralBody.transform.position - astralBody.affectedPlanets[0].transform.position;
        // var axis = astralBody.affectedPlanets[0].transform.position + new Vector3(
        //                                                                           1 * Mathf.Cos(orbit.angle *
        //                                                                                         Mathf.Deg2Rad),
        //                                                                           0,
        //                                                                           1 * Mathf.Sin(orbit.angle *
        //                                                                                         Mathf
        //                                                                                            .Deg2Rad));
        var axis = astralBody.affectedPlanets[0].transform.position - new Vector3(orbit.geoCenter.x,0,orbit.geoCenter.y);
        // Debug.DrawLine(astralBody.AffectedPlanets[0].transform.position,
        //                astralBody.AffectedPlanets[0].transform.position + tar2Ori,
        //                Color.magenta,
        //                1000f
        //               );
        // Debug.DrawLine(astralBody.AffectedPlanets[0].transform.position,
        //                astralBody.AffectedPlanets[0].transform.position + new Vector3(
        //                                                                               1 * Mathf.Cos(orbit.angle *
        //                                                                                             Mathf.Deg2Rad),
        //                                                                               0,
        //                                                                               1 * Mathf.Sin(orbit.angle *
        //                                                                                             Mathf
        //                                                                                                .Deg2Rad)) * 2000,
        //                Color.cyan,
        //                1000f
        //               );

        var angle  = Vector3.Angle(axis, tar2Ori);
        var normal = Vector3.Cross(axis, tar2Ori);                          //叉乘求出法线向量
        angle     *= Mathf.Sign(Vector3.Dot(normal, new Vector3(0, 1, 0))); //求法线向量与物体上方向向量点乘，结果为1或-1，修正旋转方向
        _curAngle =  angle;
        // Debug.Log(angle);
        // return (orbit.semiMajorAxis * (1 - orbit.eccentricity * orbit.eccentricity))/Vector3.Distance(astralBody.transform.position,
                                                                                                      // astralBody.affectedPlanets[0].transform.position);
        return angle;
    }


    private void PosOri()
    {
        _oriImageRect.anchoredPosition = _graphOrbit.GetPolarPos(GetOriAngle());

        // Debug.Log(oriImage.rectTransform.anchoredPosition);
    }

    private void PosTar()
    {
        var lSizePos = new Vector3(orbit.geoCenter.x, 0, orbit.geoCenter.y) + new Vector3(
                                                                                          1 * Mathf
                                                                                             .Cos(orbit.angle *
                                                                                                  Mathf.Deg2Rad),
                                                                                          0,
                                                                                          1 * Mathf
                                                                                             .Sin(orbit.angle *
                                                                                                  Mathf
                                                                                                     .Deg2Rad)) *
            orbit.semiMajorAxis;
        var rSizePos = new Vector3(orbit.geoCenter.x, 0, orbit.geoCenter.y) - new Vector3(
                                                                                          1 * Mathf
                                                                                             .Cos(orbit.angle *
                                                                                                  Mathf.Deg2Rad),
                                                                                          0,
                                                                                          1 * Mathf
                                                                                             .Sin(orbit.angle *
                                                                                                  Mathf
                                                                                                     .Deg2Rad)) *
            orbit.semiMajorAxis;
        var orientation = -Mathf.Sign(
                                      Vector3.Distance(astralBody.affectedPlanets[0].transform.position, lSizePos)
                                     -
                                      Vector3.Distance(astralBody.affectedPlanets[0].transform.position, rSizePos)
                                     );
        _targetImageRect.anchoredPosition = new Vector2(_graphOrbit.c * orientation, 0);

        
        //Fill
        _fillImageRect.anchoredPosition = _targetImageRect.anchoredPosition;
        var direction = _fillImageRect.position - _oriImageRect.transform.position;
        _fillImageRect.transform.up = direction;
        float r = Vector3.Distance(astralBody.transform.position,
                                   astralBody.affectedPlanets[0].transform.position);
        float s          = GetDADT()  * 30000;
        float es         = Mathf.PI   * orbit.semiMajorAxis * orbit.semiMinorAxis;
        float fillAmount = s          / (r * r * Mathf.PI);
        float angle      = fillAmount * 360;
        
        var oriR = this.GetDistanceToFociByAngle(_curAngle);
        var tarR = this.GetDistanceToFociByAngle(_curAngle + angle);
        fillAmount           *= (oriR / tarR);
        // this.tr              =  tarR;
        // this.or              =  oriR;
        fillAmount           =  Mathf.Min(fillAmount, (s / es) / (oriR / (orbit.semiMajorAxis + orbit.focalLength/2)));
        fillImage.fillAmount =  fillAmount;
        // this.r               = r;
    }

    private float GetDADT()
    {
        return (this.angularMomentum.magnitude / astralBody.Mass)/ (2 * PhysicBase.GetG() * astralBody.affectedPlanets[0].Mass );
    }

    private float GetDistanceToFociByAngle(float angle)
    {
        return (orbit.semiMajorAxis * (1 - orbit.eccentricity * orbit.eccentricity)) / (1 + orbit.eccentricity * Mathf.Cos(angle*Mathf.Deg2Rad));
    }

    private float GetDistance()
    {
        return Vector3.Distance(_oriImageRect.position, _targetImageRect.position);
    }

    // public float am;
    // public float tr;
    // public float or;

    
    private void CalculateAngularVelocity()
    {
        _angularVelocity = Mathf.Abs(_curAngle - _lastAngle) / Time.fixedDeltaTime;
        _lastAngle       = _curAngle;
    }

    private void ShowAngularVelocity()
    {
        angularVelocity.text = "角速度:" + _angularVelocity.ToString("f2") + " degree/s";
    }

    private void ShowDistance()
    {
        distance.text = "距离:" +
                        Vector3.Distance(astralBody.transform.position,
                                         astralBody.affectedPlanets[0].transform.position).ToString("f2") + " m";
    }

    private class GraphOrbit
    {
        public readonly float a;
        public readonly float b;
        public readonly float c;
        public readonly float e;

        public GraphOrbit(ConicSection conicSection, float height)
        {
            var adb = conicSection.semiMajorAxis / conicSection.semiMinorAxis;
            b = height     / 2;
            a = height / 2 * adb;
            if (a > 404f)
            {
                a = 404f;
                b = a / adb;
            }

            c = conicSection.eccentricity * a;
            e = c                         / a;
        }

        public float GetR(float ag)
        {
            return b / Mathf.Sqrt(1 - e * e * Mathf.Cos(ag) * Mathf.Cos(ag));
        }

        public Vector2 GetPolarPos(float ag)
        {
            // var r = b / Mathf.Sqrt(1 - e * e * Mathf.Cos(ag * Mathf.Deg2Rad) *
            //                        Mathf.Cos(ag * Mathf.Deg2Rad));
            // Debug.Log("r: " + r);
            // var sumAngle = ag;
            // var pos = new Vector2(r * Mathf.Cos(sumAngle * Mathf.Deg2Rad),
            //                       r * Mathf.Sin(sumAngle * Mathf.Deg2Rad));
            var pos = new Vector2(a * Mathf.Cos(ag * Mathf.Deg2Rad),
                                  b * Mathf.Sin(ag * Mathf.Deg2Rad));
            // Debug.Log(geoCenter + pos);
            // Debug.Log("pos" + pos);
            return new Vector2(0, 0) + pos;
        }
    }
}