using Quiz;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CameraController _mainCameraController;
    public                   QuizBase         quizBase;
    public                   bool             isQuizEditMode;

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