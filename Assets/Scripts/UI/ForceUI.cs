using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForceUI : MonoBehaviour
{
    public LineRenderer forceArrow;
    public AstralBody astralBody;
    public Text forceText;
    private Camera _camera;

    private void Start()
    {
        Init();
        _camera = GameManager.GetGameManager.GetMainCameraController().GetMainCamera();
    }

    private void FixedUpdate()
    {
        this.transform.position = astralBody.transform.position;
        forceArrow.SetPosition(0,astralBody.transform.position);
        forceArrow.SetPosition(1,astralBody.transform.position + astralBody.Force * 0.5f);
        forceText.text = "合力:" + (astralBody.Force.magnitude * 0.5f).ToString("f2") + " N";
        int fontSize = (int)((_camera.orthographicSize / 185) * 12);
        forceText.fontSize = fontSize > 8 ? fontSize : 8;

    }

    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        forceArrow.positionCount = 0;
    }

    public void Init()
    {
        forceArrow.positionCount = 2;
    }
}
