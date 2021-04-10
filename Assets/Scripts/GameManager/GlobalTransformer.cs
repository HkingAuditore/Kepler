using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTransformer : MonoBehaviour
{
    [SerializeField]private float _audioVolume = .5f;
    
    public static GlobalTransformer GetGlobalTransformer { get; private set; }

    public float audioVolume
    {
        get => _audioVolume;
        set
        {
            _audioVolume = Mathf.Clamp01(value);
            GameManager.GetGameManager.SetAudioVolume();
        }
    }

    private void Awake()
    {
        GetGlobalTransformer = this;
    }
    
    private void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);
    }

}
