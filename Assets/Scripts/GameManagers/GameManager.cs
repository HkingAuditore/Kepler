﻿using System;
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
    public class GameManager : MonoBehaviour
    {
        [SerializeField]                     private CameraController         _mainCameraController;
        [FormerlySerializedAs("quizEditor")] public  QuizBase                 quizBase;
        public                                       bool                     isQuizEditMode;
        public                                       Camera                   mainCamera;
        public                                       GlobalTimer              globalTimer;
        public                                       List<GameObject>         meshList;
        public                                       GravityTracing           orbit;
        public                                       SatelliteChallengeManger satelliteChallengeManger;
        public                                       AudioSource              bgmSource;
        public                                       int                      globalDistanceScaler;
        private                                      int                      _globalMassScaler;

        public static GameManager GetGameManager { get; private set; }

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
            GetGameManager = this;
        }

        private void Start()
        {
            SetAudioVolume();
        }

        public CameraController GetMainCameraController()
        {
            return _mainCameraController;
        }

        public Mesh GetMeshAndMaterialsFromList(int index, ref List<Material> materials)
        {
            materials = meshList[index].GetComponent<Renderer>().sharedMaterials.ToList();
            return meshList[index].GetComponent<MeshFilter>().sharedMesh;
        }

        public void SetAudioVolume()
        {
            bgmSource.volume = GlobalTransfer.getGlobalTransfer.audioVolume;
        }

        #region 单位

        private int _getNum;

        public void CalculateMassScales()
        {
            var coreBody = orbit.transform.Find("Core").GetComponent<AstralBody>();
            CalculateMassScales(coreBody.realMass);
        }

        public void CalculateMassScales(double realMass)
        {
            var e = MathPlus.MathPlus.GetExponent(realMass);
            Debug.Log("e:"                    + e);
            var offset = Mathf.Clamp(e, 2, 3) - e;

            globalMassScaler = Mathf.CeilToInt(offset / 2f);
            Debug.Log("globalMassScaler" + globalMassScaler);
        }

        public void CalculateDistanceScale()
        {
            var coreBody = orbit.transform.Find("Core").GetComponent<AstralBody>();
            var rBody    = orbit.transform.GetChild(1).GetComponent<AstralBody>();
            var distance = Vector3.Distance(coreBody.transform.position, coreBody.transform.position);
        }

        public int GetK(PropertyUnit propertyUnit, double mass)
        {
            int k;
            switch (propertyUnit)
            {
                case PropertyUnit.M:
                    // k = 7;
                    k = 1 + 2 * GetGameManager.globalDistanceScaler;

                    break;
                case PropertyUnit.Kg:
                    // k = 26;
                    if (_getNum == 0)
                    {
                        CalculateMassScales(mass);
                        _getNum++;
                    }

                    k = GetGameManager.globalMassScaler * 2;
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

        public int GetK(PropertyUnit propertyUnit)
        {
            int k;
            switch (propertyUnit)
            {
                case PropertyUnit.M:
                    // k = 7;
                    k = GetGameManager.globalDistanceScaler * 2;


                    break;
                case PropertyUnit.Kg:
                    // // k = 26;
                    // if (_getNum == 0)
                    // {
                    //     CalculateScales(mass);
                    // }
                    k = GetGameManager.globalMassScaler * 2;
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

        #endregion
    }
}