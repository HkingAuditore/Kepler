using CustomCamera;
using CustomPostProcessing;
using GameManagers;
using SpacePhysic;
using UnityEngine;

namespace CustomUI
{
    public class SelectorUI : MonoBehaviour
    {
        public bool               _isLocked;
        public AstralBodyEditorUI astralBodyEditorUI;
        public OutlineCatcher     outlineCatcher;

        // public  List<GameObject> selectedGameObjects = new List<GameObject>();
        public  GameObject       selectedGameObject;
        private CameraController _cameraController;
        private Camera           _mainCamera;

        public bool isLocked
        {
            get => _isLocked;
            set => _isLocked = value;
        }

        private void Start()
        {
            _mainCamera       = GameManager.getGameManager.mainCamera;
            _cameraController = GameManager.getGameManager.GetMainCameraController();
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
            var        ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            // Debug.DrawRay(ray.origin,ray.direction,Color.green);
            if (Physics.Raycast(ray, out hitInfo, 1000, (1 << 20) | (1 << 23)))
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

        private void FocusOn()
        {
            if (Input.GetMouseButtonDown(0) && selectedGameObject != null)
            {
                _cameraController.FocusOn(selectedGameObject.transform);
                isLocked                      = true;
                _cameraController.IsFollowing = true;
                astralBodyEditorUI.astralBody = selectedGameObject.GetComponent<AstralBody>();
                astralBodyEditorUI.gameObject.SetActive(true);
                astralBodyEditorUI.enabled = true;
            }
        }

        private void CancelFocus()
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