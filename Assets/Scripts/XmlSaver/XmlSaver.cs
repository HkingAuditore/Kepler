using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Quiz;
using SpacePhysic;
using UnityEditor;
using UnityEngine;

namespace XmlSaver
{
    public delegate void ConvertAstralBodyDictHandler<T>(AstralBodyDataDict<T> astralBodyDataDict, XmlElement xmlElement)
        where T : AstralBody;

    public class XmlSaver<T> : MonoBehaviour where T : AstralBody
    {
        protected XmlDocument _xmlDoc = new XmlDocument();

        private static string xmlPath => Application.dataPath + "/ScenesData/";

        /// <summary>
        ///     保存XML
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="fileName"></param>
        /// <exception cref="SaverException"></exception>
        public virtual void SaveXml(XmlDocument doc, string fileName)
        {
            var path = xmlPath + fileName + ".xml";
            if (!File.Exists(path))
            {
                doc.Save(path);
            }
            else
            {
                doc.Save(path);
                AssetDatabase.Refresh();
                throw new SaverException("文件已存在，进行覆盖！");
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     加载XML
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="pathDirectory">存储文件夹</param>
        /// <returns></returns>
        /// <exception cref="SaverException"></exception>
        public static XmlDocument LoadXml(string fileName, string pathDirectory = null)
        {
            pathDirectory = pathDirectory ?? xmlPath;
            var path = pathDirectory + fileName + ".xml";
            if (File.Exists(path))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                AssetDatabase.Refresh();
                return xmlDoc;
            }

            Debug.Log(xmlPath);
            throw new SaverException(path + "文件不存在");
        }

        protected static Vector3 ConvertString2Vector3(string str)
        {
            var vec = str.Trim(' ').Trim('(').Trim(')').Split(',');
            return new Vector3(float.Parse(vec[0]), float.Parse(vec[1]), float.Parse(vec[2]));
        }

        /// <summary>
        ///     获取存档问题集合
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public static List<XmlDocument> GetFiles(ref List<string> fileNames, string pathDirectory = null)
        {
            pathDirectory = pathDirectory ?? xmlPath;
            var dir          = new DirectoryInfo(pathDirectory);
            var xmlDocuments = new List<XmlDocument>();
            fileNames = new List<string>();
            var files = dir.GetFiles();
            foreach (var f in files)
            {
                var filename = f.Name;
                if (filename.EndsWith(".xml") && filename != ".xml")
                {
                    var trueName = filename.Split('.')[0];
                    xmlDocuments.Add(LoadXml(trueName, pathDirectory));
                    fileNames.Add(trueName);
                }
            }

            return xmlDocuments;
        }

        /// <summary>
        ///     删除存档
        /// </summary>
        /// <param name="sceneName">存档名</param>
        /// <param name="pathDirectory">存档路径</param>
        public static void DeleteFiles(string sceneName, string pathDirectory = null)
        {
            pathDirectory = pathDirectory ?? xmlPath;
            if (File.Exists(xmlPath + sceneName + ".xml")) File.Delete(xmlPath + sceneName + ".xml");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="fileName"></param>
        /// <param name="convertDelegate">对astralBodyStructList中元素处理的委托</param>
        /// <typeparam name="U">astralBodyStructList的类型</typeparam>
        /// <returns></returns>
        public static SceneBaseStruct<T> ConvertXml2SceneBase(XmlDocument                   xmlDoc, string fileName,
                                                              ConvertAstralBodyDictHandler<T> convertDelegate = null) 
        {
            SceneBaseStruct<T> sceneBaseStruct = new SceneBaseStruct<T>();
            if (typeof(T) == typeof(QuizAstralBody))
            {
                sceneBaseStruct = (SceneBaseStruct<T>)Activator.CreateInstance(typeof(QuizBaseStruct));
            }
   
            var list            = new List<AstralBodyDataDict<T>>();
            var astralBodyList  = xmlDoc.SelectSingleNode("AstralBodyList").ChildNodes;

            ConvertAstralBodyDictHandler<T> convertHandler;
            convertHandler = (astralBodyDict, xmlElement) =>
                             {
                                 //Position
                                 astralBodyDict.position =
                                     ConvertString2Vector3(xmlElement.GetElementsByTagName("Transform")[0].InnerText);

                                 //AstralBody
                                 var astralBodyXmlNode = xmlElement.GetElementsByTagName("AstralBody")[0];
                                 astralBodyDict.mass =
                                     double.Parse(astralBodyXmlNode.SelectSingleNode("Mass").InnerText);
                                 astralBodyDict.density =
                                     float.Parse(astralBodyXmlNode.SelectSingleNode("Density").InnerText);
                                 astralBodyDict.originalSize =
                                     float.Parse(astralBodyXmlNode.SelectSingleNode("Size").InnerText);
                                 astralBodyDict.oriVelocity =
                                     ConvertString2Vector3(astralBodyXmlNode.SelectSingleNode("Velocity").InnerText);
                                 astralBodyDict.enableAffect =
                                     bool.Parse(astralBodyXmlNode.SelectSingleNode("EnableAffect").InnerText);
                                 astralBodyDict.enableTracing =
                                     bool.Parse(astralBodyXmlNode.SelectSingleNode("EnableTracing").InnerText);
                                 astralBodyDict.affectRadius =
                                     float.Parse(astralBodyXmlNode.SelectSingleNode("AffectRadius").InnerText);
                                 astralBodyDict.period =
                                     float.Parse(astralBodyXmlNode.SelectSingleNode("Period").InnerText);
                                 astralBodyDict.radius =
                                     float.Parse(astralBodyXmlNode.SelectSingleNode("Radius").InnerText);
                                 astralBodyDict.isCore  = bool.Parse(astralBodyXmlNode.Attributes["IsCore"].Value);
                                 astralBodyDict.meshNum = int.Parse(astralBodyXmlNode.Attributes["Style"].Value);

                             };
            if(convertDelegate!=null)
                convertHandler += convertDelegate;
            
            foreach (XmlElement astralBodyElement in astralBodyList)
            {
                AstralBodyDataDict<T> astStruct = new AstralBodyDataDict<T>();
                if (typeof(T) == typeof(QuizAstralBody))
                {
                    astStruct = (AstralBodyDataDict<T>)Activator.CreateInstance(typeof(QuizAstralBodyDataDict));
                }

                convertHandler(astStruct, astralBodyElement);

                list.Add(astStruct);
            }

            sceneBaseStruct.astralBodyStructList = list;

            return sceneBaseStruct;
        }
    }
}