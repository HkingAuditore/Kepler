using GameManagers;
using Quiz;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI.Quiz
{
    public class AstralBodyEditorQuizUI : AstralBodyEditorUI
    {
        [Header("Quiz Setting")] public Toggle isThisTarget;

        // protected override void Awake()
        // {
        // }

        protected override void OnEnable()
        {
            varLineUis.ForEach(v => v.target = (QuizAstralBody) astralBody);
            forceUI.astralBody = astralBody;
            forceUI.gameObject.SetActive(true);
            velocityUI.astralBody = astralBody;
            velocityUI.gameObject.SetActive(true);
            normalPanel.SetActive(false);
            positionEditorUI.editingTarget = astralBody;
            positionEditorUI.gameObject.SetActive(true);
            editorPanel.SetActive(true);
        }


        public override void OnAstralBodySet()
        {
            isThisTarget.isOn = ReferenceEquals(GameManager.GetGameManager.quizBase.target, (QuizAstralBody) astralBody);
            // isThisTarget.interactable = !isThisTarget.isOn;
            isThisTarget.interactable = false;
        }

        public override void SetVelocityInCircle()
        {
            base.SetVelocityInCircle();
            ((QuizAstralBody) astralBody).UpdateHighCost();
        }

        public void SetToTarget()
        {
            ((QuizEditor) GameManager.GetGameManager.quizBase).SetTarget((QuizAstralBody) astralBody);
        }
    }
}