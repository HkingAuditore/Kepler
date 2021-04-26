using System.Collections.Generic;
using UnityEngine;

namespace Satellite
{
    /// <summary>
    /// 卫星控制器
    /// </summary>
    public class SatelliteController : MonoBehaviour
    {
        /// <summary>
        ///     角速度
        /// </summary>
        public float angularSpeed;

        /// <summary>
        ///     卫星
        /// </summary>
        public Satellite satellite;

        /// <summary>
        ///     速度
        /// </summary>
        public float speed;

        private readonly List<SatelliteEngine>[] _satelliteEngineStageLists = new List<SatelliteEngine>[5];

        /// <summary>
        ///     当前引擎执行阶段
        /// </summary>
        private int curEngineStage;

        /// <summary>
        ///     执行阶段数量
        /// </summary>
        private int engineStages;


        private void Start()
        {
            GenerateEngineStageList();
        }

        private void FixedUpdate()
        {
            // Rotate();
            // Push();
        }

        /// <summary>
        ///     设置速度
        /// </summary>
        /// <param name="newSpeed"></param>
        public void SetCurDirVelocity(float newSpeed)
        {
            _satelliteEngineStageLists[curEngineStage].ForEach(engine => engine.SetCurDirVelocity(newSpeed));
        }

        private void GenerateEngineStageList()
        {
            foreach (var satellitePart in satellite.satelliteParts)
                if (satellitePart.PartType == SatelliteType.Engine)
                {
                    var engine = (SatelliteEngine) satellitePart;
                    Debug.Log("engine stage:" + engine.engineStage);
                    if (_satelliteEngineStageLists[engine.engineStage] == null)
                    {
                        _satelliteEngineStageLists[engine.engineStage] = new List<SatelliteEngine>();
                        engineStages++;
                    }

                    _satelliteEngineStageLists[engine.engineStage].Add(engine);
                }

            Debug.Log("Rocket stages:" + engineStages);
            _satelliteEngineStageLists[engineStages] = new List<SatelliteEngine>();
            _satelliteEngineStageLists[engineStages].Add(satellite.satelliteCore);
        }

        private void Rotate()
        {
            if (Input.GetKey(KeyCode.A))
                _satelliteEngineStageLists[curEngineStage]
                   .ForEach(engine => engine.Rotate(-engine.transform.right * angularSpeed));

            // this._satellitePart.Rotate(-this.transform.up * angularSpeed);

            if (Input.GetKey(KeyCode.D))
                _satelliteEngineStageLists[curEngineStage]
                   .ForEach(engine => engine.Rotate(engine.transform.right * angularSpeed));

            // this._satellitePart.Rotate(this.transform.up  * angularSpeed);
        }

        private void Push()
        {
            if (Input.GetKey(KeyCode.W))
                _satelliteEngineStageLists[curEngineStage].ForEach(engine => engine.Push(engine.transform.up * speed));

            // if (Input.GetKey(KeyCode.S))
            // {
            //     this._satellitePart.Push(-this.transform.forward * speed);
            // }
        }

        /// <summary>
        ///     分离
        /// </summary>
        public void SeparateControl()
        {
            if (curEngineStage < engineStages)
                Separate();
        }

        private void Separate()
        {
            //是否到达核心层
            if (curEngineStage == engineStages - 1)
            {
                _satelliteEngineStageLists[curEngineStage++].ForEach(engine => engine.Separate(true));
                _satelliteEngineStageLists[curEngineStage].ForEach(engine => engine.Separate(false));
            }
            else
            {
                _satelliteEngineStageLists[curEngineStage++].ForEach(engine => engine.Separate(true));
                _satelliteEngineStageLists[curEngineStage].ForEach(engine => engine.Separate(true));
            }

            Debug.Log("Now in stage:" + curEngineStage);
        }
    }
}