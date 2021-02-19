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

    private float timer;



    public void OnBeginDrag()
    {
        timer = 0f;
    }

    public void MoveAxis(bool isXAxis)
    {
        var deltaValue = new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));

        timer += Time.deltaTime;
        if (timer > 0.5f)
        { 
            timer = 0.0f;
        }

        editingTarget.transform.position += new Vector3(deltaValue.x * (isXAxis ? 1 : 0), 0, deltaValue.y * (isXAxis ? 0 : 1)) * moveSpeed * timer;
    }
    
    public void MoveCenter()
    {
        var deltaValue = new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));

        timer += Time.deltaTime;
        if (timer > 0.5f)
        { 
            timer = 0.0f;
        }

        editingTarget.transform.position += new Vector3(deltaValue.x, 0, deltaValue.y) * moveSpeed * timer;
    }




    public void OnEndDrag()
    {
        timer = 0f;
    }

}
