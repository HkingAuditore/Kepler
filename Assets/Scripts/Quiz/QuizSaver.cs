using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using SpacePhysic;
using UnityEngine;
using System.IO;

namespace Quiz
{
    public struct QuizBaseStruct
    {
        public List<AstralBodyDict> astralBodyDictList;
        public QuizType quizType;
    }

    public class QuizSaver : MonoBehaviour
    {
        public QuizType quizType;
        public GameObject cloner;
        private string _xmlPath;
        private static XmlDocument _xmlDoc = new XmlDocument();

        private void Awake()
        {
            Debug.Log("Generate!");
            _xmlPath = Application.dataPath + "/Quiz/";
        }

        public XmlElement ConvertAstralBody2XmlElement(AstralBodyDict astralBodyDict)
        {
            XmlElement dict = _xmlDoc.CreateElement("AstralBodyDict");
            XmlElement astTransform = _xmlDoc.CreateElement("Transform");
            
            //写入坐标
            var pos = _xmlDoc.CreateElement("Position");
            pos.InnerText = astralBodyDict.transform.position.ToString();
            astTransform.AppendChild(pos);

            XmlElement astAstralBody = _xmlDoc.CreateElement("AstralBody");
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
            
            dict.SetAttribute("IsTarget", astralBodyDict.isTarget.ToString());
            dict.AppendChild(astTransform);
            dict.AppendChild(astAstralBody);
            return dict;
        }

        public XmlDocument ConvertOrbit2Xml(List<AstralBodyDict> astOrbit,QuizType quizType)
        {
            XmlElement astList = _xmlDoc.CreateElement("AstralBodyList");
            foreach (AstralBodyDict astralBodyDict in astOrbit)
            {
                astList.AppendChild(ConvertAstralBody2XmlElement(astralBodyDict));
            }

            astList.SetAttribute("QuizType", quizType.ToString());
            _xmlDoc.AppendChild(astList);
            return _xmlDoc;
        }

        public void SaveXml(XmlDocument doc,string fileName)
        {
            string path = _xmlPath + fileName + ".xml";
            if (!File.Exists(path))
            {
                doc.Save(path);
                
            }
            else
            {
                throw new QuizSaverException("文件已存在");
            }
            UnityEditor.AssetDatabase.Refresh();
        }

        public XmlDocument LoadXml(String fileName)
        {
            string path = _xmlPath + fileName + ".xml";
            if (File.Exists(path))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                UnityEditor.AssetDatabase.Refresh();
                return xmlDoc;
            }
            else
            {
                Debug.Log(_xmlPath);
                throw new QuizSaverException(path  + "文件不存在");
            }
        }

        private Vector3 ConvertString2Vector3(String str)
        {
            string[] vec = str.Trim(' ').Trim('(').Trim(')').Split(',');
            return new Vector3(float.Parse(vec[0]), float.Parse(vec[1]), float.Parse(vec[2]));
        }
        public QuizBaseStruct ConvertXml2QuizBase(XmlDocument xmlDoc)
        {
            QuizBaseStruct quizBaseStruct = new QuizBaseStruct();
            List<AstralBodyDict> list = new List<AstralBodyDict>();
            XmlNodeList astralBodyList = xmlDoc.SelectSingleNode("AstralBodyList").ChildNodes;
            foreach (XmlElement astralBodyElement in astralBodyList)
            {
                AstralBodyDict dict = new AstralBodyDict();
                GameObject tmpCloner = Instantiate(cloner) as GameObject;
                
                //Position
                dict.transform = tmpCloner.transform;
                dict.transform.position = ConvertString2Vector3(astralBodyElement.GetElementsByTagName("Transform")[0].InnerText);
                
                //AstralBody
                var astralBodyXmlNode = astralBodyElement.GetElementsByTagName("AstralBody")[0];
                dict.astralBody = tmpCloner.GetComponent<AstralBody>();
                dict.astralBody.mass = int.Parse(astralBodyXmlNode.ChildNodes[0].InnerText);
                dict.astralBody.density = int.Parse(astralBodyXmlNode.ChildNodes[1].InnerText);
                dict.astralBody.originalSize = float.Parse(astralBodyXmlNode.ChildNodes[2].InnerText);
                dict.astralBody.oriVelocity =ConvertString2Vector3(astralBodyXmlNode.ChildNodes[3].InnerText);
                dict.astralBody.enableAffect = bool.Parse(astralBodyXmlNode.ChildNodes[4].InnerText);
                dict.astralBody.enableAffect = bool.Parse(astralBodyXmlNode.ChildNodes[5].InnerText);
                dict.isTarget = bool.Parse(astralBodyElement.GetAttribute("IsTarget"));
                
                list.Add(dict);
            }

            quizBaseStruct.astralBodyDictList = list;
            quizBaseStruct.quizType = (QuizType)System.Enum.Parse(typeof(QuizType), xmlDoc.SelectSingleNode("AstralBodyList").Attributes[0].Value);
            
            return quizBaseStruct;
        }
    }
}
