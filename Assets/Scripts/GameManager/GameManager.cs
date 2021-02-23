using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CameraController _mainCameraController;

    public static GameManager GetGameManager { get; private set; }

    private void Awake()
    {
        GetGameManager = this;
    }


    public CameraController GetMainCameraController()
    {
        return _mainCameraController;
    }
}