using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialManagerUI : MonoBehaviour
{
   public List<TutorialUI> tutorialUiList = new List<TutorialUI>();

   protected static Dictionary<string, bool> hasBeenActivatedDict = new Dictionary<string, bool>();

   private void Start()
   {
       tutorialUiList.ForEach(t =>
                              {
                                  if (!hasBeenActivatedDict.ContainsKey(t.tutorialName))
                                  {
                                      hasBeenActivatedDict.Add(t.tutorialName,false);
                                  }
                              });
   }

   public void Update()
   {
       MonitorAwakePanel();
   }

   protected virtual void MonitorAwakePanel()
   {
       foreach (var tutorial in tutorialUiList.Where(tutorial => (tutorial.awakePanel.activeSelf &&
                                                                  !hasBeenActivatedDict[tutorial.tutorialName]) ))
       {
           tutorial.gameObject.SetActive(true);
           tutorial.StartTutorial();
           hasBeenActivatedDict[tutorial.tutorialName] = true;
       }
   }
}
