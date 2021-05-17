using System.Collections.Generic;
using CustomCamera;
using CustomPostProcessing;
using CustomUI.VR;
using GameManagers;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class FreeSelectorUI : MonoBehaviour
    {
        public OutlineCatcher outlineCatcher;
        public GameObject     informationPanel;

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
        private Camera           _mainCamera;
        private RectTransform    _infoRect;

        private void Start()
        {
            _mainCamera       = GameManager.getGameManager.mainCamera;
            _cameraController = GameManager.getGameManager.GetMainCameraController();
            _infoRect         = informationPanel.GetComponent<RectTransform>();
        }

        public void Update()
        {
            HighlightSelect();
            FocusOn();
        }

        private void HighlightSelect()
        {
            var        ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            // Debug.DrawRay(ray.origin,ray.direction,Color.green);
            if (Physics.Raycast(ray, out hitInfo, 5000, (1 << 20) | (1 << 23)))
            {
                Debug.Log("Hit!");
                // Debug.DrawLine(ray.origin, hitInfo.point);
                // if (hitInfo.collider.gameObject.CompareTag("AstralBody"))
                // {
                // outlineCatcher.AddTarget(hitInfo.collider.gameObject);
                // selectedGameObjects.Add(hitInfo.collider.gameObject);
                selectedGameObject = hitInfo.collider.gameObject;
                outlineCatcher.AddTarget(selectedGameObject);
                // }
            }
            else
            {
                if (selectedGameObject != null)
                {
                    outlineCatcher.RemoveTarget(selectedGameObject);
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
            if (Input.GetMouseButton(0) && selectedGameObject != null)
            {
                _infoRect.anchoredPosition = new Vector2(Input.mousePosition.x - .5f * Screen.width, Input.mousePosition.y - .5f * Screen.height );
                if (FindStarInfo(selectedGameObject) != null)
                {
                    var info = FindStarInfo(selectedGameObject);
                    starNameText.text     = "【"        + info.starName + "】";
                    starMassText.text     = "质量："      + info.starMass;
                    starRadiusText.text   = "半径："      + info.starRadius;
                    starGravityText.text  = "表面重力加速度：" + info.starGravity;
                    starDayTimeText.text  = "自转周期："    + info.starDayTime;
                    starYearTimeText.text = "公转周期："    + info.starYearTime;
                }
                else
                {
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

            if (Input.GetMouseButton(0) && selectedGameObject == null && _showInfo)
            {
                _infoRect.anchoredPosition = new Vector2(Input.mousePosition.x - .5f * Screen.width, Input.mousePosition.y - .5f * Screen.height );
                informationPanel.SetActive(true);
                _showInfo = true;
            }

            if (_showInfo && Input.GetMouseButtonUp(0))
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