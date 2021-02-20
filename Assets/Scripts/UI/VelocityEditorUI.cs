using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VelocityEditorUI : MonoBehaviour
{
    public LineRenderer velocityLine;
    public AstralBody editingTarget;
    public Text speedUI;
    public float speedText = .1f;

    private Vector3 _velocity;
    private Camera _camera;
    private Vector3 _mousePos;
    
     private protected float Speed
     {
         get
         {
             Vector3 oriPos = new Vector3(velocityLine.GetPosition(0).x,0,velocityLine.GetPosition(0).z);
             Vector3 tarPos = new Vector3(velocityLine.GetPosition(1).x,0,velocityLine.GetPosition(1).z);
             Vector3 oriScreenPos = _camera.WorldToScreenPoint(oriPos);
             Vector3 tarScreenPos = _camera.WorldToScreenPoint(tarPos);
             
             return Vector3.Magnitude((tarScreenPos - oriScreenPos) * speedText);
         }
     }

     private void Start()
    {
        velocityLine.SetPosition(0, editingTarget.transform.position);
        _camera = GameManager.GetGameManager.GetMainCameraController().GetMainCamera();
    }

    private void Update()
    {
        this.transform.position = editingTarget.transform.position;
        int fontSize = (int)((_camera.orthographicSize / 185) * 12);
        speedUI.fontSize = fontSize > 8 ? fontSize : 8;
        EditVelocity();
        SetVelocity();
        if (Input.GetMouseButton(1))
        {
            this.gameObject.SetActive(false);
        }
    }
    

    private void EditVelocity()
    {
        Time.timeScale = 0;
        _mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        velocityLine.SetPosition(0, editingTarget.transform.position);
        velocityLine.SetPosition(1, new Vector3(_mousePos.x,editingTarget.transform.position.y,_mousePos.z));

        speedUI.text = "速度：" + this.Speed.ToString("f2") + " m/s";
    }

    private void SetVelocity()
    {
        if (Input.GetMouseButtonUp(0))
        {
            _velocity = _mousePos - editingTarget.transform.position;
            editingTarget.ChangeVelocity(new Vector3(_velocity.x,0,_velocity.z) * speedText);
            this.gameObject.SetActive(false);
            Time.timeScale = 1;
        }
    }
}
 