using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEngine;
using XmlSaver;

namespace Quiz
{
    public class QuizSaver : XmlSaver<QuizAstralBody>
    {
        private static string xmlPath => Application.dataPath + "/Quiz/";


        protected override XmlElement ConvertAstralBody2XmlElement(AstralBodyDict<QuizAstralBody>        astralBodyDict,
                                                                   ConvertAstralBodyPropertyToXmlHandler convertAstralBodyPropertyToXmlHandler = null,
                                                                   ConvertAstralDictToXmlHandler         convertAstralDictToXmlHandler         = null)
        {

            try
            {
                astralBodyDict.astralBody.UpdateQuizAstralBody();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return base.ConvertAstralBody2XmlElement(astralBodyDict, convertAstralBodyPropertyToXmlHandler ?? ((dict, doc) =>
                                                                                                        {
                                                                                                            QuizAstralBodyDict
                                                                                                                quizDict
                                                                                                                    = (
                                                                                                                        QuizAstralBodyDict
                                                                                                                    ) dict;
                                                                                                            var
                                                                                                                astAstralBody
                                                                                                                    = doc
                                                                                                                       .CreateElement("AstralBody");
                                                                                                            //写入天体属性
                                                                                                            var mass =
                                                                                                                doc
                                                                                                                   .CreateElement("Mass");
                                                                                                            mass
                                                                                                                   .InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .realMass
                                                                                                                 .ToString(CultureInfo
                                                                                                                              .InvariantCulture);
                                                                                                            mass
                                                                                                               .SetAttribute("IsPublic",
                                                                                                                             quizDict
                                                                                                                                .astralBody
                                                                                                                                .isMassPublic
                                                                                                                                .ToString());
                                                                                                            astAstralBody
                                                                                                               .AppendChild(mass);
                                                                                                            var density
                                                                                                                = doc
                                                                                                                   .CreateElement("Density");
                                                                                                            density
                                                                                                                   .InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .density
                                                                                                                 .ToString(CultureInfo
                                                                                                                              .InvariantCulture);
                                                                                                            astAstralBody
                                                                                                               .AppendChild(density);
                                                                                                            var size =
                                                                                                                doc
                                                                                                                   .CreateElement("Size");
                                                                                                            size
                                                                                                               .SetAttribute("IsPublic",
                                                                                                                             quizDict
                                                                                                                                .astralBody
                                                                                                                                .isSizePublic
                                                                                                                                .ToString());

                                                                                                            size
                                                                                                                   .InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .size
                                                                                                                 .ToString(CultureInfo
                                                                                                                              .InvariantCulture);
                                                                                                            astAstralBody
                                                                                                               .AppendChild(size);

                                                                                                            var velocity
                                                                                                                = doc
                                                                                                                   .CreateElement("Velocity");
                                                                                                            velocity
                                                                                                                   .InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .GetVelocity()
                                                                                                                 .ToString();
                                                                                                            velocity
                                                                                                               .SetAttribute("IsPublic",
                                                                                                                             quizDict
                                                                                                                                .astralBody
                                                                                                                                .isVelocityPublic
                                                                                                                                .ToString());
                                                                                                            astAstralBody
                                                                                                               .AppendChild(velocity);

                                                                                                            var isAffect
                                                                                                                =doc
                                                                                                                   .CreateElement("EnableAffect");
                                                                                                            isAffect
                                                                                                                   .InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .enableAffect
                                                                                                                 .ToString();
                                                                                                            astAstralBody
                                                                                                               .AppendChild(isAffect);
                                                                                                            var
                                                                                                                isTracing
                                                                                                                    = doc
                                                                                                                       .CreateElement("EnableTracing");
                                                                                                            isTracing
                                                                                                                   .InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .enableTracing
                                                                                                                 .ToString();
                                                                                                            astAstralBody
                                                                                                               .AppendChild(isTracing);
                                                                                                            var
                                                                                                                affectRadius
                                                                                                                    = doc
                                                                                                                       .CreateElement("AffectRadius");
                                                                                                            affectRadius
                                                                                                                   .InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .affectRadius
                                                                                                                 .ToString(CultureInfo
                                                                                                                              .InvariantCulture);
                                                                                                            astAstralBody
                                                                                                               .AppendChild(affectRadius);

                                                                                                            var period =
                                                                                                                doc
                                                                                                                   .CreateElement("Period");
                                                                                                            period
                                                                                                               .SetAttribute("IsPublic",
                                                                                                                             quizDict
                                                                                                                                .astralBody
                                                                                                                                .isPeriodPublic
                                                                                                                                .ToString());
                                                                                                            period
                                                                                                                   .InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .period
                                                                                                                 .ToString(CultureInfo
                                                                                                                              .InvariantCulture);
                                                                                                            astAstralBody
                                                                                                               .AppendChild(period);
                                                                                                            //
                                                                                                            var
                                                                                                                angularVelocity
                                                                                                                    = doc
                                                                                                                       .CreateElement("AngularVelocity");
                                                                                                            angularVelocity
                                                                                                               .SetAttribute("IsPublic",
                                                                                                                             quizDict
                                                                                                                                .astralBody
                                                                                                                                .isAngularVelocityPublic
                                                                                                                                .ToString());
                                                                                                            angularVelocity
                                                                                                                   .InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .globalAngularVelocity
                                                                                                                 .ToString();
                                                                                                            astAstralBody
                                                                                                               .AppendChild(angularVelocity);
                                                                                                            //
                                                                                                            var radius =
                                                                                                                doc
                                                                                                                   .CreateElement("Radius");
                                                                                                            radius
                                                                                                               .SetAttribute("IsPublic",
                                                                                                                             quizDict
                                                                                                                                .astralBody
                                                                                                                                .isRadiusPublic
                                                                                                                                .ToString());
                                                                                                            radius
                                                                                                                   .InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .radius
                                                                                                                 .ToString(CultureInfo
                                                                                                                              .InvariantCulture);
                                                                                                            astAstralBody
                                                                                                               .AppendChild(radius);


                                                                                                            var t =
                                                                                                                doc
                                                                                                                   .CreateElement("T");
                                                                                                            t.SetAttribute("IsPublic",
                                                                                                                           quizDict
                                                                                                                              .astralBody
                                                                                                                              .isTPublic
                                                                                                                              .ToString());
                                                                                                            t.InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .t
                                                                                                                 .ToString(CultureInfo
                                                                                                                              .InvariantCulture);
                                                                                                            astAstralBody
                                                                                                               .AppendChild(t);

                                                                                                            var gravity
                                                                                                                = doc
                                                                                                                   .CreateElement("Gravity");
                                                                                                            gravity
                                                                                                               .SetAttribute("IsPublic",
                                                                                                                             quizDict
                                                                                                                                .astralBody
                                                                                                                                .isGravityPublic
                                                                                                                                .ToString());
                                                                                                            gravity
                                                                                                                   .InnerText
                                                                                                                = quizDict
                                                                                                                 .astralBody
                                                                                                                 .gravity
                                                                                                                 .ToString(CultureInfo
                                                                                                                              .InvariantCulture);
                                                                                                            astAstralBody
                                                                                                               .AppendChild(gravity);



                                                                                                            astAstralBody
                                                                                                               .SetAttribute("Style",
                                                                                                                             quizDict
                                                                                                                                .astralBody
                                                                                                                                .meshNum
                                                                                                                                .ToString());
                                                                                                            astAstralBody
                                                                                                               .SetAttribute("IsCore",
                                                                                                                             quizDict
                                                                                                                                .isTarget
                                                                                                                                .ToString());
                                                                                                            
                                                                                                            return
                                                                                                                astAstralBody;

                                                                                                        }),convertAstralDictToXmlHandler ?? ((dict, element) =>
                                                                                                                                         {
                                                                                                                                             QuizAstralBodyDict
                                                                                                                                                 quizDict
                                                                                                                                                     = (
                                                                                                                                                         QuizAstralBodyDict
                                                                                                                                                     ) dict;
                                                                                                                                             element.SetAttribute("IsTarget", quizDict.isTarget.ToString());
                                                                                                                                         }));
            
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
            var quizBaseStruct =  (QuizBaseStruct)(XmlSaver<QuizAstralBody>.ConvertXml2SceneBase(xmlDoc, fileName, (astralBodyStruct, astralBodyElement) =>
                                                                          {
                                                                              QuizAstralBodyDataDict astStruct =
                                                                                  (QuizAstralBodyDataDict)astralBodyStruct;
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
                                                                              astStruct.period =
                                                                                  float.Parse(astralBodyXmlNode.SelectSingleNode("Period").InnerText);
                                                                              astStruct.radius =
                                                                                  float.Parse(astralBodyXmlNode.SelectSingleNode("Radius").InnerText);

                                                                          }));
            quizBaseStruct.quizType =
                (QuizType) Enum.Parse(typeof(QuizType), xmlDoc.SelectSingleNode("AstralBodyList").Attributes[0].Value);
            quizBaseStruct.sceneName = fileName;
            return quizBaseStruct;
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

        public override void SaveXml(XmlDocument doc, string fileName)
        {
            var path = xmlPath + fileName + ".xml";
            if (!File.Exists(path))
            {
                doc.Save(path);
            }
            else
            {
                doc.Save(path);
                // AssetDatabase.Refresh();
                throw new SaverException("文件已存在，进行覆盖！");
            }

        }
    }
}