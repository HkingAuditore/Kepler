using System.Collections.Generic;
using GameManagers;

namespace CustomUI.Tutorial
{
    public class MultiTutorialManagerUI : TutorialManagerUI
    {
        public List<Difficulty> difficulties = new List<Difficulty>();

        protected override void MonitorAwakePanel()
        {
            for (var i = 0; i < tutorialUiList.Count; i++)
            {
                var tutorial = tutorialUiList[i];
                if (tutorial.CheckAwake() &&
                    !hasBeenActivatedDict[tutorial.tutorialName]
                 && GlobalTransfer.getGlobalTransfer.difficulty == difficulties[i])
                {
                    tutorial.gameObject.SetActive(true);
                    tutorial.StartTutorial();
                    hasBeenActivatedDict[tutorial.tutorialName] = true;
                }
            }
        }
    }
}