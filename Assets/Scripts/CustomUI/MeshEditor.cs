using System;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class MeshEditor : MonoBehaviour
    {
        public AstralBodyEditorUI astralBodyEditorUI;
        public InputField         inputField;


        public void ChangeMesh()
        {
            var index = Convert.ToInt32(inputField.text);
            astralBodyEditorUI.astralBody.meshNum = index;
        }
    }
}