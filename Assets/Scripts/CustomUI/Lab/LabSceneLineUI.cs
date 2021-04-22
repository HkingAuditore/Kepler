using System;
using System.Collections;
using CustomUI.Quiz;
using GameManagers;
using Quiz;
using SpacePhysic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XmlSaver;

namespace CustomUI.Lab
{
    public class LabSceneLineUI : MonoBehaviour
    {
        public string            name;
        public LabSceneListUI    labSceneListUI;
        public Text              sceneName;
        public SceneBaseStruct<AstralBody> sceneStruct;

        private void OnEnable()
        {
            StartCoroutine(WaitForNameLoad());
        }

        IEnumerator WaitForNameLoad()
        {
            yield return new WaitUntil(() => this.name != "");
            Generate();
        }

        public void OnClick()
        {
            GlobalTransfer.getGlobalTransfer.sceneName = name;
            SceneManager.LoadScene("PhysicScene");
        }
        

        private void Generate()
        {
            sceneName.text = name;
        }

    }
}
