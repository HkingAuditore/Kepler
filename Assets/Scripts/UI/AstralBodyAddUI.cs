using System;
using System.Collections;
using SpacePhysic;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class AstralBodyAddUI : MonoBehaviour
{
    public AstralBodyPlacementUI astralBodyPlacementUI;
    public AstralBody placePrefab;
    public GravityTracing orbits;
    public bool isQuizEditMode;
    public Button button;

    public Transform OrbitCore { get; set; }

    private void Start()
    {
        if (!isQuizEditMode)
        {
            OrbitCore = orbits.transform.Find("Core");
        }
        else
        {
            Debug.Log("To Wait");
            StartCoroutine(WaitForLoad());
        }

    }
    
    public IEnumerator WaitForLoad()
    {
        // Debug.Log("Wait");
        // Debug.Log(GameManager.GetGameManager.quizBase.IsLoadDone);
        yield return new WaitUntil(() => GameManager.GetGameManager.quizBase.IsLoadDone);
        OrbitCore = orbits.transform.Find("Core");
        // Debug.Log(OrbitCore );
    }


    public void Switch2Placement()
    {
        button.gameObject.SetActive(false);
        astralBodyPlacementUI.SetPlacing();
        astralBodyPlacementUI.gameObject.SetActive(true);
    }

    public void Switch2Normal()
    {
        button.gameObject.SetActive(true);
        astralBodyPlacementUI.gameObject.SetActive(false);
    }
}