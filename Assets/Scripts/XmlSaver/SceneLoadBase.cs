using System;
using System.Collections.Generic;
using Quiz;
using SpacePhysic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace XmlSaver
{
    public class SceneLoadBase<T> : MonoBehaviour where T : AstralBody
    {
        protected delegate void AstralBodyDataDictProcessHandler(T prefab,AstralBodyDataDict<T> astralBodyDataDict);

        /// <summary>
        /// 场景星球集合
        /// </summary>
        [SerializeField] protected List<AstralBodyDict<T>> astralBodiesDict;
        /// <summary>
        ///     生成用实体
        /// </summary>
        public T astralBodyPrefab;
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
        public Transform      sceneRoot;

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


        protected virtual void GenerateAstralBodiesWithPrefab()
        {
            List<AstralBodyDict<T>>     astralBodyDicts = new List<AstralBodyDict<T>>();
            AstralBody core ;
            foreach (var pair in astralBodiesDict)
            {
                var target =
                    Instantiate(pair.astralBody, pair.transform.position, pair.transform.rotation, sceneRoot);
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
            if (typeof(T) == typeof(QuizAstralBody))
            {
                astralBodyDicts = (List<AstralBodyDict<T>>)Activator.CreateInstance(typeof(QuizAstralBodyDataDict));
            }
            AstralBodyDataDictProcessHandler processHandler = (prefab,pair) =>
                                                              {
                                                                  prefab.realMass      = pair.mass;
                                                                  prefab.size          = pair.originalSize;
                                                                  prefab.oriVelocity   = pair.oriVelocity;
                                                                  prefab.affectRadius  = pair.affectRadius;
                                                                  prefab.enableAffect  = pair.enableAffect;
                                                                  prefab.enableTracing = pair.enableTracing;

                                                              };
            if (dataDictProcessHandler != null)
            {
                processHandler += dataDictProcessHandler;
            }
            
            AstralBody core ;

            processHandler += (prefab, pair) =>
                              {
                                  var target =
                                      Instantiate(astralBodyPrefab, pair.position, Quaternion.Euler(0, 0, 0),
                                                  sceneRoot);
                                  target.meshNum = pair.meshNum;
                                  orbitBase.AddTracingTarget(target);
                                  if (pair.isCore)
                                      core = target;

                                  astralBodyDicts.Add(new AstralBodyDict<T>(target.transform, target, pair.isCore));

                                  target.gameObject.name = target.gameObject.name.Replace("(Clone)", "");
                                  if (pair.isCore) target.gameObject.name = "Core";

                              };
            
            if (afterDictProcessHandler != null)
            {
                processHandler += afterDictProcessHandler;
            }
            
            foreach (AstralBodyDataDict<T> pair in _astralBodyStructDictList)
            {
                processHandler(astralBodyPrefab,pair);
            }

            astralBodiesDict = astralBodyDicts;
        }
    }
}