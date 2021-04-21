using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;
using XmlSaver;

namespace Quiz
{
    public class QuizSaver : XmlSaver<QuizAstralBody>
    {
        private static string xmlPath => Application.dataPath + "/Quiz/";


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
        public static QuizBaseStruct ConvertXml2SceneBase(XmlDocument xmlDoc, string fileName)
        {
            return (QuizBaseStruct)ConvertXml2SceneBase(xmlDoc, fileName, (astralBodyStruct, astralBodyElement) =>
                                                          {
                                                              QuizAstralBodyDict astStruct =
                                                                  (QuizAstralBodyDict) astralBodyStruct;
                                                              var astralBodyXmlNode =
                                                                  astralBodyElement
                                                                     .GetElementsByTagName("AstralBody")
                                                                      [0];
                                                              astStruct.isMassPublic =
                                                                  bool.Parse(astralBodyXmlNode
                                                                            .SelectSingleNode("Mass")
                                                                           ?.Attributes?["IsPublic"]
                                                                            .Value ?? "false");
                                                              astStruct.isSizePublic =
                                                                  bool.Parse(astralBodyXmlNode
                                                                            .SelectSingleNode("Size")
                                                                           ?.Attributes?["IsPublic"]
                                                                            .Value ?? "false");
                                                              astStruct.isVelocityPublic =
                                                                  bool.Parse(astralBodyXmlNode
                                                                            .SelectSingleNode("Velocity")
                                                                           ?.Attributes?["IsPublic"]
                                                                            .Value ??  "false");
                                                              astStruct.isPeriodPublic =
                                                                  bool.Parse(astralBodyXmlNode
                                                                            .SelectSingleNode("Period")
                                                                           ?.Attributes?["IsPublic"]
                                                                            .Value ??  "false");
                                                              astStruct.isAngularVelocityPublic =
                                                                  bool.Parse(astralBodyXmlNode
                                                                            .SelectSingleNode("AngularVelocity")
                                                                           ?.Attributes?["IsPublic"]
                                                                            .Value ??  "false");
                                                              astStruct.radius =
                                                                  float.Parse(astralBodyXmlNode
                                                                             .SelectSingleNode("Radius")
                                                                            ?.InnerText ?? "0");
                                                              astStruct.isRadiusPublic =
                                                                  bool.Parse(astralBodyXmlNode
                                                                            .SelectSingleNode("Radius")
                                                                           ?.Attributes?["IsPublic"]
                                                                            .Value ??  "false");
                                                              astStruct.t =
                                                                  float.Parse(astralBodyXmlNode
                                                                             .SelectSingleNode("T")
                                                                            ?.InnerText ?? "0");
                                                              astStruct.isTPublic =
                                                                  bool.Parse(astralBodyXmlNode
                                                                            .SelectSingleNode("T")
                                                                           ?.Attributes?["IsPublic"]
                                                                            .Value ??  "false");
                                                              astStruct.isGravityPublic =
                                                                  bool.Parse(astralBodyXmlNode
                                                                            .SelectSingleNode("Gravity")
                                                                           ?.Attributes?["IsPublic"]
                                                                            .Value ??  "false");
                                                              astStruct.isTarget =
                                                                  bool.Parse(astralBodyElement
                                                                                .GetAttribute("IsTarget"));
                                                          });
        }


        /// <summary>
        ///     获取存档问题集合
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public new static List<XmlDocument> GetFiles(ref List<string> fileNames, string pathDirectory = null)
        {
            return XmlSaver<QuizAstralBody>.GetFiles(ref fileNames, pathDirectory ?? xmlPath);
        }

        /// <summary>
        ///     删除问题存档
        /// </summary>
        /// <param name="quizName"></param>
        public new static void DeleteFiles(string quizName, string pathDirectory = null)
        {
            XmlSaver<QuizAstralBody>.DeleteFiles(quizName, pathDirectory ?? xmlPath);
        }


        /// <summary>
        ///     读取XML文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="pathDirectory"></param>
        /// <returns></returns>
        public new static XmlDocument LoadXml(string fileName, string pathDirectory = null)
        {
            return XmlSaver<QuizAstralBody>.LoadXml(fileName, pathDirectory ?? xmlPath);
        }
    }
}