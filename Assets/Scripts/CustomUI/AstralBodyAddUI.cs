using System.Collections;
using System.Collections.Generic;
using GameManagers;
using SpacePhysic;
using UnityEngine;

namespace CustomUI
{
    public class AstralBodyAddUI : MonoBehaviour
    {
        public AstralBodyPlacementUI astralBodyPlacementUI;
        public AstralBody            placePrefab;
        public GravityTracing        orbits;
        public bool                  isQuizEditMode;
        public List<GameObject>      setActiveList;


        public Transform OrbitCore { get; set; }

        private void Start()
        {
            if (!isQuizEditMode)
            {
                OrbitCore = orbits.transform.Find("Core");
            }
            else
            {
                Debug.Log("To Wait");
                StartCoroutine(WaitForLoad());
            }
        }

        public IEnumerator WaitForLoad()
        {
            // Debug.Log("Wait 4 Core");
            // Debug.Log(GameManager.GetGameManager.quizBase.IsLoadDone);
            yield return new WaitUntil(() => GameManager.GetGameManager.quizBase.isLoadDone);
            OrbitCore = orbits.transform.Find("Core");
            Debug.Log(OrbitCore);
        }


        public void Switch2Placement()
        {
            setActiveList.ForEach(o => o.SetActive(false));
            astralBodyPlacementUI.SetPlacing();
            astralBodyPlacementUI.gameObject.SetActive(true);
        }

        public void Switch2Normal()
        {
            setActiveList.ForEach(o => o.SetActive(true));
            astralBodyPlacementUI.gameObject.SetActive(false);
        }
    }
}