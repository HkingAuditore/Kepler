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
    
    private Vector2 deltaValue = Vector2.zero;

    private void Start()
    {

    }



    private void Update()
    {
        this.transform.position = editingTarget.transform.position;
    }

    private float timer;


    private Vector2 oriPos;
    public void OnBeginDrag()
    {
        Debug.Log("ON BEGIN DRAG");
        deltaValue = Vector2.zero;
        oriPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }

    public void MoveAxis(bool isXAxis)
    {
        Vector2 curPos = 
        deltaValue += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Debug.Log(deltaValue);

        timer += Time.deltaTime;
        if (timer > 0.5f)
        { 
            timer = 0.0f;
        }

        editingTarget.transform.position += new Vector3(deltaValue.x * (isXAxis ? 1 : 0), 0, deltaValue.y * (isXAxis ? 0 : 1)) * moveSpeed;
    }




    public void OnEndDrag()
    {
        deltaValue = Vector2.zero;
    }

}
