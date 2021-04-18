using System;
using System.Collections.Generic;
using TTS;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUI
{
    [Serializable]
    public class TutorialUI : MonoBehaviour
    {
        public  GameObject        awakePanel;
        public  List<GameObject>  tutorialClips;
        public  string            tutorialName;
        public  TutorialManagerUI tutorialManagerUI;
        private int               _curStep;
        private VoiceGenerator    _voiceGenerator;

        public int curStep
        {
            get => _curStep;
            set
            {
                _curStep = value >= 0 ? value : 0;
                UpdateStep();
            }
        }

        // private void Start()
        // {
        //     _voiceGenerator = tutorialManagerUI.voiceGenerator;
        // }

        public void StartTutorial()
        {
            curStep = 0;
            UpdateStep();
        }

        private void UpdateStep()
        {
            tutorialClips.ForEach(t => t.SetActive(false));
            if (curStep < tutorialClips.Count)
                tutorialClips[curStep].SetActive(true);
            // tutorialManagerUI.voiceGenerator.content = ( tutorialClips[curStep].transform.Find("Translucent Image").Find("Text").GetComponent<Text>().text);
            // tutorialManagerUI.voiceGenerator.Speak(( tutorialClips[curStep].transform.Find("Translucent Image").Find("Text").GetComponent<Text>().text));

            else
                gameObject.SetActive(false);
        }

        [ContextMenu("GetTutorialCLipsList")]
        public void GetTutorialCLipsList()
        {
            tutorialClips = new List<GameObject>();
            for (var i = 0; i < transform.childCount; i++) tutorialClips.Add(transform.GetChild(i).gameObject);
        }

        [ContextMenu("GenerateTutorialName")]
        public void GenerateTutorialName()
        {
            tutorialName = SceneManager.GetActiveScene().name + awakePanel.name;
        }

        public void NextStep()
        {
            curStep++;
        }

        public void FormerStep()
        {
            curStep--;
        }

        public void Quit()
        {
            curStep = tutorialClips.Count;
        }
    }
}