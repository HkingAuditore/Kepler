using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Quiz;
using UnityEngine;

public class QuizListUI : MonoBehaviour
{
    public List<QuizLineUI> quizLineUis = new List<QuizLineUI>();
    public RectTransform    content;
    public QuizLineUI       linePrefab;
    public float            offset;
    private void Start()
    {
        GenerateLines();
    }

    private void GenerateLines()
    {
        var oriPos = new Vector3(content.position.x, 
                                 content.position.y,
                                 content.position.z);
        List<string>      fileNames = new List<string>();
        List<XmlDocument> xmlList   = QuizSaver.GetQuizFiles(ref fileNames);
        List<QuizBaseStruct> quizBaseStructs = (from xmlDocument in xmlList
                                                select QuizSaver.ConvertXml2QuizBase(xmlDocument, fileNames[xmlList.IndexOf(xmlDocument)])).ToList();
        content.sizeDelta = new Vector2(content.sizeDelta.x, quizBaseStructs.Count * offset * 0.5f);
        for (var i = 0; i < quizBaseStructs.Count; i++)
        {
            var quizStruct = quizBaseStructs[i];
            var line = Instantiate(linePrefab, oriPos - new Vector3(0, (i + 1) * offset, 0), Quaternion.Euler(0, 0, 0), content);
            quizLineUis.Add(line);
            line.name = quizStruct.quizName;
            line.gameObject.SetActive(true);
        }
    }
}
