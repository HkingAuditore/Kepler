using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dreamteck.Splines;
using MathPlus;
using UnityEngine;

namespace SpacePhysic
{
    public interface ITraceable
    {
        Transform        GetTransform();
        Vector3          GetPosition();
        GameObject       GetGameObject();
        bool             GetEnableTracing();
        float            GetMass();
        Rigidbody        GetRigidbody();
        Vector3          GetVelocity();
        List<AstralBody> GetAffectedPlanets();

        AstralBody GetAstralBody();
    }

    enum ActionType
    {
        Finished
    }

    public class GravityTracing : MonoBehaviour
    {
        public int sample = 20;

        public float timeScale = 100;

        // public LineRenderer mathOrbitDrawer;
        public SplineComputer splineComputer;

        private readonly List<ITraceable> _astralBodies = new List<ITraceable>();

        //坐标点
        private readonly Dictionary<ITraceable, List<Vector3>> _orbitPoints =
            new Dictionary<ITraceable, List<Vector3>>();

        private readonly Dictionary<ITraceable, LineRenderer> _orbitRenderers =
            new Dictionary<ITraceable, LineRenderer>();

        private float _deltaTime;

        private bool _isFreezing;

        public bool isFreezing
        {
            get { return _isFreezing; }
        }


        public void Awake()
        {
            _deltaTime = 100f / sample * Time.fixedDeltaTime * timeScale;
            foreach (ITraceable traceable in GetComponentsInChildren<AstralBody>()) AddTracingTarget(traceable);
        }

        private List<ActionType> _actionTypes = new List<ActionType>();
        private Thread           thread;

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
                    TraceGravity();
                }
            }
        }

        private void HandleAction()
        {
            DrawOrbits();
        }

        private void StartNewThread(object[] dict)
        {
            thread = new Thread(new ParameterizedThreadStart(Sample));
            thread.Start(dict);
        }

        private void OnDisable()
        {
            if (thread != null)
            {
                thread.Abort();
            }
        }

        private void FixedUpdate()
        {
            Dispatch();
        }

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

        public void Freeze(bool isFreezing)
        {
            _isFreezing = isFreezing;
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

        public List<AstralBody> GetAstralBodyList()
        {
            var list = (from traceable in _astralBodies
                        select traceable.GetAstralBody()).ToList();
            return list;
        }

        #region 引力步进

        #region 引力计算

        //计算两个星体之间的引力
        private float CalculateGravityModulus(float originMass, float targetMass, float distance)
        {
            return PhysicBase.GetG() * (originMass * targetMass / (distance * distance));
        }

        private Vector3 GetGravityVector3(ITraceable a0, ITraceable a1, float a0Mass,float a1Mass,int sampleTime)
        {
            var distance            = Vector3.Distance(_orbitPoints[a0][sampleTime], _orbitPoints[a1][sampleTime]);
            var normalizedDirection = (_orbitPoints[a1][sampleTime] - _orbitPoints[a0][sampleTime]).normalized;
            // Debug.Log("a0Mass:" + a0Mass);
            // Debug.Log("a1Mass:" + a1Mass);
            // Debug.Log("distance:" + distance);
            // Debug.Log("normalizedDirection:" + normalizedDirection);
            // return PhysicBase.GetG() * (a0Mass * a1Mass / (distance * distance))* normalizedDirection;
            return CalculateGravityModulus(a0Mass, a1Mass, distance) * normalizedDirection;
        }

        private Vector3 CalculateForce(ITraceable astralBody, int sampleTime,Dictionary<ITraceable,float> massDict)
        {
            var result = new Vector3(0, 0, 0);
            //求合力
            foreach (ITraceable body in astralBody.GetAffectedPlanets())
            {
                if (body == astralBody) continue;
                result += GetGravityVector3(astralBody, body, massDict[astralBody],massDict[body],sampleTime);
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

            Dictionary<ITraceable, float> astralBodyMasses = new Dictionary<ITraceable, float>();
            _astralBodies.ForEach(a => astralBodyMasses.Add(a,a.GetMass()));
            
            StartNewThread(new object[]{
                (object)astralBodyVelocities,
                (object)astralBodyMasses
            });

            
           
        }

        private void Sample(object objs)
        {
            // Debug.Log("Tracing!");
            object[] dicts= (object[])objs;
            Dictionary<ITraceable, Vector3> astralBodyVelocities = (Dictionary <ITraceable, Vector3 >)dicts[0];
            Dictionary<ITraceable, float> astralBodyMasses = (Dictionary <ITraceable, float >)dicts[1];
            //开始采样
            for (var i = 0; i < sample; i++)
            {
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
                    _orbitPoints[astralBody].Add(_orbitPoints[astralBody].Last()               +
                                                 astralBodyVelocities[astralBody] * _deltaTime +
                                                 .5f                              * acceleration * Mathf.Pow(_deltaTime, 2));
                    //加速后速度
                    astralBodyVelocities[astralBody] += acceleration * _deltaTime;
                    //Debug.DrawLine(_orbitPoints[astralBody].Last(),_orbitPoints[astralBody].Last() + CalculateForce(astralBody,i));
                }
            }
            _actionTypes.Add(ActionType.Finished);
            // Debug.Log(_actionTypes.Count);
        }

        private void DrawOrbit(ITraceable astralBody)
        {
            _orbitRenderers[astralBody].positionCount = sample;
            _orbitRenderers[astralBody].SetPositions(_orbitPoints[astralBody].ToArray());
        }

        public void DrawOrbits()
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

        public ConicSection GetConicSection(ITraceable astralBody, int sampleCount)
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

            var conicSection = CustomSolver.CalculateOrbit(new Vector2(astralBody.GetTransform().position.x,astralBody.GetTransform().position.z),
                                                              new Vector2(astralBody.GetAffectedPlanets()[0].transform.position.x,astralBody.GetAffectedPlanets()[0].transform.position.z),
                                                                          new Vector2(astralBody.GetVelocity().x,astralBody.GetVelocity().z), astralBody.GetMass(),
                                                                          astralBody.GetAffectedPlanets()[0].mass);

            // Debug.Log("new section:" + "semi major = " + newConicSection.semiMajorAxis);
            // Debug.Log("new section:" + "semi minor = " + newConicSection.semiMinorAxis);
            // Debug.Log("new section:" + "eccentricity = " + newConicSection.eccentricity);
            // Debug.Log("new section:" + "geo center = " + newConicSection.geoCenter);
            // Debug.Log("new section:" + "T = " + newConicSection.GetT(astralBody.GetAffectedPlanets()[0].mass));
            Debug.Log("new section:" + "angle = " + conicSection.angle);

            
            return conicSection;
        }

        public void DrawMathOrbit(ConicSection conicSection, int sam)
        {
            var points = new List<SplinePoint>();
            var step   = 360f / sam;
            for (var i = 0; i < sam; i++)
                points.Add(new SplinePoint(ConvertV2ToV3(conicSection.GetPolarPos(i * step))));
            points.Add(new SplinePoint(ConvertV2ToV3(conicSection.GetPolarPos(360))));

            splineComputer.SetPoints(points.ToArray());
            splineComputer.Close();

            // mathOrbitDrawer.positionCount = sam + 1;
            // mathOrbitDrawer.SetPositions(points.ToArray());
        }

        #endregion
    }
}