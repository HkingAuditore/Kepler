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
    public VectorUI forceUI;
    public VectorUI velocityUI;

    private Text _massText;

    private void Awake()
    {
        _massText = massSlider.transform.parent.Find("MassText").GetComponent<Text>();
    }

    private void OnEnable()
    {
        
        forceUI.astralBody = this.astralBody;
        forceUI.gameObject.SetActive(true);
        velocityUI.astralBody = this.astralBody;
        velocityUI.gameObject.SetActive(true);
        positionEditorUI.editingTarget = this.astralBody.transform;
        positionEditorUI.gameObject.SetActive(true);
        normalPanel.SetActive(false);
        InitMassEditor();
    }

    private void OnDisable()
    {
        forceUI.gameObject.SetActive(false);
        velocityUI.gameObject.SetActive(false);
        positionEditorUI.gameObject.SetActive(false);
        positionEditorUI.editingTarget = null;
        normalPanel.SetActive(true);
    }

    private void InitMassEditor()
    {
        _massText.text = "";
        Debug.Log(astralBody.mass);
        var tmpMass = astralBody.mass;
        massSlider.minValue = (int) (tmpMass / 10);
        massSlider.maxValue = (int) (tmpMass * 10);
        massSlider.value = tmpMass;
        
        _massText.text = astralBody.Mass + " kg";
    }
    public void EditMass()
    {
        astralBody.Mass = massSlider.value;
        _massText.text = astralBody.Mass + " kg";
    }

    public void EditVelocity()
    {
        velocityEditorUI.editingTarget = this.astralBody;
        velocityEditorUI.gameObject.SetActive(true);
    }
    
}
