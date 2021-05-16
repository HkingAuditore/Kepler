using System.Collections;
using System.Collections.Generic;
using GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace CustomUI
{
    public class LabModeUI : MonoBehaviour
    {
        public GameObject difficultyPanel;
        public GameObject sceneListPanel;
        public GameObject satellitePanel;
        public GameObject modePanel;
        public int        satelliteSceneIndex;

        public void ToLab()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("PhysicScene");
        }

        public void ToSatelliteChallenge(int difficulty)
        {
            GlobalTransfer.getGlobalTransfer.difficulty = (Difficulty) difficulty;
            switch (satelliteSceneIndex)
            {
                case 0 :
                    GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("Satellite");
                    break;
                case 1:
                    GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("Satellite 1");
                    break;
                default:
                    break;
            }

        }

        public void ToMain()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("Main");
        }

        public void ShowDifficultyPanel()
        {
            difficultyPanel.SetActive(true);
        }

        public void CloseDifficultyPanel()
        {
            difficultyPanel.SetActive(false);
        }

        public void ShowScenesListPanel()
        {
            sceneListPanel.SetActive(true);
        }
        
        public void CloseScenesListPanel()
        {
            sceneListPanel.SetActive(false);
        }

        public void ShowSatellitePanel()
        {
            satellitePanel.SetActive(true);
        }
        public void CloseSatellitePanel()
        {
            satellitePanel.SetActive(false);
        }

        public void SetSatelliteIndex(int index)
        {
            this.satelliteSceneIndex = index;
        }

        public void ShowModePanel()
        {
            modePanel.SetActive(true);
        }
        
        public void CloseModePanel()
        {
            modePanel.SetActive(false);
        }

        public void ToFreeTravel()
        {

            if (GlobalTransfer.getGlobalTransfer.hasXrInput)
            {
                GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("SpaceTraveller VR",true);
            }
            else
            {
                // GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("SpaceTraveller VR", true);

                GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("SpaceTraveller");
            }
        }
        
        

    }
}