using System.IO;
using System.Xml;
using SpacePhysic;
using UnityEditor;
using UnityEngine;

namespace XmlSaver
{
    public class XmlSaver<T> : MonoBehaviour where T : AstralBody
    {
        protected  XmlDocument _xmlDoc = new XmlDocument();

        private static string xmlPath      => Application.dataPath + "/ScenesData/";

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
        public static XmlDocument LoadXml(string fileName,string pathDirectory = null)
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
        ///     删除问题存档
        /// </summary>
        /// <param name="quizName"></param>
        public static void DeleteFiles(string sceneName, string pathDirectory = null)
        {
            pathDirectory = pathDirectory ?? xmlPath;
            if (File.Exists(xmlPath + sceneName + ".xml")) File.Delete(xmlPath + sceneName + ".xml");
        }

    }
}