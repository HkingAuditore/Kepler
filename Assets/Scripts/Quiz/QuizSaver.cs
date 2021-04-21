using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;
using XmlSaver;

namespace Quiz
{
    public class QuizSaver : XmlSaver.XmlSaver<QuizAstralBody>
    {
        private new static string xmlPath => Application.dataPath + "/Quiz/";
        

        private XmlElement ConvertAstralBody2XmlElement(AstralBodyDict<QuizAstralBody> astralBodyDict)
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
            mass.InnerText = astralBodyDict.astralBody.realMass.ToString(CultureInfo.InvariantCulture);
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


            astAstralBody.SetAttribute("IsCore", astralBodyDict.isTarget.ToString());
            astAstralBody.SetAttribute("Style",  astralBodyDict.astralBody.meshNum.ToString());

            dict.SetAttribute("IsTarget", astralBodyDict.isTarget.ToString());
            dict.AppendChild(astTransform);
            dict.AppendChild(astAstralBody);
            return dict;
        }

        /// <summary>
        ///     将星体群转为XML文档
        /// </summary>
        /// <param name="astOrbit">AstralBodyDict集</param>
        /// <param name="quizType">问题类型</param>
        /// <returns></returns>
        public XmlDocument ConvertOrbit2Xml(List<AstralBodyDict<QuizAstralBody>> astOrbit, QuizType quizType)
        {
            var astList = _xmlDoc.CreateElement("AstralBodyList");
            foreach (var astralBodyDict in astOrbit) astList.AppendChild(ConvertAstralBody2XmlElement(astralBodyDict));

            astList.SetAttribute("QuizType", quizType.ToString());
            _xmlDoc.AppendChild(astList);
            return _xmlDoc;
        }

        /// <summary>
        ///     将XML转为问题
        /// </summary>
        /// <param name="xmlDoc">xml文档</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static QuizBaseStruct ConvertXml2QuizBase(XmlDocument xmlDoc, string fileName)
        {
            var quizBaseStruct = new QuizBaseStruct();
            quizBaseStruct.quizName = fileName;
            var list           = new List<QuizAstralBodyDict>();
            var astralBodyList = xmlDoc.SelectSingleNode("AstralBodyList").ChildNodes;
            foreach (XmlElement astralBodyElement in astralBodyList)
            {
                var astStruct = new QuizAstralBodyDict();

                //Position
                astStruct.position =
                    ConvertString2Vector3(astralBodyElement.GetElementsByTagName("Transform")[0].InnerText);

                //AstralBody
                var astralBodyXmlNode = astralBodyElement.GetElementsByTagName("AstralBody")[0];
                astStruct.mass             = double.Parse(astralBodyXmlNode.SelectSingleNode("Mass").InnerText);
                astStruct.isMassPublic     = bool.Parse(astralBodyXmlNode.SelectSingleNode("Mass").Attributes["IsPublic"].Value);
                astStruct.density          = float.Parse(astralBodyXmlNode.SelectSingleNode("Density").InnerText);
                astStruct.originalSize     = float.Parse(astralBodyXmlNode.SelectSingleNode("Size").InnerText);
                astStruct.isSizePublic     = bool.Parse(astralBodyXmlNode.SelectSingleNode("Size").Attributes["IsPublic"].Value);
                astStruct.oriVelocity      = ConvertString2Vector3(astralBodyXmlNode.SelectSingleNode("Velocity").InnerText);
                astStruct.isVelocityPublic = bool.Parse(astralBodyXmlNode.SelectSingleNode("Velocity").Attributes["IsPublic"].Value);
                astStruct.enableAffect     = bool.Parse(astralBodyXmlNode.SelectSingleNode("EnableAffect").InnerText);
                astStruct.enableTracing    = bool.Parse(astralBodyXmlNode.SelectSingleNode("EnableTracing").InnerText);
                astStruct.affectRadius     = float.Parse(astralBodyXmlNode.SelectSingleNode("AffectRadius").InnerText);

                astStruct.period         = float.Parse(astralBodyXmlNode.SelectSingleNode("Period").InnerText);
                astStruct.isPeriodPublic = bool.Parse(astralBodyXmlNode.SelectSingleNode("Period").Attributes["IsPublic"].Value);

                astStruct.isAngularVelocityPublic =
                    bool.Parse(astralBodyXmlNode.SelectSingleNode("AngularVelocity").Attributes["IsPublic"].Value);
                astStruct.radius         = float.Parse(astralBodyXmlNode.SelectSingleNode("Radius").InnerText);
                astStruct.isRadiusPublic = bool.Parse(astralBodyXmlNode.SelectSingleNode("Radius").Attributes["IsPublic"].Value);
                // astStruct.AnglePerT = float.Parse(astralBodyXmlNode.ChildNodes[10].InnerText);
                // astStruct.isAnglePerTPublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[10].Attributes["IsPublic"].Value);
                // astStruct.distancePerT= float.Parse(astralBodyXmlNode.ChildNodes[11].InnerText);
                // astStruct.isDistancePerTPublic = Boolean.Parse(astralBodyXmlNode.ChildNodes[11].Attributes["IsPublic"].Value);

                astStruct.t         = float.Parse(astralBodyXmlNode.SelectSingleNode("T").InnerText);
                astStruct.isTPublic = bool.Parse(astralBodyXmlNode.SelectSingleNode("T").Attributes["IsPublic"].Value);

                astStruct.isGravityPublic = bool.Parse(astralBodyXmlNode.SelectSingleNode("Gravity").Attributes["IsPublic"].Value);


                astStruct.isCore   = bool.Parse(astralBodyXmlNode.Attributes["IsCore"].Value);
                astStruct.meshNum  = int.Parse(astralBodyXmlNode.Attributes["Style"].Value);
                astStruct.isTarget = bool.Parse(astralBodyElement.GetAttribute("IsTarget"));

                list.Add(astStruct);
            }

            quizBaseStruct.astralBodyStructList = list;
            quizBaseStruct.quizType =
                (QuizType) Enum.Parse(typeof(QuizType), xmlDoc.SelectSingleNode("AstralBodyList").Attributes[0].Value);

            return quizBaseStruct;
        }
        

        /// <summary>
        ///     获取存档问题集合
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public static List<XmlDocument> GetQuizFiles(ref List<string> fileNames)
        {
            var dir          = new DirectoryInfo(xmlPath);
            var xmlDocuments = new List<XmlDocument>();
            fileNames = new List<string>();
            var files = dir.GetFiles();
            foreach (var f in files)
            {
                var filename = f.Name;
                if (filename.EndsWith(".xml") && filename != ".xml")
                {
                    var trueName = filename.Split('.')[0];
                    xmlDocuments.Add(LoadXml(trueName));
                    fileNames.Add(trueName);
                }
            }

            return xmlDocuments;
        }

        /// <summary>
        ///     删除问题存档
        /// </summary>
        /// <param name="quizName"></param>
        public static void DeleteFiles(string quizName, string pathDirectory = null)
        {
            XmlSaver.XmlSaver<QuizAstralBody>.DeleteFiles(quizName, pathDirectory = pathDirectory ?? xmlPath);
        }

        
        /// <summary>
        /// 读取XML文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="pathDirectory"></param>
        /// <returns></returns>
        public new static XmlDocument LoadXml(string fileName, string pathDirectory = null)
        {
            return XmlSaver.XmlSaver<QuizAstralBody>.LoadXml(fileName, pathDirectory = pathDirectory ?? xmlPath);
        }

    }
}