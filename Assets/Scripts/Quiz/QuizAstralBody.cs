using System;
using System.Text;
using MathPlus;
using Quiz;
using StaticClasses;
using UnityEngine;

public class QuizAstralBody : AstralBody
{
    public bool  isMassPublic;
    public bool  isVelocityPublic;
    public bool  isAngularVelocityPublic;
    public bool  isGravityPublic;
    public bool  isSizePublic;
    public float globalAngularVelocity;

    private float      _curRadius;
    private GameObject _line;


    public float _period;

    public float period
    {
        get => _period;
        set => _period = value;
    }

    public bool isPeriodPublic;

    public float _radius;

    public float radius
    {
        get => _radius;
        set => _radius = value;
    }

    public bool isRadiusPublic;


    public float _anglePerT;

    public float anglePerT
    {
        get => _anglePerT;
        set => _anglePerT = value;
    }

    public bool isAnglePerTPublic;

    public float _distancePerT;

    public float distancePerT
    {
        get => _distancePerT;
        set => _distancePerT = value;
    }

    public bool isDistancePerTPublic;

    public float _t;

    public float t
    {
        get => _t;
        set => _t = value;
    }

    public float oriRadius { get; set; }

    public bool isTPublic;

    //TODO 二级运算不存储，开放尺寸和中立加速度作为条件
    public void UpdateQuizAstralBody()
    {
        if (GameManager.GetGameManager.quizBase.target != this)
        {
            radius = Vector3.Distance(transform.position,
                                      GameManager.GetGameManager.quizBase.target.transform.position);
            var conicSection = GameManager.GetGameManager.orbit.GetConicSection(this);
            period = conicSection.GetT(GameManager.GetGameManager.quizBase.target.GetMass());
            //TODO t
            t                     = 2;
            globalAngularVelocity = period                  / 360f;
            anglePerT             = globalAngularVelocity   / t;
            distancePerT          = GetVelocity().magnitude * t;
        }
        else
        {
            radius                = 0;
            period                = 0;
            t                     = 0;
            anglePerT             = 0;
            distancePerT          = 0;
            globalAngularVelocity = 0;
        }
    }

    public void UpdateHighCost()
    {
        var conicSection = GameManager.GetGameManager.orbit.GetConicSection(this);
        period = conicSection.GetT(GameManager.GetGameManager.quizBase.target.GetMass());
        //TODO t
        t                     = 2;
        globalAngularVelocity = period                  / 360f;
        anglePerT             = globalAngularVelocity   / t;
        distancePerT          = GetVelocity().magnitude * t;
    }

    public void UpdateLowCost()
    {
        radius = Vector3.Distance(transform.position, GameManager.GetGameManager.quizBase.target.transform.position);
    }

    public void UpdateQuizAstralBodyPer()
    {
        globalAngularVelocity = period                / 360f;
        anglePerT             = globalAngularVelocity / t;
        distancePerT          = oriVelocity.magnitude * t;
    }

    protected override void Start()
    {
        base.Start();
        _line = transform.GetChild(0).gameObject;
        try
        {
            var solver = (QuizSolver) GameManager.GetGameManager.quizBase;
            _line.SetActive(false);
            solver.answerEvent.AddListener(() => _line.SetActive(true));
        }
        catch (Exception e)
        {
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateLowCost();
        if (GameManager.GetGameManager.isQuizEditMode) return;
        if (!GameManager.GetGameManager.quizBase.orbitBase.isFreezing &&
            ((QuizSolver) GameManager.GetGameManager.quizBase).isRight)
        {
            _curRadius = Vector3.Distance(transform.position,
                                          GameManager.GetGameManager.quizBase.target.transform.position);
            if (Mathf.Abs(_curRadius - oriRadius) >
                ((QuizSolver) GameManager.GetGameManager.quizBase).radiusOffset * oriRadius)
            {
                ((QuizSolver) GameManager.GetGameManager.quizBase).isRight = false;
                ((QuizSolver) GameManager.GetGameManager.quizBase).reason  = Reason.NonCircleOrbit;
                // Debug.Log("Test Result: Not Circle!");
                // Debug.Log("Test Result Cur Radius:" + _curRadius);
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (ReferenceEquals(this, GameManager.GetGameManager.quizBase.target))
        {
            ((QuizSolver) GameManager.GetGameManager.quizBase).isRight = false;
            ((QuizSolver) GameManager.GetGameManager.quizBase).reason  = Reason.Crash;
            // Debug.Log("Test Result: Hit!");
        }
    }

    /*
        \u2070   >>   0
        \u00B9   >>   1
        \u00B2   >>   2
        \u00B3   >>   3
        \u2074   >>   4
        \u2075   >>   5
        \u2076   >>   6
        \u2077   >>   7
        \u2078   >>   8
        \u2079   >>   9
        \u207A   >>   +
        \u207B   >>   -
        \u207C   >>   =
        \u207D   >>   (
        \u207E   >>   )
     */

    private string GetScientificFloat(double num, int baseE)
    {
        var    f    = num.GetMantissa();
        var    e    = num.GetExponent() + baseE;
        string eStr = e.ToString().ToSuperscript();
        return f.ToString("f3") + "x10" + eStr;
    }

    public string GetQuizConditionString()
    {
        var stringBuilder = new StringBuilder("");
        if (isMassPublic)
            stringBuilder.Append("质量是"                                                                          +
                                 GetScientificFloat(realMass, GameManager.GetGameManager.GetK(PropertyUnit.Kg)) +
                                 "kg，");
        if (isRadiusPublic)
            stringBuilder.Append("轨道半径是"                                                                         +
                                 GetScientificFloat(radius, 1 + GameManager.GetGameManager.GetK(PropertyUnit.M)) +
                                 "m，");
        if (isVelocityPublic) stringBuilder.Append("速度是" + GetScientificFloat(this.GetVelocity().magnitude, 3) + "m/s，");

        if (isAngularVelocityPublic)
            stringBuilder.Append("角速度是" +GetScientificFloat((globalAngularVelocity * Mathf.Deg2Rad), -4) + "rad/s，");
        if (isPeriodPublic) stringBuilder.Append("周期是"       + GetScientificFloat(period, GameManager.GetGameManager.GetK(PropertyUnit.S)) + "s，");
        if (isGravityPublic) stringBuilder.Append("表面重力加速度是" + GetScientificFloat(this.gravity, 0)                     + "m/s²，");
        if (isSizePublic) stringBuilder.Append("半径是"         +  GetScientificFloat(this.size, GameManager.GetGameManager.GetK(PropertyUnit.M))                          + "m，");

        if (isTPublic)
        {
            stringBuilder.Append("在" + t.ToString("f2") + "s内,其可");
            if (isAnglePerTPublic) stringBuilder.Append("绕轨道圆心旋转"   + anglePerT.ToString("f2")    + "度，");
            if (isDistancePerTPublic) stringBuilder.Append("沿着轨道运行" + distancePerT.ToString("f2") + "m，");
        }

        if (stringBuilder.Length == 0) return null;
        stringBuilder.Remove(stringBuilder.Length - 1, 1);
        return stringBuilder.ToString();
    }

    public bool CheckPublicity()
    {
        // if (!isRadiusPublic) return false;
        var a = isAngularVelocityPublic ? 1 : 0;
        var b = isPeriodPublic ? 1 : 0;
        var c = isRadiusPublic ? 1 : 0;
        var d = isVelocityPublic ? 1 : 0;
        if (a + b + c + d > 1) return true;
        return false;
    }
}