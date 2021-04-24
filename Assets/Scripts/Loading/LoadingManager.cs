using System.Collections;
using GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Loading
{
    public class LoadingManager : MonoBehaviour
    {
        public string nextLoadSceneName;
        public Image  loadingBar;
        public Text   loadingText;

        private float          _curProgressValue;
        private float          _showProgressValue;
        private AsyncOperation _operation;

        private void Start()
        {
            nextLoadSceneName = GlobalTransfer.getGlobalTransfer.nextScene;
            LoadNextScene();
        }

        private void Update()
        {
            if (_showProgressValue < _curProgressValue) _showProgressValue += Time.deltaTime;
            loadingBar.fillAmount = Mathf.SmoothStep(0, .85f, _showProgressValue * 1.1f) / .85f;
            loadingText.text      = (Mathf.Clamp01(_showProgressValue * 1.2f) * 100).ToString("f2") + "%";
            if (_showProgressValue > .85f)
            {
                _operation.allowSceneActivation = true;
            }
        }

        private void LoadNextScene()
        {
            StartCoroutine(AsyncLoading());
        }


        private IEnumerator AsyncLoading()
        {
            _operation                      = SceneManager.LoadSceneAsync(nextLoadSceneName);
            _operation.allowSceneActivation = false;

            while (!_operation.isDone)
            {
                _curProgressValue = _operation.progress;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}