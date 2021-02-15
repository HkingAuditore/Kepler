using System;
using System.Collections.Generic;
using CustomPostProcessing;
using UnityEngine;

namespace UI
{
    public class SelectorUI : MonoBehaviour
    {
        public OutlineCatcher outlineCatcher;
        private List<GameObject> selectedGameObjects = new List<GameObject>();
        public void Update()
        {
            Ray ray = GameManager.GetGameManager.GetMainCamera().ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            // Debug.DrawRay(ray.origin,ray.direction,Color.green);
            if(Physics.Raycast(ray,out hitInfo,500))
            {
                // Debug.Log("Hit!");
                    if (!selectedGameObjects.Contains(hitInfo.collider.gameObject) && (hitInfo.collider.gameObject.CompareTag("AstralBody")))
                    {
                        outlineCatcher.AddTarget(hitInfo.collider.gameObject);
                        selectedGameObjects.Add(hitInfo.collider.gameObject);

                    }

            }
            else
            {
                selectedGameObjects.ForEach((o => outlineCatcher.RemoveTarget(o)));
                selectedGameObjects.Clear();
            }
        }
    }
}
