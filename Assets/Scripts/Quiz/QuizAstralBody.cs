using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MathPlus;
using Quiz;
using UnityEngine;

public class QuizAstralBody : AstralBody
{
    public bool  isMassPublic;
    public bool  isVelocityPublic;
    public bool  isAngularVelocityPublic;
    public bool  isGravityPublic;
    public bool  isSizePublic;
    public float globalAngularVelocity;

    private float _oriRadius;
    private float _curRadius;


    public float _period;
    public float period
    {
        get => _period;
        set => _period = value;
    }
    public  bool  isPeriodPublic;

    public float _radius;
    public float radius
    {
        get => _radius;
        set => _radius = value;
    }
    public  bool  isRadiusPublic;


    public float _anglePerT;
    public float anglePerT
    {
        get => _anglePerT;
        set => _anglePerT = value;
    }
    public  bool  isAnglePerTPublic;

    public float _distancePerT;
    public float distancePerT
    {
        get => _distancePerT;
        set => _distancePerT = value;
    }
    public  bool  isDistancePerTPublic;

    public float _t;
    public float t
    {
        get => _t;
        set => _t = value;
    }

    public float oriRadius
    {
        get => _oriRadius;
        set => _oriRadius = value;
    }

    public  bool  isTPublic;

    //TODO 二级运算不存储，开放尺寸和中立加速度作为条件
    public void UpdateQuizAstralBody()
    {
        if (GameManager.GetGameManager.quizBase.target != this)
        {
            this.radius = Vector3.Distance(this.transform.position, GameManager.GetGameManager.quizBase.target.transform.position);
            ConicSection conicSection = GameManager.GetGameManager.orbit.GetConicSection(this, 1000);
            this.period = conicSection.GetT(GameManager.GetGameManager.quizBase.target.GetMass());
            //TODO t
            this.t                     = 2;
            this.globalAngularVelocity = this.period                    / 360f;
            this.anglePerT             = this.globalAngularVelocity / t;
            this.distancePerT          = this.GetVelocity().magnitude   * t;

        }
        else
        {
            this.radius       = 0;
            this.period       = 0;
            this.t            = 0;
            this.anglePerT    = 0;
            this.distancePerT = 0;
            this.globalAngularVelocity = 0;
        }
    }

    public void UpdateHighCost()
    {
        ConicSection conicSection = GameManager.GetGameManager.orbit.GetConicSection(this, 1000);
        this.period = conicSection.GetT(GameManager.GetGameManager.quizBase.target.GetMass());
        //TODO t
        this.t                     = 2;
        this.globalAngularVelocity = this.period                  / 360f;
        this.anglePerT             = this.globalAngularVelocity   / t;
        this.distancePerT          = this.GetVelocity().magnitude * t;

    }

    public void UpdateLowCost()
    {
        this.radius = Vector3.Distance(this.transform.position, GameManager.GetGameManager.quizBase.target.transform.position);

    }

    public void UpdateQuizAstralBodyPer()
    {
        this.globalAngularVelocity = this.period                  / 360f;
        this.anglePerT             = this.globalAngularVelocity   / t;
        this.distancePerT          = this.oriVelocity.magnitude * t;

    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateLowCost();
        if (GameManager.GetGameManager.isQuizEditMode) return;
        if (!GameManager.GetGameManager.quizBase.orbitBase.isFreezing && ((QuizSolver) (GameManager.GetGameManager.quizBase)).isRight)
        {
            _curRadius =  Vector3.Distance(this.transform.position, GameManager.GetGameManager.quizBase.target.transform.position);
            if (Mathf.Abs(_curRadius - oriRadius) >
                (((QuizSolver) (GameManager.GetGameManager.quizBase)).radiusOffset * oriRadius))
            {
                ((QuizSolver) (GameManager.GetGameManager.quizBase)).isRight = false;
                // Debug.Log("Test Result: Not Circle!");
                // Debug.Log("Test Result Cur Radius:" + _curRadius);

            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (ReferenceEquals(this, GameManager.GetGameManager.quizBase.target))
        {
            ((QuizSolver) (GameManager.GetGameManager.quizBase)).isRight = false;
            // Debug.Log("Test Result: Hit!");
        }
    }

    public String GetQuizConditionString()
    {
        StringBuilder stringBuilder = new StringBuilder("");
        if(isMassPublic){
            stringBuilder.Append("质量是" + this.mass.ToString("f2") + "kg，");
        }
        if(isRadiusPublic){
            stringBuilder.Append("轨道半径是" + this.radius.ToString("f2") + "m，");
        }
        if(isVelocityPublic){
            stringBuilder.Append("速度是" + this.oriVelocity.magnitude.ToString("f2") + "m/s，");
        }

        if(isAngularVelocityPublic){
            stringBuilder.Append("角速度是" + (this.globalAngularVelocity * Mathf.Deg2Rad).ToString("f2") + "rad/s，");
        }
        if(isPeriodPublic){
            stringBuilder.Append("周期是" + this.period.ToString("f2") + "s，");
        }
        if(isGravityPublic){
            stringBuilder.Append("表面重力加速度是" + this.gravity.ToString("f2") + "m/s²，");
        }
        if(isSizePublic){
            stringBuilder.Append("星球半径是" + this.size.ToString("f2") + "m，");
        }

        if(isTPublic){
            stringBuilder.Append("在" + this.t.ToString("f2") + "s内,其可");
            if(isAnglePerTPublic){
                stringBuilder.Append("绕轨道圆心旋转" + this.anglePerT.ToString("f2") + "度，");
            }
            if(isDistancePerTPublic){
                stringBuilder.Append("沿着轨道运行" + this.distancePerT.ToString("f2") +"m，");
            }

        }


        stringBuilder.Remove(stringBuilder.Length - 1, 1);
        return stringBuilder.ToString();
    }

    public bool CheckPublicity()
    {
        // if (!isRadiusPublic) return false;
        int a = isAngularVelocityPublic ? 1 : 0;
        int b = isPeriodPublic ? 1 : 0;
        int c = isRadiusPublic ? 1 : 0;
        int d = isVelocityPublic ? 1 : 0;
        if (a + b + c + d > 1) return true;
        return false;
    }
    
    
}
