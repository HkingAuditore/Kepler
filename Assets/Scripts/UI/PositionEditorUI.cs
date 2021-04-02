using System;
using UnityEngine;
using UnityEngine.UI;

public class PositionEditorUI : MonoBehaviour
{
    public Button    xAxis;
    public Button    zAxis;
    public AstralBody editingTarget;
    public float     moveSpeed;
    public bool      isQuizEditor;

    private AstralBody _astralBody;

    private Camera _camera;


    private Vector3 oriMousePos;

    private void Start()
    {
        _camera = GameManager.GetGameManager.GetMainCameraController().GetMainCamera();
    }
    

    private void Update()
    {
        transform.position = new Vector3(editingTarget.transform.position.x, transform.position.y,
                                         editingTarget.transform.position.z);
        transform.localScale = _camera.orthographicSize / 185 * new Vector3(1, 1, 1);
    }

    public void OnBeginDrag()
    {
        GameManager.GetGameManager.GetMainCameraController().IsFollowing = false;
        oriMousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
    }

    public void MoveAxis(bool isXAxis)
    {
        var mousePos   = _camera.ScreenToWorldPoint(Input.mousePosition);
        var deltaValue = mousePos - oriMousePos;
        oriMousePos = mousePos;
        editingTarget.transform.position +=
            new Vector3(deltaValue.x * (isXAxis ? 1 : 0), 0, deltaValue.z * (isXAxis ? 0 : 1));
    }

    public void MoveCenter()
    {
        var mousePos   = _camera.ScreenToWorldPoint(Input.mousePosition);
        var deltaValue = mousePos - oriMousePos;
        oriMousePos                      =  mousePos;
        editingTarget.transform.position += new Vector3(deltaValue.x, 0, deltaValue.z);
    }


    public void OnEndDrag()
    {
        GameManager.GetGameManager.GetMainCameraController().IsFollowing = true;
        if (isQuizEditor)
        {
            ((QuizAstralBody) this.editingTarget).UpdateHighCost();
        }
    }


}