using System.Collections;
using System.Collections.Generic;
using Quiz;
using UnityEngine;
using UnityEngine.UI;

public class AstralBodyEditorQuizUI : AstralBodyEditorUI
{


    [Header("Quiz Setting")] public Toggle isThisTarget;


    public override void OnAstralBodySet()
    {
        isThisTarget.isOn         = ReferenceEquals(GameManager.GetGameManager.quizBase.target, (QuizAstralBody)this.astralBody);
        // isThisTarget.interactable = !isThisTarget.isOn;
        isThisTarget.interactable = false;
    }

    // protected override void Awake()
    // {
    // }

    protected override void OnEnable()
    {
        varLineUis.ForEach(v => v.target = (QuizAstralBody)this.astralBody);
        forceUI.astralBody = astralBody;
        forceUI.gameObject.SetActive(true);
        velocityUI.astralBody = astralBody;
        velocityUI.gameObject.SetActive(true);
        normalPanel.SetActive(false);
        positionEditorUI.editingTarget = astralBody;
        positionEditorUI.gameObject.SetActive(true);
        editorPanel.SetActive(true);

    }

    public override void SetVelocityInCircle()
    {
        base.SetVelocityInCircle();
        ((QuizAstralBody) this.astralBody).UpdateHighCost();
    }

    public void SetToTarget()
    {
        ((QuizEditor) GameManager.GetGameManager.quizBase).SetTarget((QuizAstralBody)this.astralBody);
    }


}
