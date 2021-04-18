using System;
using StaticClasses;
using UnityEngine;
using UnityEngine.UI;

public enum vectorType
{
    Force,
    Velocity
}

public class VectorUI : MonoBehaviour
{
    public LineRenderer vectorArrow;
    public AstralBody   astralBody;
    public Text         vectorText;
    public vectorType   thisType;
    public string       header;
    public string       unit;
    public float        showSize = .5f;

    private Camera  _camera;
    private Vector3 _targetVector;


    private void Start()
    {
        _camera = GameManager.GetGameManager.GetMainCameraController().GetMainCamera();
        Init();
    }

    private void FixedUpdate()
    {
        ShowVector();
        // int fontSize = (int)((_camera.orthographicSize / 185) * 12);
        // vectorText.fontSize = fontSize > 8 ? fontSize : 8;
    }

    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        vectorArrow.positionCount = 0;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void ShowVector()
    {
        switch (thisType)
        {
            case vectorType.Force:
                _targetVector = astralBody.Force;
                break;
            case vectorType.Velocity:
                _targetVector = astralBody.GetVelocity();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (_targetVector.magnitude <= 0)
        {
            vectorText.text = "";
            return;
        }


        transform.position = astralBody.transform.position;
        vectorArrow.SetPosition(0, astralBody.transform.position);
        vectorArrow.SetPosition(1, astralBody.transform.position + _targetVector * showSize);
        var tmpScreenPos = _camera.WorldToScreenPoint(astralBody.transform.position + showSize * _targetVector);
        // Debug.Log(this.gameObject.name + " : " + tmpScreenPos);
        transform.position = new Vector3(Mathf.Clamp(tmpScreenPos.x, 60, Screen.width  - 60),
                                         Mathf.Clamp(tmpScreenPos.y, 20, Screen.height - 20),
                                         0);
        string result ="";
        switch (thisType)
        {
            case vectorType.Force:
                result = ((double) (_targetVector.magnitude * 1000)).ToSuperscript(2);
                break;
            case vectorType.Velocity:
                result = ((double) (_targetVector.magnitude * GameManager.GetGameManager.GetK(PropertyUnit.M))).ToSuperscript(2);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        vectorText.text = header + ":" + result + unit;
    }

    public void Init()
    {
        vectorArrow.positionCount = 2;
        ShowVector();
    }
}