using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

namespace GameManagers
{
    /// <summary>
    ///     全局信息传递
    /// </summary>
    public class GlobalTransfer : MonoBehaviour
    {
        [SerializeField] private float _audioVolume = .5f;

        /// <summary>
        ///     难度
        /// </summary>
        public Difficulty difficulty;

        /// <summary>
        ///     问题名称
        /// </summary>
        public string sceneName;

        public string nextScene;

        public static GlobalTransfer getGlobalTransfer { get; private set; }

        /// <summary>
        ///     获取音量
        /// </summary>
        public float audioVolume
        {
            get => _audioVolume;
            set
            {
                _audioVolume = Mathf.Clamp01(value);
                GameManager.getGameManager.SetAudioVolume();
            }
        }

        private void Awake()
        {
            getGlobalTransfer = this;
            
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(GetXRInputSituation());
        }


        private bool _enableVr = false;
        /// <summary>
        /// 在Loading场景中加载新场景
        /// </summary>
        /// <param name="nextLoadSceneName"></param>
        /// <param name="isVr">是否为VR模式</param>
        public void LoadSceneInLoadingScene(string nextLoadSceneName,bool isVr = false)
        {
            if(!isVr && _enableVr)
            {
                StopXR();
                _enableVr = false;
            }
            else if(isVr)
            {
                StartCoroutine(StartXR());
                _enableVr = true;
            }
            
            this.nextScene = nextLoadSceneName;
            SceneManager.LoadSceneAsync("Loading");

            
        }
        
        
        public IEnumerator StartXR() {
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
 
            if (XRGeneralSettings.Instance.Manager.activeLoader == null || XRGeneralSettings.Instance.Manager.activeLoaders[0].name != "Oculus Loader") {
                this.nextScene = this.nextScene.Split(new char[] {' '})[0];
                Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
            } else {
                Debug.Log("Starting XR...");
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                yield return null;
            }
        }


        public bool hasXrInput = false;
        
        public IEnumerator GetXRInputSituation() {
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
            Debug.Log(XRGeneralSettings.Instance.Manager.activeLoader);
            if (XRGeneralSettings.Instance.Manager.activeLoader == null || XRGeneralSettings.Instance.Manager.activeLoaders[0].name != "Oculus Loader")
            {
                hasXrInput = false;
                throw new Exception("Initializing XR Failed. Check Editor or Player log for details.");
            } else {
                Debug.Log("Starting XR...");
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                hasXrInput = true;
                yield return null;
            }

            StopXR();
        }
        
 
        void StopXR() {
            Debug.Log("Stopping XR...");
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            Debug.Log("XR stopped completely.");
        }
    }
}