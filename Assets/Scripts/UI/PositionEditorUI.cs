using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PositionEditorUI : MonoBehaviour
{
    public Button xAxis;
    public Button zAxis;
    public Transform editingTarget;
    public float moveSpeed;

    private Camera _camera;

    private void Start()
    {
        _camera = GameManager.GetGameManager.GetMainCameraController().GetMainCamera();
    }


    private void Update()
    {
        this.transform.position = new Vector3(editingTarget.transform.position.x,this.transform.position.y,editingTarget.transform.position.z);
        this.transform.localScale = (_camera.orthographicSize / 185) * new Vector3(1,1,1);
    }
    


    private Vector3 oriMousePos;
    public void OnBeginDrag()
    {
        GameManager.GetGameManager.GetMainCameraController().IsFollowing = false;
        oriMousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
    }

    public void MoveAxis(bool isXAxis)
    {
        Vector3 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        var deltaValue = mousePos - oriMousePos;
        oriMousePos = mousePos;
        editingTarget.transform.position += new Vector3(deltaValue.x * (isXAxis ? 1 : 0), 0, deltaValue.z * (isXAxis ? 0 : 1)) ;
    }
    
    public void MoveCenter()
    {
        Vector3 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        var deltaValue = mousePos - oriMousePos;
        oriMousePos = mousePos;
        editingTarget.transform.position += new Vector3(deltaValue.x, 0, deltaValue.z) ;

    }




    public void OnEndDrag()
    {
        GameManager.GetGameManager.GetMainCameraController().IsFollowing = true;
    }

}
