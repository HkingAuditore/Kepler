using System.Collections.Generic;
using CustomPostProcessing;
using UnityEngine;

namespace UI
{
    public class SelectorUI : MonoBehaviour
    {
        public OutlineCatcher     outlineCatcher;
        public AstralBodyEditorUI astralBodyEditorUI;

        private readonly List<GameObject> selectedGameObjects = new List<GameObject>();
        private          CameraController _cameraController;
        private          bool             _isLocked;

        private void Start()
        {
            _cameraController = GameManager.GetGameManager.GetMainCameraController();
        }

        public void Update()
        {
            if (!_isLocked)
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
            var        ray = _cameraController.GetMainCamera().ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            
            // Debug.DrawRay(ray.origin,ray.direction,Color.green);
            if (Physics.Raycast(ray, out hitInfo, 500))
            {
                // Debug.Log("Hit!");
                Debug.DrawLine(ray.origin, hitInfo.point);
                if (!selectedGameObjects.Contains(hitInfo.collider.gameObject) &&
                    hitInfo.collider.gameObject.CompareTag("AstralBody"))
                {
                    outlineCatcher.AddTarget(hitInfo.collider.gameObject);
                    selectedGameObjects.Add(hitInfo.collider.gameObject);
                }
            }
            else
            {
                selectedGameObjects.ForEach(o => outlineCatcher.RemoveTarget(o));
                selectedGameObjects.Clear();
            }
        }

        public void FocusOn()
        {
            if (Input.GetMouseButtonDown(0) && selectedGameObjects.Count > 0)
            {
                _cameraController.FocusOn(selectedGameObjects[0].transform);
                _isLocked                     = true;
                _cameraController.IsFollowing = true;
                astralBodyEditorUI.astralBody = selectedGameObjects[0].GetComponent<AstralBody>();
                astralBodyEditorUI.gameObject.SetActive(true);
                astralBodyEditorUI.enabled = true;
            }
        }

        public void CancelFocus()
        {
            if (Input.GetMouseButtonDown(1))
            {
                _isLocked = false;
                _cameraController.ExitFocus();
                _cameraController.IsFollowing = false;
                astralBodyEditorUI.enabled    = false;
                astralBodyEditorUI.gameObject.SetActive(false);
            }
        }
    }
}