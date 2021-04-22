using System.Collections.Generic;
using System.Linq;
using Quiz;
using SpacePhysic;
using UnityEngine;
using XmlSaver;

namespace CustomUI.Lab
{
    public class LabSceneListUI : MonoBehaviour
    {
        public RectTransform    content;
        public LabSceneLineUI       linePrefab;
        public float            offset;
        public List<LabSceneLineUI> scenesLineUis = new List<LabSceneLineUI>();

        private void Start()
        {
            GenerateLines();
        }

        private void GenerateLines()
        {
            var oriPos = new Vector3(content.position.x,
                                     content.position.y,
                                     content.position.z);
            Debug.Log(oriPos);
            var fileNames = new List<string>();
            var xmlList   = XmlSaver.XmlSaver<AstralBody>.GetFiles(ref fileNames);
            var sceneQuizStruct = (from xmlDocument in xmlList
                                   select XmlSaver.XmlSaver<AstralBody>.ConvertXml2SceneBase(xmlDocument,
                                                                                             fileNames[xmlList.IndexOf(xmlDocument)]))
               .ToList();
            content.sizeDelta = new Vector2(content.sizeDelta.x, sceneQuizStruct.Count * offset * 0.5f);
            for (var i = 0; i < sceneQuizStruct.Count; i++)
            {
                SceneBaseStruct<AstralBody> quizStruct = sceneQuizStruct[i];
                // Debug.Log(oriPos - new Vector3(0, (i + 1) * offset, 0));

                var line = Instantiate(linePrefab, oriPos - new Vector3(0, (i + 1) * offset, 0),
                                       Quaternion.Euler(0, 0, 0),
                                       content);
                var rect = line.GetComponent<RectTransform>();
                rect.anchoredPosition3D = new Vector3(0, rect.localPosition.y, 0);
                scenesLineUis.Add(line);
                line.sceneStruct = quizStruct;
                line.name       = quizStruct.sceneName;
                line.labSceneListUI = this;
                line.gameObject.SetActive(true);
            }
        }

    }
}
