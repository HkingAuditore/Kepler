using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarStyleSettingUI : MonoBehaviour
{
    public  AstralBody       astralBody;
    private List<GameObject> _meshList;

    private void Start()
    {
        this._meshList = GameManager.GetGameManager.meshList;
    }

    public void ChangeStyle(int index)
    {
        astralBody.meshNum = index;
    }
}
