using System;
using System.Collections;
using System.Collections.Generic;
using MathPlus;
using Quiz;
using UnityEngine;

public class QuizAstralBody : AstralBody
{
    public bool isMassPublic;
    public bool isVelocityPublic;
    public bool isAngularVelocityPublic;

    private float _oriRadius;
    private float _curRadius;


    private float _period;
    public float period
    {
        get => _period;
        private set => _period = value;
    }
    public  bool  isPeriodPublic;
    
    private float _radius;
    public float radius
    {
        get => _radius;
        private set => _radius = value;
    }
    public  bool  isRadiusPublic;
    

    private float _anglePerT;
    public float anglePerT
    {
        get => _anglePerT;
        set => _anglePerT = value;
    }
    public  bool  isAnglePerTPublic;

    private float _distancePerT;
    public float distancePerT
    {
        get => _distancePerT;
        set => _distancePerT = value;
    }
    public  bool  isDistancePublic;

    private float _t;
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

    public void UpdateQuizAstralBody()
    {
        this.radius = Vector3.Distance(this.transform.position, GameManager.GetGameManager.quizBase.target.transform.position);
        ConicSection conicSection  = GameManager.GetGameManager.quizBase.orbitBase.GetConicSection(this, 1000);
        this.period = conicSection.GetT(GameManager.GetGameManager.quizBase.target.GetMass());
        //TODO t
        this.t            = 1;
        this.anglePerT    = this.angularVelocity.magnitude / t;
        this.distancePerT = this.GetVelocity().magnitude   * t;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!GameManager.GetGameManager.quizBase.orbitBase.isFreezing && ((QuizSolver) (GameManager.GetGameManager.quizBase)).isRight)
        {
            _curRadius =  Vector3.Distance(this.transform.position, GameManager.GetGameManager.quizBase.target.transform.position);
            if (Mathf.Abs(_curRadius - oriRadius) >
                (((QuizSolver) (GameManager.GetGameManager.quizBase)).radiusOffset * oriRadius))
            {
                ((QuizSolver) (GameManager.GetGameManager.quizBase)).isRight = false;
                Debug.Log("Test Result: Not Circle!");
                Debug.Log("Test Result Cur Radius:" + _curRadius);

            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (ReferenceEquals(this, GameManager.GetGameManager.quizBase.target))
        {
            ((QuizSolver) (GameManager.GetGameManager.quizBase)).isRight = false;
            Debug.Log("Test Result: Hit!");
        }
    }
}
