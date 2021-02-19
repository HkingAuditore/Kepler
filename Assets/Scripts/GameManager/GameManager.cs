using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _gameManager;
    [SerializeField] private CameraController _mainCameraController;

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
        return _mainCameraController;
    }
}
