using System;
using System.Collections.Generic;
using System.Linq;
using CustomCamera;
using Quiz;
using Satellite;
using SpacePhysic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameManagers
{
    /// <summary>
    ///     全局管理
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private CameraController _mainCameraController;

        /// <summary>
        ///     音源
        /// </summary>
        public AudioSource bgmSource;

        /// <summary>
        ///     全局缩放量
        /// </summary>
        public int globalDistanceScaler;

        /// <summary>
        ///     全局计时器
        /// </summary>
        public GlobalTimer globalTimer;

        /// <summary>
        ///     是否为问题模式
        /// </summary>
        public bool isQuizEditMode;

        /// <summary>
        ///     主相机
        /// </summary>
        public Camera mainCamera;

        /// <summary>
        ///     模型列表
        /// </summary>
        public List<GameObject> meshList;

        /// <summary>
        ///     引力步进执行对象
        /// </summary>
        public GravityTracing orbit;

        /// <summary>
        ///     问题管理对象
        /// </summary>
        [FormerlySerializedAs("quizEditor")] public QuizBase quizBase;

        /// <summary>
        ///     卫星挑战管理对象
        /// </summary>
        public SatelliteChallengeManger satelliteChallengeManger;

        private int _globalMassScaler;

        public static GameManager getGameManager { get; private set; }

        public int globalMassScaler
        {
            get => _globalMassScaler;
            private set
            {
                _globalMassScaler    = value;
                globalDistanceScaler = 3 + (-_globalMassScaler - 11) / 2;
            }
        }

        private void Awake()
        {
            getGameManager = this;
        }

        private void Start()
        {
            SetAudioVolume();
        }

        /// <summary>
        ///     获取主相机控制器
        /// </summary>
        /// <returns></returns>
        public CameraController GetMainCameraController()
        {
            return _mainCameraController;
        }

        /// <summary>
        ///     获取模型Mesh与材质
        /// </summary>
        /// <param name="index">模型在GM的序号</param>
        /// <param name="materials">导出材质</param>
        /// <returns></returns>
        public Mesh GetMeshAndMaterialsFromList(int index, ref List<Material> materials)
        {
            materials = meshList[index].GetComponent<Renderer>().sharedMaterials.ToList();
            return meshList[index].GetComponent<MeshFilter>().sharedMesh;
        }

        /// <summary>
        ///     设置音量
        /// </summary>
        public void SetAudioVolume()
        {
            bgmSource.volume = GlobalTransfer.getGlobalTransfer.audioVolume;
        }

        #region 单位

        private int _getNum;

        /// <summary>
        ///     计算质量缩放
        /// </summary>
        public void CalculateMassScales()
        {
            var coreBody = orbit.transform.Find("Core").GetComponent<AstralBody>();
            CalculateMassScales(coreBody.realMass);
        }

        /// <summary>
        ///     计算质量缩放
        /// </summary>
        /// <param name="realMass">参照质量</param>
        public void CalculateMassScales(double realMass)
        {
            var e = MathPlus.MathPlus.GetExponent(realMass);
            // Debug.Log("e:"                    + e);
            var offset = Mathf.Clamp(e, 2, 3) - e;

            globalMassScaler = Mathf.CeilToInt(offset / 2f);
            // Debug.Log("globalMassScaler" + globalMassScaler);
        }

        /// <summary>
        ///     计算距离缩放
        /// </summary>
        public void CalculateDistanceScale()
        {
            var coreBody = orbit.transform.Find("Core").GetComponent<AstralBody>();
            var rBody    = orbit.transform.GetChild(1).GetComponent<AstralBody>();
            var distance = Vector3.Distance(coreBody.transform.position, coreBody.transform.position);
        }

        /// <summary>
        ///     获取缩放系数
        /// </summary>
        /// <param name="propertyUnit">目标单位</param>
        /// <param name="mass">初始参照质量</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int GetK(PropertyUnit propertyUnit, double mass)
        {
            int k;
            switch (propertyUnit)
            {
                case PropertyUnit.M:
                    // k = 7;
                    k = 1 + 2 * getGameManager.globalDistanceScaler;

                    break;
                case PropertyUnit.Kg:
                    // k = 26;
                    if (_getNum == 0)
                    {
                        CalculateMassScales(mass);
                        _getNum++;
                    }

                    k = getGameManager.globalMassScaler * 2;
                    Debug.Log("k:" + k);

                    break;
                case PropertyUnit.S:
                    k = 3 + (-globalMassScaler - globalDistanceScaler * 3);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyUnit), propertyUnit, null);
            }

            return k;
        }

        /// <summary>
        ///     获取缩放系数
        /// </summary>
        /// <param name="propertyUnit">目标单位</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int GetK(PropertyUnit propertyUnit)
        {
            int k;
            switch (propertyUnit)
            {
                case PropertyUnit.M:
                    // k = 7;
                    k = getGameManager.globalDistanceScaler * 2;


                    break;
                case PropertyUnit.Kg:
                    // // k = 26;
                    // if (_getNum == 0)
                    // {
                    //     CalculateScales(mass);
                    // }
                    k = getGameManager.globalMassScaler * 2;
                    // Debug.Log("k:" + k);

                    break;
                case PropertyUnit.S:
                    k = 3 + (-globalMassScaler - globalDistanceScaler * 3);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyUnit), propertyUnit, null);
            }

            return k;
        }

        #endregion
    }
}