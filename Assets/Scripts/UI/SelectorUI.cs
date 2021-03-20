using System.Collections.Generic;
using CustomPostProcessing;
using UnityEngine;

namespace UI
{
    public class SelectorUI : MonoBehaviour
    {
        public OutlineCatcher     outlineCatcher;
        public AstralBodyEditorUI astralBodyEditorUI;

        // public  List<GameObject> selectedGameObjects = new List<GameObject>();
        public  GameObject       selectedGameObject = null;
        private CameraController _cameraController;
        private Camera           _mainCamera;
        private bool             _isLocked;

        public bool isLocked
        {
            get => _isLocked;
            set => _isLocked = value;
        }

        private void Start()
        {
            _mainCamera       = GameManager.GetGameManager.mainCamera;
            _cameraController = GameManager.GetGameManager.GetMainCameraController();
        }

        public void Update()
        {
            if (!isLocked)
            {
                HighlightSelect();
                FocusOn();
            }
            else
            {
                CancelFocus();
            }
        }

        private void HighlightSelect()
        {
            var        ray = _mainCamera .ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            
            // Debug.DrawRay(ray.origin,ray.direction,Color.green);
            if (Physics.Raycast(ray, out hitInfo, 1000, 1 <<20 |1 <<23))
            {
                // Debug.Log("Hit!");
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
                if(selectedGameObject!=null)
                {
                    outlineCatcher.RemoveTarget(selectedGameObject);
                    selectedGameObject = null;
                }
                // selectedGameObjects.ForEach(o => outlineCatcher.RemoveTarget(o));
                // selectedGameObjects.Clear();
            }

            // Debug.DrawLine(ray.origin, hitInfo.point);
        }

        public void FocusOn()
        {
            if (Input.GetMouseButtonDown(0) && selectedGameObject!=null)
            {
                _cameraController.FocusOn(selectedGameObject.transform);
                isLocked                      = true;
                _cameraController.IsFollowing = true;
                astralBodyEditorUI.astralBody = selectedGameObject.GetComponent<AstralBody>();
                astralBodyEditorUI.gameObject.SetActive(true);
                astralBodyEditorUI.enabled = true;
            }
        }

        public void CancelFocus()
        {
            if (Input.GetMouseButtonDown(1))
            {
                isLocked = false;
                _cameraController.ExitFocus();
                _cameraController.IsFollowing = false;
                astralBodyEditorUI.enabled    = false;
                astralBodyEditorUI.gameObject.SetActive(false);
            }
        }
    }
}