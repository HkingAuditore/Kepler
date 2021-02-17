using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AstralBodyEditorUI : MonoBehaviour
{
    public AstralBody astralBody;

    public Slider massSlider;
    public LineRenderer forceArrow;
    public PositionEditorUI positionEditorUI;

    private void Start()
    {
        forceArrow.positionCount = 2;
    }

    private void FixedUpdate()
    {
        forceArrow.SetPosition(0,astralBody.transform.position);
        forceArrow.SetPosition(1,astralBody.transform.position + astralBody.Force * 0.5f);
    }

    private void OnEnable()
    {
        forceArrow.positionCount = 2;
        forceArrow.gameObject.SetActive(true);
        positionEditorUI.editingTarget = this.astralBody.transform;
        positionEditorUI.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        forceArrow.gameObject.SetActive(false);
        positionEditorUI.gameObject.SetActive(false);
        positionEditorUI.editingTarget = null;
        forceArrow.positionCount = 0;
    }

    public void EditMass()
    {
        astralBody.Mass = massSlider.value;
    }
    
}
