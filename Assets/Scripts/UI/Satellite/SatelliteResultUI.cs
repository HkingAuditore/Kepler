using System;
using System.Collections;
using System.Collections.Generic;
using Satellite;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SatelliteResultUI : MonoBehaviour
{
    public GameObject panel;
    public Text       resultText;
    void Start()
    {
        StartCoroutine(WaitForCheck());
    }
    
    IEnumerator WaitForCheck()
    {
        yield return new WaitUntil(() => GameManager.GetGameManager.satelliteChallengeManger.isCheckEnd); //Lambda表达式
        ShowResultPanel(GameManager.GetGameManager.satelliteChallengeManger.satelliteResultType);
    }

    private void ShowResultPanel(SatelliteResultType resultType)
    {
        switch (resultType)
        {
            case SatelliteResultType.Success:
                resultText.text = "\"这是我个人的一小步，却是人类的一大步。\"";
                break;
            case SatelliteResultType.Crash:
                resultText.text = "你的卫星成为了人类的新遗迹";
                break;
            case SatelliteResultType.NotOrbit:
                resultText.text = "未能进入目标轨道";

                break;
            case SatelliteResultType.NonResult:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(resultType), resultType, null);
        }
        panel.gameObject.SetActive(true);
    }

    public void BackToLabMode()
    {
        SceneManager.LoadScene("LabMode");
        
    }

    public void Reload()
    {
        SceneManager.LoadScene("Satellite");
    }
}
