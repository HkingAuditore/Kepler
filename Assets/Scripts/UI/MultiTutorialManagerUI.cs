using System.Collections.Generic;
using UnityEngine;

public class MultiTutorialManagerUI : TutorialManagerUI
{
    public List<Difficulty> difficulties = new List<Difficulty>();

    protected override void MonitorAwakePanel()
    {
        for (var i = 0; i < tutorialUiList.Count; i++)
        {
            var tutorial = tutorialUiList[i];
            if (tutorial.awakePanel.activeSelf &&
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