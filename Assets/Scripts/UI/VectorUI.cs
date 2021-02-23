using System;
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
    public AstralBody astralBody;
    public Text vectorText;
    public vectorType thisType;
    public string header;
    public string unit;
    public float showSize = .5f;

    private Camera _camera;
    private Vector3 _targetVector;


    private void Start()
    {
        Init();
        _camera = GameManager.GetGameManager.GetMainCameraController().GetMainCamera();
    }

    private void FixedUpdate()
    {
        switch (thisType)
        {
            case vectorType.Force:
                _targetVector = astralBody.Force;
                break;
            case vectorType.Velocity:
                _targetVector = astralBody.GetRigidbody().velocity;
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
        transform.position = new Vector3(Mathf.Clamp(tmpScreenPos.x, 60, Screen.width - 60),
                                         Mathf.Clamp(tmpScreenPos.y, 20, Screen.height - 20),
                                         0);
        vectorText.text = header + ":" + (_targetVector.magnitude * showSize).ToString("f2") + " " + unit;
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

    public void Init()
    {
        vectorArrow.positionCount = 2;
    }
}