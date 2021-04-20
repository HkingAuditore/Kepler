using System.Collections.Generic;
using GameManagers;
using SpacePhysic;
using UnityEngine;

namespace CustomUI
{
    public class StarStyleSettingUI : MonoBehaviour
    {
        public  AstralBody       astralBody;
        private List<GameObject> _meshList;

        private void Start()
        {
            _meshList = GameManager.getGameManager.meshList;
        }

        public void ChangeStyle(int index)
        {
            astralBody.meshNum = index;
        }
    }
}