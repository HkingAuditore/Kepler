using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeshEditor : MonoBehaviour
{
    public InputField         inputField;
    public AstralBodyEditorUI astralBodyEditorUI;
    

    public void ChangeMesh()
    {
        int index = Convert.ToInt32( inputField.text);
        astralBodyEditorUI.astralBody.meshNum = index;
    }
}
