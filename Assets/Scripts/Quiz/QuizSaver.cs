using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace Quiz
{
    public struct QuizBaseStruct
    {
        public List<AstralBodyStructDict> astralBodyStructList;
        public QuizType                   quizType;
    }

    public class QuizSaver : MonoBehaviour
    {
        public                  QuizType    quizType;
        public                  GameObject  cloner;
        private                 string      _xmlPath;
        private static readonly XmlDocument _xmlDoc = new XmlDocument();

        private void Awake()
        {
            Debug.Log("Generate!");
            _xmlPath = Application.dataPath + "/Quiz/";
        }

        public XmlElement ConvertAstralBody2XmlElement(AstralBodyDict astralBodyDict)
        {
            var dict         = _xmlDoc.CreateElement("AstralBodyDict");
            var astTransform = _xmlDoc.CreateElement("Transform");

            //写入坐标
            var pos = _xmlDoc.CreateElement("Position");
            pos.InnerText = astralBodyDict.transform.position.ToString();
            astTransform.AppendChild(pos);

            var astAstralBody = _xmlDoc.CreateElement("AstralBody");
            //写入天体属性
            var mass = _xmlDoc.CreateElement("Mass");
            mass.InnerText = astralBodyDict.astralBody.mass.ToString(CultureInfo.InvariantCulture);
            astAstralBody.AppendChild(mass);
            var density = _xmlDoc.CreateElement("Density");
            density.InnerText = astralBodyDict.astralBody.density.ToString(CultureInfo.InvariantCulture);
            astAstralBody.AppendChild(density);
            var size = _xmlDoc.CreateElement("Size");
            size.InnerText = astralBodyDict.astralBody.originalSize.ToString(CultureInfo.InvariantCulture);
            astAstralBody.AppendChild(size);
            var velocity = _xmlDoc.CreateElement("Velocity");
            velocity.InnerText = astralBodyDict.astralBody.oriVelocity.ToString();
            astAstralBody.AppendChild(velocity);
            var isAffect = _xmlDoc.CreateElement("EnableAffect");
            isAffect.InnerText = astralBodyDict.astralBody.enableAffect.ToString();
            astAstralBody.AppendChild(isAffect);
            var isTracing = _xmlDoc.CreateElement("EnableTracing");
            isTracing.InnerText = astralBodyDict.astralBody.enableTracing.ToString();
            astAstralBody.AppendChild(isTracing);
            astAstralBody.SetAttribute("IsCore", (astralBodyDict.astralBody.GetGameObject().name == "Core").ToString());

            dict.SetAttribute("IsTarget", astralBodyDict.isTarget.ToString());
            dict.AppendChild(astTransform);
            dict.AppendChild(astAstralBody);
            return dict;
        }

        public XmlDocument ConvertOrbit2Xml(List<AstralBodyDict> astOrbit, QuizType quizType)
        {
            var astList = _xmlDoc.CreateElement("AstralBodyList");
            foreach (var astralBodyDict in astOrbit) astList.AppendChild(ConvertAstralBody2XmlElement(astralBodyDict));

            astList.SetAttribute("QuizType", quizType.ToString());
            _xmlDoc.AppendChild(astList);
            return _xmlDoc;
        }

        public void SaveXml(XmlDocument doc, string fileName)
        {
            var path = _xmlPath + fileName + ".xml";
            if (!File.Exists(path))
                doc.Save(path);
            else
                throw new QuizSaverException("文件已存在");
            AssetDatabase.Refresh();
        }

        public XmlDocument LoadXml(string fileName)
        {
            var path = _xmlPath + fileName + ".xml";
            if (File.Exists(path))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                AssetDatabase.Refresh();
                return xmlDoc;
            }

            Debug.Log(_xmlPath);
            throw new QuizSaverException(path + "文件不存在");
        }

        private Vector3 ConvertString2Vector3(string str)
        {
            var vec = str.Trim(' ').Trim('(').Trim(')').Split(',');
            return new Vector3(float.Parse(vec[0]), float.Parse(vec[1]), float.Parse(vec[2]));
        }

        public QuizBaseStruct ConvertXml2QuizBase(XmlDocument xmlDoc)
        {
            var quizBaseStruct = new QuizBaseStruct();
            var list           = new List<AstralBodyStructDict>();
            var astralBodyList = xmlDoc.SelectSingleNode("AstralBodyList").ChildNodes;
            foreach (XmlElement astralBodyElement in astralBodyList)
            {
                var astStruct = new AstralBodyStructDict();

                //Position
                astStruct.position =
                    ConvertString2Vector3(astralBodyElement.GetElementsByTagName("Transform")[0].InnerText);

                //AstralBody
                XmlNode astralBodyXmlNode = astralBodyElement.GetElementsByTagName("AstralBody")[0];
                astStruct.mass          = int.Parse(astralBodyXmlNode.ChildNodes[0].InnerText);
                astStruct.density       = int.Parse(astralBodyXmlNode.ChildNodes[1].InnerText);
                astStruct.originalSize  = float.Parse(astralBodyXmlNode.ChildNodes[2].InnerText);
                astStruct.oriVelocity   = ConvertString2Vector3(astralBodyXmlNode.ChildNodes[3].InnerText);
                astStruct.enableAffect  = bool.Parse(astralBodyXmlNode.ChildNodes[4].InnerText);
                astStruct.enableTracing = bool.Parse(astralBodyXmlNode.ChildNodes[5].InnerText);
                // astStruct.isCore        = bool.Parse(astralBodyXmlNode.Attributes["IsTarget"].Value);
                astStruct.isTarget      = bool.Parse(astralBodyElement.GetAttribute("IsTarget"));

                list.Add(astStruct);
            }

            quizBaseStruct.astralBodyStructList = list;
            quizBaseStruct.quizType =
                (QuizType) Enum.Parse(typeof(QuizType), xmlDoc.SelectSingleNode("AstralBodyList").Attributes[0].Value);

            return quizBaseStruct;
        }
    }
}