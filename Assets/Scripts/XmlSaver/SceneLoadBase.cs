using System;
using System.Collections.Generic;
using GameManagers;
using Quiz;
using SpacePhysic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace XmlSaver
{
    /// <summary>
    /// 存档加载管理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SceneLoadBase<T> : MonoBehaviour where T : AstralBody
    {
        /// <summary>
        /// 星体数据处理委托
        /// </summary>
        /// <param name="prefab">星体引用</param>
        /// <param name="astralBodyDataDict">星体数据</param>
        /// <param name="astralBodyDictList">星体表</param>
        protected delegate void AstralBodyDataDictProcessHandler(T prefab,AstralBodyDataDict<T> astralBodyDataDict,List<AstralBodyDict<T>> astralBodyDictList);

        /// <summary>
        /// 场景星球集合
        /// </summary>
        [SerializeField] protected List<AstralBodyDict<T>> astralBodiesDict;
        /// <summary>
        ///     生成用实体
        /// </summary>
        public T astralBodyPrefab;

        /// <summary>
        /// 核心
        /// </summary>
        public T core;
        /// <summary>
        ///     是否由Prefab载入
        /// </summary>
        public bool isLoadByPrefab;

        /// <summary>
        ///     加载完成事件
        /// </summary>
        public UnityEvent loadDoneEvent = new UnityEvent();

        /// <summary>
        ///     加载文件名
        /// </summary>
        public string loadTarget;
        /// <summary>
        /// 轨道
        /// </summary>
        public                                    GravityTracing orbitBase;
        /// <summary>
        /// 生成基点
        /// </summary>
       public Transform OrbitRoot;

        protected List<AstralBodyDataDict<T>> _astralBodyStructDictList;

        #region IS_LOAD_DONE

        private bool _isLoadDone;
        public bool isLoadDone
        {
            protected set
            {
                _isLoadDone = value;
                if (isLoadDone) loadDoneEvent.Invoke();
            }
            get => _isLoadDone;
        }

        #endregion

        protected virtual void Start()
        {
            try
            {
                this.loadTarget = GlobalTransfer.getGlobalTransfer.sceneName;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            loadDoneEvent.AddListener(() => { GameManager.getGameManager.CalculateMassScales(); });
            // List<AstralBody> astralBodies = new List<AstralBody>();
            // 放置星球
            if (isLoadByPrefab)
            {
                GenerateAstralBodiesWithPrefab();
            }
            else
            {
                LoadScene(loadTarget);
                GenerateAstralBodiesWithoutPrefab();
            }


            isLoadDone = true;
        }

        protected virtual void LoadScene(string fileName)
        {
            var result = XmlSaver.XmlSaver<T>.ConvertXml2SceneBase(XmlSaver.XmlSaver<AstralBody>.LoadXml(fileName), fileName);
            _astralBodyStructDictList = result.astralBodyStructList;

        }

        protected virtual void GenerateAstralBodiesWithPrefab()
        {
            List<AstralBodyDict<T>>     astralBodyDicts = new List<AstralBodyDict<T>>();
            AstralBody core ;
            foreach (var pair in astralBodiesDict)
            {
                var target =
                    Instantiate(pair.astralBody, pair.transform.position, pair.transform.rotation, OrbitRoot);
                orbitBase.AddTracingTarget(target);
                if (pair.isCore)
                    core = target;

                astralBodyDicts.Add(new AstralBodyDict<T>(target.transform, target,pair.isCore));                // Debug.Log("add HashCode:" + target.GetHashCode());
                target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
            }

            astralBodiesDict = astralBodyDicts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataDictProcessHandler">对AstralBodyDataDict的数据传输处理委托</param>
        /// <param name="afterDictProcessHandler">对AstralBodyDataDict的后置处理委托</param>
        protected virtual void GenerateAstralBodiesWithoutPrefab(AstralBodyDataDictProcessHandler dataDictProcessHandler = null,
                                                                 AstralBodyDataDictProcessHandler afterDictProcessHandler = null)
        {
            List<AstralBodyDict<T>>          astralBodyDicts = new List<AstralBodyDict<T>>();

            AstralBodyDataDictProcessHandler processHandler = (prefab,pair,dictList) =>
                                                              {
                                                                  prefab.realMass      = pair.mass;
                                                                  prefab.size          = pair.originalSize;
                                                                  prefab.oriVelocity   = pair.oriVelocity;
                                                                  prefab.affectRadius  = pair.affectRadius;
                                                                  prefab.enableAffect  = pair.enableAffect;
                                                                  prefab.enableTracing = pair.enableTracing;
                                                                  prefab.size          = pair.originalSize;
                                                              };
            if (dataDictProcessHandler != null)
            {
                processHandler += dataDictProcessHandler;
            }

            
            if (afterDictProcessHandler != null)
            {
                processHandler += afterDictProcessHandler;
            }
            else
            {
                processHandler += (prefab, pair, dictList) =>
                                  {
                                      var target =
                                          Instantiate(astralBodyPrefab, pair.position, Quaternion.Euler(0, 0, 0),
                                                      OrbitRoot);
                                      target.meshNum = pair.meshNum;
                                      orbitBase.AddTracingTarget(target);
                                      if (pair.isCore)
                                          core = target;
                                      else
                                      {
                                          target.affectedPlanets.Add(core);
                                      }

                                      dictList.Add(new AstralBodyDict<T>(target.transform, target, pair.isCore));

                                      target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                                      if (pair.isCore) target.gameObject.name = "Core";

                                  };

            }
            
            foreach (AstralBodyDataDict<T> pair in _astralBodyStructDictList)
            {
                processHandler(astralBodyPrefab, pair, astralBodyDicts);
            }

            astralBodiesDict = astralBodyDicts;
        }
    }
    
}