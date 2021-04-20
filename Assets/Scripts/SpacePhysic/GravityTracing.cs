using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dreamteck.Splines;
using MathPlus;
using UnityEngine;

namespace SpacePhysic
{
    public class GravityTracing : MonoBehaviour
    {
        /// <summary>
        ///     采样率
        /// </summary>
        public int sample = 20;

        /// <summary>
        ///     采样时间尺度
        /// </summary>
        public float timeScale = 100;

        public           SplineComputer   splineComputer;
        private readonly List<ActionType> _actionTypes  = new List<ActionType>();
        private readonly List<ITraceable> _astralBodies = new List<ITraceable>();

        //坐标点
        private readonly Dictionary<ITraceable, List<Vector3>> _orbitPoints =
            new Dictionary<ITraceable, List<Vector3>>();

        private readonly Dictionary<ITraceable, LineRenderer> _orbitRenderers =
            new Dictionary<ITraceable, LineRenderer>();

        private float  _deltaTime;
        private Thread _thread;

        /// <summary>
        ///     是否冻结星群
        /// </summary>
        public bool isFreezing { get; private set; }


        public void Awake()
        {
            _deltaTime = 100f / sample * Time.fixedDeltaTime * timeScale;
            foreach (ITraceable traceable in GetComponentsInChildren<AstralBody>()) AddTracingTarget(traceable);
            _thread = new Thread(Sample);
        }

        private void Update()
        {
            Dispatch();
        }

        private void OnDisable()
        {
            if (_thread != null) _thread.Abort();
        }

        private void Dispatch()
        {
            lock (((ICollection) _actionTypes).SyncRoot)
            {
                if (_actionTypes.Count > 0)
                {
                    HandleAction();
                    _actionTypes.RemoveAt(0);
                }
                else
                {
                    if (!_thread.ThreadState.Equals(ThreadState.Running)) TraceGravity();
                }
            }
        }

        private void HandleAction()
        {
            DrawOrbits();
        }

        private void StartNewThread(object[] dict)
        {
            _thread = new Thread(Sample);
            _thread.Start(dict);
        }

        /// <summary>
        ///     增加追踪实体
        /// </summary>
        /// <param name="traceable"></param>
        public void AddTracingTarget(ITraceable traceable)
        {
            // Debug.Log("Add HashCode:" + traceable.GetHashCode());
            //
            if (isFreezing)
            {
                traceable.GetRigidbody().isKinematic = true;
            }
            else
            {
                var tmpV = traceable.GetVelocity();
                traceable.GetRigidbody().velocity    = tmpV;
                traceable.GetRigidbody().isKinematic = false;
            }

            if (traceable.GetEnableTracing())
            {
                _astralBodies.Add(traceable);
                _orbitPoints[traceable] = new List<Vector3> {traceable.GetTransform().position};
                _orbitRenderers[traceable] =
                    traceable.GetTransform().Find("Line").gameObject.GetComponent<LineRenderer>();
            }
        }

        /// <summary>
        ///     冻结
        /// </summary>
        /// <param name="isFreezing">是否冻结</param>
        public void Freeze(bool isFreezing)
        {
            this.isFreezing = isFreezing;
            _astralBodies.ForEach(astral =>
                                  {
                                      if (isFreezing)
                                      {
                                          astral.GetRigidbody().isKinematic = true;
                                      }
                                      else
                                      {
                                          var tmpV = astral.GetVelocity();
                                          astral.GetRigidbody().velocity    = tmpV;
                                          astral.GetRigidbody().isKinematic = false;
                                      }
                                  });
        }

        private List<AstralBody> GetAstralBodyList()
        {
            var list = (from traceable in _astralBodies
                        select traceable.GetAstralBody()).ToList();
            return list;
        }

        /// <summary>
        ///     移除追踪实体
        /// </summary>
        /// <param name="astralBody"></param>
        public void RemoveAstralBody(AstralBody astralBody)
        {
            GetAstralBodyList().Remove(astralBody);
            _astralBodies.Remove(astralBody);
            GetAstralBodyList().ForEach(a => a.GetAffectedPlanets().Remove(astralBody));
        }

        #region 引力步进

        #region 引力计算

        //计算两个星体之间的引力
        private float CalculateGravityModulus(float originMass, float targetMass, float distance)
        {
            return PhysicBase.GetG() * (originMass * targetMass / (distance * distance));
        }

        private Vector3 GetGravityVector3(ITraceable a0, ITraceable a1, float a0Mass, float a1Mass, int sampleTime)
        {
            var distance            = Vector3.Distance(_orbitPoints[a0][sampleTime], _orbitPoints[a1][sampleTime]);
            var normalizedDirection = (_orbitPoints[a1][sampleTime] - _orbitPoints[a0][sampleTime]).normalized;
            // Debug.Log(_orbitPoints[a1][sampleTime]);
            // Debug.Log(_orbitPoints[a0][sampleTime]);
            // Debug.Log("count:" + _orbitPoints[a1].Count);
            // Debug.Log("a0Mass:" + a0Mass);
            // Debug.Log("a1Mass:" + a1Mass);
            // Debug.Log("distance:" + distance);
            // Debug.Log("normalizedDirection:" + normalizedDirection);
            // return PhysicBase.GetG() * (a0Mass * a1Mass / (distance * distance))* normalizedDirection;
            return CalculateGravityModulus(a0Mass, a1Mass, distance) * normalizedDirection;
        }

        private Vector3 CalculateForce(ITraceable astralBody, int sampleTime, Dictionary<ITraceable, float> massDict)
        {
            var result = new Vector3(0, 0, 0);
            //求合力
            foreach (ITraceable body in astralBody.GetAffectedPlanets())
            {
                if (body == astralBody) continue;
                result += GetGravityVector3(astralBody, body, massDict[astralBody], massDict[body], sampleTime);
            }

            // Debug.Log(astralBody.GetMass() + " force: " + result);
            return result;
        }

        #endregion

        //引力步进
        private void TraceGravity()
        {
            var astralBodyVelocities = new Dictionary<ITraceable, Vector3>();


            foreach (var astralBody in _astralBodies)
            {
                //起始速度
                // Debug.Log(astralBody.GetRigidbody().velocity);

                astralBodyVelocities[astralBody] = astralBody.GetVelocity();
                //起始点改为当前位置
                if (_orbitPoints.ContainsKey(astralBody)) _orbitPoints[astralBody].Clear();
                _orbitPoints[astralBody].Add(astralBody.GetPosition());
            }


            var astralBodyMasses = new Dictionary<ITraceable, float>();
            _astralBodies.ForEach(a => astralBodyMasses.Add(a, a.GetMass()));

            StartNewThread(new[]
                           {
                               astralBodyVelocities,
                               (object) astralBodyMasses
                           });
        }

        private void Sample(object objs)
        {
            // Debug.Log("Tracing!");
            var dicts                = (object[]) objs;
            var astralBodyVelocities = (Dictionary<ITraceable, Vector3>) dicts[0];
            var astralBodyMasses     = (Dictionary<ITraceable, float>) dicts[1];
            //开始采样
            for (var i = 0; i < sample; i++)
                //遍历星体
                foreach (var astralBody in _astralBodies)
                {
                    //加速度
                    /*
                 * F=ma
                 * delta v=at
                 * s = vt + 0.5a(t^2)
                 */
                    var acceleration = CalculateForce(astralBody, i, astralBodyMasses) / astralBodyMasses[astralBody];
                    // Debug.Log("deltatime: " + _deltaTime);
                    _orbitPoints[astralBody].Add(_orbitPoints[astralBody].Last() +
                                                 astralBodyVelocities[astralBody] * _deltaTime +
                                                 .5f * acceleration * Mathf.Pow(_deltaTime, 2));
                    //加速后速度
                    astralBodyVelocities[astralBody] += acceleration * _deltaTime;
                    //Debug.DrawLine(_orbitPoints[astralBody].Last(),_orbitPoints[astralBody].Last() + CalculateForce(astralBody,i));
                }

            _actionTypes.Add(ActionType.Finished);
            // Debug.Log(_actionTypes.Count);
        }

        private void DrawOrbit(ITraceable astralBody)
        {
            _orbitRenderers[astralBody].positionCount = sample;
            _orbitRenderers[astralBody].SetPositions(_orbitPoints[astralBody].ToArray());
        }

        /// <summary>
        ///     绘制轨道
        /// </summary>
        private void DrawOrbits()
        {
            // TraceGravity();
            foreach (var astralBody in _astralBodies) DrawOrbit(astralBody);
        }

        #endregion

        #region 求解圆锥曲线

        private Vector2 ConvertV3ToV2(Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }

        private Vector3 ConvertV2ToV3(Vector2 vector2)
        {
            return new Vector3(vector2.x, 0, vector2.y);
        }

        /// <summary>
        ///     获取轨道圆锥曲线
        /// </summary>
        /// <param name="astralBody"></param>
        /// <returns></returns>
        public ConicSection GetConicSection(ITraceable astralBody)
        {
            // var sampleStep = sample / sampleCount;
            //
            // // Debug.Log("[0]:"+ConvertV3ToV2(_orbitPoints[astralBody][0]));
            // // Debug.Log("[1]:"+ConvertV3ToV2(_orbitPoints[astralBody][sampleStep]));
            // // Debug.Log("[2]:"+ConvertV3ToV2(_orbitPoints[astralBody][2 * sampleStep]));
            // // Debug.Log("[3]:"+ConvertV3ToV2(_orbitPoints[astralBody][3 * sampleStep]));
            // // Debug.Log("[4]:"+ConvertV3ToV2(_orbitPoints[astralBody][4 * sampleStep]));
            // // Debug.Log("[5]:"+ConvertV3ToV2(_orbitPoints[astralBody][5 * sampleStep]));
            // // Debug.Log("Now HashCode:" + astralBody.GetHashCode());
            // var points = new List<Vector2>();
            // for (var i = 0; i < sampleCount; i++) points.Add(ConvertV3ToV2(_orbitPoints[astralBody][i * sampleStep]));
            //
            // var conicSection =
            //     CustomSolver.FitConicSection(points);

            var conicSection =
                CustomSolver
                   .CalculateOrbit(new Vector2(astralBody.GetTransform().position.x, astralBody.GetTransform().position.z),
                                   new Vector2(astralBody.GetAffectedPlanets()[0].transform.position.x,
                                               astralBody.GetAffectedPlanets()[0].transform.position.z),
                                   new Vector2(astralBody.GetVelocity().x, astralBody.GetVelocity().z),
                                   astralBody.GetMass(),
                                   astralBody.GetAffectedPlanets()[0].Mass);

            // Debug.Log("new section:" + "semi major = " + newConicSection.semiMajorAxis);
            // Debug.Log("new section:" + "semi minor = " + newConicSection.semiMinorAxis);
            // Debug.Log("new section:" + "eccentricity = " + newConicSection.eccentricity);
            // Debug.Log("new section:" + "geo center = " + newConicSection.geoCenter);
            // Debug.Log("new section:" + "T = " + newConicSection.GetT(astralBody.GetAffectedPlanets()[0].mass));
            // Debug.Log("new section:" + "angle = " + conicSection.angle);


            return conicSection;
        }

        /// <summary>
        ///     绘制轨道
        /// </summary>
        /// <param name="conicSection"></param>
        /// <param name="sam"></param>
        public void DrawMathOrbit(ConicSection conicSection, int sam)
        {
            if (conicSection != null)
            {
                var points = new List<SplinePoint>();
                var step   = 360f / sam;
                for (var i = 0; i < sam; i++)
                    points.Add(new SplinePoint(ConvertV2ToV3(conicSection.GetPolarPos(i * step))));
                points.Add(new SplinePoint(ConvertV2ToV3(conicSection.GetPolarPos(360))));

                splineComputer.SetPoints(points.ToArray());
                splineComputer.Close();
            }
            else
            {
                splineComputer.SetPoints(new[]
                                         {
                                             new SplinePoint(new Vector3(0, 0, 0)),
                                             new SplinePoint(new Vector3(0, 0, 0)),
                                             new SplinePoint(new Vector3(0, 0, 0)),
                                             new SplinePoint(new Vector3(0, 0, 0))
                                         });
                splineComputer.Close();
            }

            // mathOrbitDrawer.positionCount = sam + 1;
            // mathOrbitDrawer.SetPositions(points.ToArray());
        }

        #endregion
    }
}