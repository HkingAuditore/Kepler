using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AstralBodyEditorUI : MonoBehaviour
{
    public AstralBody astralBody;

    public Slider massSlider;
    public PositionEditorUI positionEditorUI;
    public VelocityEditorUI velocityEditorUI;
    public GameObject normalPanel;
    public ForceUI forceUI;



    private void OnEnable()
    {
        forceUI.astralBody = this.astralBody;
        forceUI.gameObject.SetActive(true);
        positionEditorUI.editingTarget = this.astralBody.transform;
        positionEditorUI.gameObject.SetActive(true);
        normalPanel.SetActive(false);
    }

    private void OnDisable()
    {
        forceUI.gameObject.SetActive(false);
        positionEditorUI.gameObject.SetActive(false);
        positionEditorUI.editingTarget = null;
        normalPanel.SetActive(true);
    }

    public void EditMass()
    {
        astralBody.Mass = massSlider.value;
    }

    public void EditVelocity()
    {
        velocityEditorUI.editingTarget = this.astralBody;
        velocityEditorUI.gameObject.SetActive(true);
    }
    
}
