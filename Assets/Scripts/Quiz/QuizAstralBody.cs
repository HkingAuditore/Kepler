using System;
using System.Text;
using GameManagers;
using SpacePhysic;
using StaticClasses;
using StaticClasses.MathPlus;
using UnityEngine;

namespace Quiz
{
    public class QuizAstralBody : AstralBody
    {
        /// <summary>
        ///     角速度
        /// </summary>
        public float globalAngularVelocity;

        public bool isAnglePerTPublic;
        public bool isAngularVelocityPublic;
        public bool isDistancePerTPublic;

        /// <summary>
        ///     表面重力酒速达是否为条件
        /// </summary>
        public bool isGravityPublic;

        /// <summary>
        ///     质量是否为条件
        /// </summary>
        public bool isMassPublic;

        /// <summary>
        ///     周期是否为条件
        /// </summary>
        public bool isPeriodPublic;

        /// <summary>
        ///     与中心天体距离是否为条件
        /// </summary>
        public bool isRadiusPublic;

        /// <summary>
        ///     星球半径是否为条件
        /// </summary>
        public bool isSizePublic;

        public bool isTPublic;

        /// <summary>
        ///     速度是否为条件
        /// </summary>
        public bool isVelocityPublic;

        private                  float      _curRadius;
        private                  GameObject _line;
        [SerializeField] private float      _radius;
        [SerializeField]                 private float      _period;

        /// <summary>
        ///     周期
        /// </summary>
        public float period
        {
            get => _period;
            set => _period = value;
        }

        /// <summary>
        ///     与中心天体距离
        /// </summary>
        public float radius
        {
            get => _radius;
            set
            {
                // Debug.Log("tmp radius set:" + value);
                _radius = value;
            }
        }

        private float anglePerT { get; set; }

        private float distancePerT { get; set; }

        public float t { get; set; }

        public float oriRadius { get; set; }



        protected override void Start()
        {
            Generate();
            _line = transform.GetChild(0).gameObject;
            if (GameManager.getGameManager.quizBase.GetType() == typeof(QuizSolver))
            {
                var solver = (QuizSolver) GameManager.getGameManager.quizBase;
                _line.SetActive(false);
                solver.answerEvent.AddListener(() => _line.SetActive(true));
            }

            isLoadDone = true;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateQuizAstralBody();
            if (GameManager.getGameManager.isQuizEditMode) return;
            if (!GameManager.getGameManager.quizBase.orbitBase.isFreezing &&
                ((QuizSolver) GameManager.getGameManager.quizBase).isRight)
            {
                _curRadius = Vector3.Distance(transform.position,
                                              GameManager.getGameManager.quizBase.target.transform.position);
                if (Mathf.Abs(_curRadius - oriRadius) >
                    ((QuizSolver) GameManager.getGameManager.quizBase).radiusOffset * oriRadius)
                {
                    ((QuizSolver) GameManager.getGameManager.quizBase).isRight = false;
                    ((QuizSolver) GameManager.getGameManager.quizBase).reason  = Reason.NonCircleOrbit;
                    // Debug.Log("Test Result: Not Circle!");
                    // Debug.Log("Test Result Cur Radius:" + _curRadius);
                }
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (ReferenceEquals(this, GameManager.getGameManager.quizBase.target))
            {
                ((QuizSolver) GameManager.getGameManager.quizBase).isRight = false;
                ((QuizSolver) GameManager.getGameManager.quizBase).reason  = Reason.Crash;
                // Debug.Log("Test Result: Hit!");
            }
        }

        //TODO 二级运算不存储，开放尺寸和中立加速度作为条件
        /// <summary>
        ///     更新天体数据
        /// </summary>
        public void UpdateQuizAstralBody()
        {
            if (GameManager.getGameManager.quizBase.target != this)
            {
                radius = Vector3.Distance(transform.position,
                                          GameManager.getGameManager.quizBase.target.transform.position);
                var conicSection = GameManager.getGameManager.orbit.GetConicSection(this);
                period = conicSection.GetT(GameManager.getGameManager.quizBase.target.GetMass());
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

        /// <summary>
        ///     更新独立天体数据
        /// </summary>
        public void UpdateHighCost()
        {
            var conicSection = GameManager.getGameManager.orbit.GetConicSection(this);
            period = conicSection.GetT(GameManager.getGameManager.quizBase.target.GetMass());
            //TODO t
            t                     = 2;
            globalAngularVelocity = period                  / 360f;
            anglePerT             = globalAngularVelocity   / t;
            distancePerT          = GetVelocity().magnitude * t;
        }

        private void UpdateLowCost()
        {
            radius = Vector3.Distance(transform.position,
                                      GameManager.getGameManager.quizBase.target.transform.position);

        }

        public void UpdateQuizAstralBodyPer()
        {
            globalAngularVelocity = period                / 360f;
            anglePerT             = globalAngularVelocity / t;
            distancePerT          = oriVelocity.magnitude * t;
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
            var f    = num.GetMantissa();
            var e    = num.GetExponent() + baseE;
            var eStr = e.ToString().ToSuperscript();
            return f.ToString("f3") + "x10" + eStr;
        }

        /// <summary>
        ///     获取星球条件文本
        /// </summary>
        /// <returns></returns>
        public string GetQuizConditionString()
        {
            var stringBuilder = new StringBuilder("");
            if (isMassPublic)
                stringBuilder.Append("质量是"                                                                          +
                                     GetScientificFloat(realMass, GameManager.getGameManager.GetK(PropertyUnit.Kg)) +
                                     "kg，");
            if (isRadiusPublic)
                stringBuilder.Append("轨道半径是"                                                                         +
                                     GetScientificFloat(radius, 1 + GameManager.getGameManager.GetK(PropertyUnit.M)) +
                                     "m，");
            if (isVelocityPublic) stringBuilder.Append("速度是" + GetScientificFloat(GetVelocity().magnitude, 3) + "m/s，");
            Debug.Log(this.name + ":"+GetVelocity());
            if (isAngularVelocityPublic)
                stringBuilder.Append("角速度是" + GetScientificFloat(globalAngularVelocity * Mathf.Deg2Rad, -4) + "rad/s，");
            if (isPeriodPublic)
                stringBuilder.Append(
                                     "周期是"                                                                       +
                                     GetScientificFloat(period, GameManager.getGameManager.GetK(PropertyUnit.S)) +
                                     "s，");
            if (isGravityPublic) stringBuilder.Append("表面重力加速度是" + GetScientificFloat(gravity, 0) + "m/s²，");
            if (isSizePublic)
                stringBuilder.Append("半径是" + GetScientificFloat(size, GameManager.getGameManager.GetK(PropertyUnit.M)) +
                                     "m，");

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
}