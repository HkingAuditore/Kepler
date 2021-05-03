using GameManagers;
using UnityEngine;

namespace CustomUI.Satellite
{
    public class SatelliteUI : MonoBehaviour
    {
        public void BackToLabMode()
        {
            GlobalTransfer.getGlobalTransfer.LoadSceneInLoadingScene("LabMode");
        }
    }
}
