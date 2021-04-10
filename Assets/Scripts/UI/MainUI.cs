using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainUI : MonoBehaviour
{
    public GameObject settingPanel;
    public void LoadLabScene()
    {
        SceneManager.LoadScene("LabMode") ;
    }

    public void LoadTestMode()
    {
        SceneManager.LoadScene("TestMode") ;

    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OpenSettingPanel()
    {
        settingPanel.SetActive(true);
    }
}
