using System.Collections.Generic;
using System.Linq;
using TTS;
using UnityEngine;

namespace CustomUI.Tutorial
{
    public class TutorialManagerUI : MonoBehaviour
    {
        public VoiceGenerator   voiceGenerator;

        protected static Dictionary<string, bool> hasBeenActivatedDict = new Dictionary<string, bool>();
        public           List<TutorialUI>         tutorialUiList       = new List<TutorialUI>();

        private void Start()
        {
            tutorialUiList.ForEach(t =>
                                   {
                                       if (!hasBeenActivatedDict.ContainsKey(t.tutorialName))
                                           hasBeenActivatedDict.Add(t.tutorialName, false);
                                   });
        }

        public void Update()
        {
            MonitorAwakePanel();
        }

        protected virtual void MonitorAwakePanel()
        {
            foreach (var tutorial in tutorialUiList.Where(tutorial => tutorial.awakePanel.activeSelf &&
                                                                      !hasBeenActivatedDict[tutorial.tutorialName]))
            {
                tutorial.gameObject.SetActive(true);
                tutorial.StartTutorial();
                hasBeenActivatedDict[tutorial.tutorialName] = true;
            }
        }
    }
}