using System;
using System.Collections;
using System.Collections.Generic;
using Quiz;
using UnityEngine;

public class QuizAstralBody : AstralBody
{
    public bool isMassPublic;
    public bool isVelocityPublic;
    public bool isAngularVelocityPublic;

    private float _oriRadius;


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
    public  bool  isTPublic;

    public void UpdateQuizAstralBody()
    {
        this.radius = Vector3.Distance(this.transform.position, GameManager.GetGameManager.quizBase.target.transform.position);
    }

    protected override void Start()
    {
        base.Start();
        _oriRadius = Vector3.Distance(this.transform.position, GameManager.GetGameManager.quizBase.target.transform.position);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (ReferenceEquals(this, GameManager.GetGameManager.quizBase.target))
        {
            ((QuizSolver) (GameManager.GetGameManager.quizBase)).isRight = false;
        }
    }
}
