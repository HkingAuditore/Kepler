﻿using GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI
{
    public class LabModeUI : MonoBehaviour
    {
        public GameObject difficultyPanel;
        public GameObject sceneListPanel;
        public GameObject satellitePanel;
        public int        satelliteSceneIndex;

        public void ToLab()
        {
            SceneManager.LoadScene("PhysicScene");
        }

        public void ToMoon(int difficulty)
        {
            GlobalTransfer.getGlobalTransfer.difficulty = (Difficulty) difficulty;
            switch (satelliteSceneIndex)
            {
                case 0 :
                    SceneManager.LoadScene("Satellite");
                    break;
                case 1:
                    SceneManager.LoadScene("Satellite 1");
                    break;
                default:
                    break;
            }

        }

        public void ToMain()
        {
            SceneManager.LoadScene("Main");
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
    }
}