using System.Collections;
using System.Collections.Generic;
using GameManagers;
using SpacePhysic;
using UnityEngine;

namespace CustomUI.AstralBodyEditor
{
    public class AstralBodyAddUI : MonoBehaviour
    {
        public AstralBodyPlacementUI astralBodyPlacementUI;
        public bool                  isQuizEditMode;
        public GravityTracing        orbits;
        public AstralBody            placePrefab;
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

        private IEnumerator WaitForLoad()
        {
            // Debug.Log("Wait 4 Core");
            // Debug.Log(GameManager.GetGameManager.quizBase.IsLoadDone);
            yield return new WaitUntil(() => GameManager.getGameManager.quizBase.isLoadDone);
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