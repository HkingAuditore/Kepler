using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class TutorialUI : MonoBehaviour
{
    public  GameObject       awakePanel;
    public  List<GameObject> tutorialClips;
    public  string           tutorialName;
    private int              _curStep = 0;

    public int curStep
    {
        get => _curStep;
        set
        {
            _curStep = value >= 0 ? value : 0;
            UpdateStep();
        }
    }

    public void StartTutorial()
    {
        curStep = 0;
        UpdateStep();
    }

    private void UpdateStep()
    {
        tutorialClips.ForEach(t => t.SetActive(false));
        if(curStep < tutorialClips.Count)
            tutorialClips[curStep].SetActive(true);
        else
        {
            this.gameObject.SetActive(false);
        }
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
        this.tutorialName = SceneManager.GetActiveScene().name + awakePanel.name;
    }

    public void NextStep() => curStep++;

    public void FormerStep() => curStep--;

    public void Quit() => curStep = tutorialClips.Count;
}