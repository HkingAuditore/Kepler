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
        public string                     quizName;
        public QuizType                   quizType;
    }

    public class QuizSaver : MonoBehaviour
    {
        private    XmlDocument _xmlDoc = new XmlDocument();
        public                  QuizType    quizType;
        public                  GameObject  cloner;

        public static string xmlPath
        {
            get => Application.dataPath + "/Quiz/";
        }
        

        // private void Awake()
        // {
        //     Debug.Log("Generate!");
        //     
        // }

        public XmlElement ConvertAstralBody2XmlElement(AstralBodyDict astralBodyDict)
        {
            var dict         = _xmlDoc.CreateElement("AstralBodyDict");
            var astTransform = _xmlDoc.CreateElement("Transform");
            try
            {
                astralBodyDict.astralBody.UpdateQuizAstralBody();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            //写入坐标
            var pos = _xmlDoc.CreateElement("Position");
            pos.InnerText = astralBodyDict.transform.position.ToString();
            astTransform.AppendChild(pos);

            var astAstralBody = _xmlDoc.CreateElement("AstralBody");
            //写入天体属性
            var mass = _xmlDoc.CreateElement("Mass");
            mass.InnerText = (astralBodyDict.astralBody.realMass).ToString(CultureInfo.InvariantCulture);
            mass.SetAttribute("IsPublic", astralBodyDict.astralBody.isMassPublic.ToString());
            astAstralBody.AppendChild(mass);
            var density = _xmlDoc.CreateElement("Density");
            density.InnerText = astralBodyDict.astralBody.density.ToString(CultureInfo.InvariantCulture);
            astAstralBody.AppendChild(density);
            var size = _xmlDoc.CreateElement("Size");
            size.SetAttribute("IsPublic", astralBodyDict.astralBody.isSizePublic.ToString());

            size.InnerText = astralBodyDict.astralBody.size.ToString(CultureInfo.InvariantCulture);
            astAstralBody.AppendChild(size);
            
            var velocity = _xmlDoc.CreateElement("Velocity");
            velocity.InnerText = astralBodyDict.astralBody.GetVelocity().ToString();
            velocity.SetAttribute("IsPublic", astralBodyDict.astralBody.isVelocityPublic.ToString());
            astAstralBody.AppendChild(velocity);
            
            var isAffect = _xmlDoc.CreateElement("EnableAffect");
            isAffect.InnerText = astralBodyDict.astralBody.enableAffect.ToString();
            astAstralBody.AppendChild(isAffect);
            var isTracing = _xmlDoc.CreateElement("EnableTracing");
            isTracing.InnerText = astralBodyDict.astralBody.enableTracing.ToString();
            astAstralBody.AppendChild(isTracing);
            var affectRadius = _xmlDoc.CreateElement("AffectRadius");
            affectRadius.InnerText = astralBodyDict.astralBody.affectRadius.ToString(CultureInfo.InvariantCulture);
            astAstralBody.AppendChild(affectRadius);

            var period = _xmlDoc.CreateElement("Period");
            period.SetAttribute("IsPublic", astralBodyDict.astralBody.isPeriodPublic.ToString());
            period.InnerText = astralBodyDict.astralBody.period.ToString(CultureInfo.InvariantCulture);
            astAstralBody.AppendChild(period);
            //
            var angularVelocity = _xmlDoc.CreateElement("AngularVelocity");
            angularVelocity.SetAttribute("IsPublic", astralBodyDict.astralBody.isAngularVelocityPublic.ToString());
            angularVelocity.InnerText = astralBodyDict.astralBody.globalAngularVelocity.ToString();
            astAstralBody.AppendChild(angularVelocity);
            //
            var radius = _xmlDoc.CreateElement("Radius");
            radius.SetAttribute("IsPublic", astralBodyDict.astralBody.isRadiusPublic.ToString());
            radius.InnerText = astralBodyDict.astralBody.radius.ToString(CultureInfo.InvariantCulture);
            astAstralBody.AppendChild(radius);
            //
            // var anglePerT = _xmlDoc.CreateElement("AnglePerT");
            // anglePerT.SetAttribute("IsPublic", astralBodyDict.astralBody.isAnglePerTPublic.ToString());
            // anglePerT.InnerText = astralBodyDict.astralBody.anglePerT.ToString(CultureInfo.InvariantCulture);
            // astAstralBody.AppendChild(anglePerT);
            //
            //
            // var distancePerT = _xmlDoc.CreateElement("DistancePerT");
            // distancePerT.SetAttribute("IsPublic", astralBodyDict.astralBody.isDistancePerTPublic.ToString());
            // distancePerT.InnerText = astralBodyDict.astralBody.distancePerT.ToString(CultureInfo.InvariantCulture);
            // astAstralBody.AppendChild(distancePerT);
            
            var t = _xmlDoc.CreateElement("T");
            t.SetAttribute("IsPublic", astralBodyDict.astralBody.isTPublic.ToString());
            t.InnerText = astralBodyDict.astralBody.t.ToString(CultureInfo.InvariantCulture);
            astAstralBody.AppendChild(t);
            
            var gravity = _xmlDoc.CreateElement("Gravity");
            gravity.SetAttribute("IsPublic", astralBodyDict.astralBody.isGravityPublic.ToString());
            gravity.InnerText = astralBodyDict.astralBody.gravity.ToString(CultureInfo.InvariantCulture);
            astAstralBody.AppendChild(gravity);
            

            astAstralBody.SetAttribute("IsCore", (astralBodyDict.isTarget).ToString());
            astAstralBody.SetAttribute("Style", (astralBodyDict.astralBody.meshNum).ToString());

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
            var path = xmlPath + fileName + ".xml";
            if (!File.Exists(path))
                doc.Save(path);
            else
            {
                doc.Save(path);
                AssetDatabase.Refresh();
                throw new QuizSaverException("文件已存在，进行覆盖！");
            }            
            AssetDatabase.Refresh();
        }

        public static XmlDocument LoadXml(string fileName)
        {
            var path = xmlPath + fileName + ".xml";
            if (File.Exists(path))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                AssetDatabase.Refresh();
                return xmlDoc;
            }

            Debug.Log(xmlPath);
            throw new QuizSaverException(path + "文件不存在");
        }

        private static Vector3 ConvertString2Vector3(string str)
        {
            var vec = str.Trim(' ').Trim('(').Trim(')').Split(',');
            return new Vector3(float.Parse(vec[0]), float.Parse(vec[1]), float.Parse(vec[2]));
        }

        public static QuizBaseStruct ConvertXml2QuizBase(XmlDocument xmlDoc,string fileName)
        {
            var quizBaseStruct = new QuizBaseStruct();
            quizBaseStruct.quizName = fileName;
            var list           = new List<AstralBodyStructDict>();
            var astralBodyList = xmlDoc.SelectSingleNode("AstralBodyList").ChildNodes;
            foreach (XmlElement astralBodyElement in astralBodyList)
            {
                var astStruct = new AstralBodyStructDict();

                //Position
                astStruct.position =
                    ConvertString2Vector3(astralBodyElement.GetElementsByTagName("Transform")[0].InnerText);

                //AstralBody
                var astralBodyXmlNode = astralBodyElement.GetElementsByTagName("AstralBody")[0];
                astStruct.mass         = double.Parse(astralBodyXmlNode.ChildNodes[0].InnerText);
                astStruct.isMassPublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[0].Attributes["IsPublic"].Value);
                astStruct.density      = float.Parse(astralBodyXmlNode.ChildNodes[1].InnerText);
                astStruct.originalSize = float.Parse(astralBodyXmlNode.ChildNodes[2].InnerText);
                astStruct.isSizePublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[2].Attributes["IsPublic"].Value);
                astStruct.oriVelocity  = ConvertString2Vector3(astralBodyXmlNode.ChildNodes[3].InnerText);
                astStruct.isVelocityPublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[3].Attributes["IsPublic"].Value);
                astStruct.enableAffect  = bool.Parse(astralBodyXmlNode.ChildNodes[4].InnerText);
                astStruct.enableTracing = bool.Parse(astralBodyXmlNode.ChildNodes[5].InnerText);
                astStruct.affectRadius  = float.Parse(astralBodyXmlNode.ChildNodes[6].InnerText);
                
                astStruct.period = float.Parse(astralBodyXmlNode.ChildNodes[7].InnerText);
                astStruct.isPeriodPublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[7].Attributes["IsPublic"].Value);

                astStruct.isAngularVelocityPublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[8].Attributes["IsPublic"].Value);
                astStruct.radius = float.Parse(astralBodyXmlNode.ChildNodes[9].InnerText);
                astStruct.isRadiusPublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[9].Attributes["IsPublic"].Value);
                // astStruct.AnglePerT = float.Parse(astralBodyXmlNode.ChildNodes[10].InnerText);
                // astStruct.isAnglePerTPublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[10].Attributes["IsPublic"].Value);
                // astStruct.distancePerT= float.Parse(astralBodyXmlNode.ChildNodes[11].InnerText);
                // astStruct.isDistancePerTPublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[11].Attributes["IsPublic"].Value);
                
                astStruct.t= float.Parse(astralBodyXmlNode.ChildNodes[10].InnerText);
                astStruct.isTPublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[10].Attributes["IsPublic"].Value);

                astStruct.isGravityPublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[11].Attributes["IsPublic"].Value);


                astStruct.isCore        = bool.Parse(astralBodyXmlNode.Attributes["IsCore"].Value);
                astStruct.meshNum        = int.Parse(astralBodyXmlNode.Attributes["Style"].Value);
                astStruct.isTarget      = bool.Parse(astralBodyElement.GetAttribute("IsTarget"));

                list.Add(astStruct);
            }

            quizBaseStruct.astralBodyStructList = list;
            quizBaseStruct.quizType =
                (QuizType) Enum.Parse(typeof(QuizType), xmlDoc.SelectSingleNode("AstralBodyList").Attributes[0].Value);

            return quizBaseStruct;
        }

        public static List<XmlDocument> GetQuizFiles(ref List<string> fileNames)
        {
            DirectoryInfo     dir          = new DirectoryInfo(xmlPath);
            List<XmlDocument> xmlDocuments = new List<XmlDocument>();
            fileNames = new List<string>();
            FileInfo[]        files        = dir.GetFiles();
            foreach (FileInfo f in files)
            {
                var filename = f.Name;
                if (filename.EndsWith(".xml") && filename!=".xml")
                {
                    string trueName = filename.Split(new char[] {'.'})[0];
                    xmlDocuments.Add(LoadXml(trueName));
                    fileNames.Add(trueName);

                }
            }

            return xmlDocuments;
        }

        public static void DeleteQuizFiles(string quizName)
        {
            if (File.Exists(xmlPath + quizName + ".xml"))
            {
                File.Delete(xmlPath + quizName + ".xml");
            }
        }
    }
}