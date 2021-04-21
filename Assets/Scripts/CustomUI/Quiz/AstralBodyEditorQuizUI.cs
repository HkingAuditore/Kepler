using CustomUI.AstralBodyEditor;
using GameManagers;
using Quiz;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI.Quiz
{
    public class AstralBodyEditorQuizUI : AstralBodyEditorUI
    {
        /// <summary>
        ///     是否作为目标
        /// </summary>
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

        /// <summary>
        ///     星体初始目标设置
        /// </summary>
        protected override void OnAstralBodySet()
        {
            isThisTarget.isOn =
                ReferenceEquals(GameManager.getGameManager.quizBase.target, (QuizAstralBody) astralBody);
            // isThisTarget.interactable = !isThisTarget.isOn;
            isThisTarget.interactable = false;
        }

        /// <summary>
        ///     设置环绕速度
        /// </summary>
        public override void SetVelocityInCircle()
        {
            base.SetVelocityInCircle();
            ((QuizAstralBody) astralBody).UpdateHighCost();
        }

        /// <summary>
        ///     设为问题目标
        /// </summary>
        public void SetToTarget()
        {
            ((QuizEditor) GameManager.getGameManager.quizBase).SetTarget((QuizAstralBody) astralBody);
        }
    }
}