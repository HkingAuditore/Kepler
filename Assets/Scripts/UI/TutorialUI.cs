using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    public  List<GameObject> tutorialClips;
    private int              _curStep;

    public int curStep
    {
        get => _curStep;
        set
        {
            _curStep = value;
            UpdateStep();
        }
    }

    private void Start()
    {
        tutorialClips[curStep].SetActive(true);
    }

    private void UpdateStep()
    {
        tutorialClips.ForEach(t => t.SetActive(false));
        if(curStep < tutorialClips.Count)
            tutorialClips[curStep].SetActive(true);
    }

    [ContextMenu("GetTutorialCLipsList")]
    public void GetTutorialCLipsList()
    {
        tutorialClips = new List<GameObject>();
        for (var i = 0; i < transform.childCount; i++) tutorialClips.Add(transform.GetChild(i).gameObject);
    }

    public void NextStep() => curStep++;

    public void FormerStep() => curStep--;
}