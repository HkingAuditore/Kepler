using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _gameManager;
    private Camera _mainCamera;
    public float newMass;

    public static GameManager GetGameManager{
        get{
            return _gameManager;
        }
    }
    
    void Awake () {
        _gameManager = this;
        _mainCamera = Camera.main;
    }
    

    public void ResetScene(float mass)
    {
        Application.LoadLevel(Application.loadedLevelName);
        newMass = mass;
    }

    public Camera GetMainCamera()
    {
        return _mainCamera;
    }
}
