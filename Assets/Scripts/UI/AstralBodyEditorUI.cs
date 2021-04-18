using System;
using System.Collections.Generic;
using SpacePhysic;
using UnityEngine;
using UnityEngine.UI;

public class AstralBodyEditorUI : MonoBehaviour
{
    public bool           isEnableEdit        = true;
    public bool           isEnableEditorPanel = true;

    public AstralBody astralBody
    {
        get => _astralBody;
        set
        {
            _astralBody = value;
            OnAstralBodySet();
        }
    }

    public virtual void OnAstralBodySet()
    {
        
    }

    public GravityTracing gravityTracing;
    
    public  PositionEditorUI positionEditorUI;
    public  VelocityEditorUI velocityEditorUI;
    public  GameObject       normalPanel;
    public  VectorUI         forceUI;
    public  VectorUI         velocityUI;
    public  List<LengthUI>   lengthUIList;
    public  OrbitPanelUI     orbitPanelUI;
    public  GameObject       editorPanel;

    [Header("Var Line List")] 
    public List<VarLineUI> varLineUis;

    public  GameObject         mainPanel;
    public  StarStyleSettingUI styleSheetPanel;
    
    private Text               _massText;
    private AstralBody         _astralBody;
    

    protected virtual void OnEnable()
    {
        varLineUis.ForEach(v => v.target = (AstralBody)this.astralBody);
        forceUI.astralBody = astralBody;
        forceUI.gameObject.SetActive(true);
        velocityUI.astralBody = astralBody;
        velocityUI.gameObject.SetActive(true);
        if (isEnableEdit)
        {
            normalPanel.SetActive(false);
            if (isEnableEditorPanel)
            {
                positionEditorUI.editingTarget = astralBody;
                positionEditorUI.gameObject.SetActive(true);

                editorPanel.SetActive(true);
            }
            // InitMassEditor();

            lengthUIList.ForEach(l =>
                                 {
                                     l.astralBody = astralBody;
                                 });
            lengthUIList[0]?.transform.parent.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        forceUI.gameObject.SetActive(false);
        velocityUI.gameObject.SetActive(false);
        this.StyleSheetToMain();
        CloseConicSectionPanel();
        if (isEnableEdit)
        {
            positionEditorUI.gameObject.SetActive(false);
            positionEditorUI.editingTarget = null;
            normalPanel.SetActive(true);
            try
            {
                lengthUIList[0]?.transform.parent.gameObject.SetActive(false);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // throw;
            }
        }


    }

    // private void InitMassEditor()
    // {
    //     _massText.text = "";
    //     Debug.Log(astralBody.mass);
    //     var tmpMass = astralBody.mass;
    //     massSlider.minValue = (int) (tmpMass / 10);
    //     massSlider.maxValue = (int) (tmpMass * 10);
    //     massSlider.value    = tmpMass;
    //
    //     _massText.text = astralBody.Mass + " kg";
    // }

    // public void EditMass()
    // {
    //     astralBody.Mass = massSlider.value;
    //     _massText.text  = astralBody.Mass + " kg";
    // }

    public void EditVelocity()
    {
        velocityEditorUI.editingTarget = astralBody;
        velocityEditorUI.gameObject.SetActive(true);
    }

    public void OpenConicSectionPanel()
    {
        try
        {
            orbitPanelUI.astralBody = this.astralBody;
            orbitPanelUI.gameObject.SetActive(true);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    public void CloseConicSectionPanel()
    {
        try
        {
            orbitPanelUI.gameObject.SetActive(false);

        }
        catch
        {
            //ignore
        }
    }

    

    public virtual void SetVelocityInCircle()
    {
        this.astralBody.SetCircleVelocity();
    }
    
    public void ClosePanel()
    {
        this.gameObject.SetActive(false);
        GameManager.GetGameManager.GetMainCameraController().ExitFocus();
    }

    public void MainToStyleSheet()
    {
        mainPanel.SetActive(false);
        styleSheetPanel.astralBody = this.astralBody;
        styleSheetPanel.gameObject.SetActive(true);
    }
    
    public void StyleSheetToMain()
    {
        try
        {
            styleSheetPanel.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

        }
        mainPanel.SetActive(true);
    }
}
