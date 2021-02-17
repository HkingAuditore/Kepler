using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _gameManager;
    public CameraController mainCameraController;

    public static GameManager GetGameManager{
        get{
            return _gameManager;
        }
    }
    
    void Awake () {
        _gameManager = this;
        
    }
    


    public CameraController GetMainCameraController()
    {
        return mainCameraController;
    }
}
