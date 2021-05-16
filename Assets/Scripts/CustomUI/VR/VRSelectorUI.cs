using System;
using System.Collections.Generic;
using CustomCamera;
using CustomPostProcessing;
using CustomUI.AstralBodyEditor;
using GameManagers;
using SpacePhysic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI.VR
{

    
    public class VRSelectorUI : MonoBehaviour
    {
        public List<OutlineCatcher> outlineCatchers;
        public Transform            pointerTransform;
        public GameObject           informationPanel;
        public Transform            faceCamera;

        [Header("星球信息")] public List<StarInfoMap> starInfoMaps;
        public                  Text              starNameText;
        public                  Text              starMassText;
        public                  Text              starRadiusText;
        public                  Text              starGravityText;
        public                  Text              starDayTimeText;
        public                  Text              starYearTimeText;


        // public  List<GameObject> selectedGameObjects = new List<GameObject>();
        public  GameObject       selectedGameObject;
        private CameraController _cameraController;
        
        private void Start()
        {
            _cameraController = GameManager.getGameManager.GetMainCameraController();
        }

        public void Update()
        {
            HighlightSelect();
            FocusOn();

        }

        private void HighlightSelect()
        {
            var        ray = new Ray(pointerTransform.position, pointerTransform.forward);
            // Debug.DrawLine(pointerTransform.position, pointerTransform.position + pointerTransform.forward * 1000);
            RaycastHit hitInfo;

            // Debug.DrawRay(ray.origin,ray.direction,Color.green);
            if (Physics.Raycast(ray, out hitInfo, 5000, (1 << 20) | (1 << 23)))
            {
                Debug.Log("Hit!");
                selectedGameObject = hitInfo.collider.gameObject;
                outlineCatchers.ForEach(c => c.AddTarget(selectedGameObject));
                // }
            }
            else
            {
                if (selectedGameObject != null)
                {
                    outlineCatchers.ForEach(c => c.RemoveTarget(selectedGameObject));
                    selectedGameObject = null;
                }

                // selectedGameObjects.ForEach(o => outlineCatcher.RemoveTarget(o));
                // selectedGameObjects.Clear();
            }

            // Debug.DrawLine(ray.origin, hitInfo.point);
        }

        private bool _showInfo = false;

        private void FocusOn()
        {
            if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && selectedGameObject != null)
            {
                var        ray = new Ray(pointerTransform.position, pointerTransform.forward);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 5000, (1 << 24),QueryTriggerInteraction.Collide))
                {
                    informationPanel.transform.position = hitInfo.point;
                    informationPanel.transform.forward  = informationPanel.transform.position - faceCamera.transform.position;

                    if(FindStarInfo(selectedGameObject) !=null)
                    {
                        var info = FindStarInfo(selectedGameObject);
                        starNameText.text     = "【"        + info.starName + "】";
                        starMassText.text     = "质量："      + info.starMass;
                        starRadiusText.text   = "半径："      + info.starRadius;
                        starGravityText.text  = "表面重力加速度：" + info.starGravity;
                        starDayTimeText.text  = "自转周期："    + info.starDayTime;
                        starYearTimeText.text = "公转周期："    + info.starYearTime;
                    }else{
                        starNameText.text     = "【"        + "小行星" + "】";
                        starMassText.text     = "质量："      + "未知";
                        starRadiusText.text   = "半径："      + "未知";
                        starGravityText.text  = "表面重力加速度：" + "未知";
                        starDayTimeText.text  = "自转周期："    + "未知";
                        starYearTimeText.text = "公转周期："    + "未知";

                    }
                    informationPanel.SetActive(true);
                    _showInfo = true;
                }

            }
            
            if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && selectedGameObject == null && _showInfo)
            {
                var        ray = new Ray(pointerTransform.position, pointerTransform.forward);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 1000, (1 << 24),QueryTriggerInteraction.Collide))
                {
                    informationPanel.transform.position = hitInfo.point;
                    informationPanel.transform.forward  = informationPanel.transform.position - faceCamera.transform.position;
                    informationPanel.SetActive(true);
                    _showInfo = true;
                }

            }

            
            
            
            if (_showInfo && OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            {
                informationPanel.SetActive(false);
                _showInfo = false;
            }
        }

        private StarInfoMap FindStarInfo(GameObject star)
        {
            return starInfoMaps.Find(info => info.star == star);
        } 
        
    }
}