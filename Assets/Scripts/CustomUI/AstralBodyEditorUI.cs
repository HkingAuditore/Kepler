using System;
using System.Collections.Generic;
using CustomUI.Quiz;
using GameManagers;
using SpacePhysic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class AstralBodyEditorUI : MonoBehaviour
    {
        public GameObject editorPanel;
        public VectorUI   forceUI;

        public GravityTracing gravityTracing;
        public bool           isEnableEdit        = true;
        public bool           isEnableEditorPanel = true;
        public List<LengthUI> lengthUIList;

        public GameObject   mainPanel;
        public GameObject   normalPanel;
        public OrbitPanelUI orbitPanelUI;

        public PositionEditorUI   positionEditorUI;
        public StarStyleSettingUI styleSheetPanel;

        [Header("Var Line List")] public List<VarLineUI>  varLineUis;
        public                           VelocityEditorUI velocityEditorUI;
        public                           VectorUI         velocityUI;
        private                          AstralBody       _astralBody;

        private Text _massText;

        public AstralBody astralBody
        {
            get => _astralBody;
            set
            {
                _astralBody = value;
                OnAstralBodySet();
            }
        }


        protected virtual void OnEnable()
        {
            varLineUis.ForEach(v => v.target = astralBody);
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

                lengthUIList.ForEach(l => { l.astralBody = astralBody; });
                lengthUIList[0]?.transform.parent.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            forceUI.gameObject.SetActive(false);
            velocityUI.gameObject.SetActive(false);
            StyleSheetToMain();
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

        protected virtual void OnAstralBodySet()
        {
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
                orbitPanelUI.astralBody = astralBody;
                orbitPanelUI.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void CloseConicSectionPanel()
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
            astralBody.SetCircleVelocity();
        }

        public void ClosePanel()
        {
            gameObject.SetActive(false);
            GameManager.getGameManager.GetMainCameraController().ExitFocus();
        }

        public void MainToStyleSheet()
        {
            mainPanel.SetActive(false);
            styleSheetPanel.astralBody = astralBody;
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
}