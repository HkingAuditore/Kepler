using System.IO;
using UnityEditor;
using UnityEngine;

namespace Minimalist
{
    [InitializeOnLoad]
    public class MinimalistEditorHelper
    {

        static MinimalistEditorHelper()
        {
            FindMinimalistDirectory();    
        }
        
        
        private static string _minimalistRoot = string.Empty;
        public static string MinimalistRootDirectory
        {
            get
            {
                if(string.IsNullOrEmpty(_minimalistRoot))
                    FindMinimalistDirectory();
                return _minimalistRoot;
            }
        }
        
        private static void FindMinimalistDirectory() {
            string[] ids = AssetDatabase.FindAssets("MinimalistStandardEditor t:Script");
            string scriptPath = string.Empty;
            foreach (string id in ids) {
                string assetPath = AssetDatabase.GUIDToAssetPath(id);
                if (!assetPath.Contains($"Minimalist/Editor/MinimalistStandardEditor.cs") &&
                    !assetPath.Contains($"Minimalist\\Editor\\MinimalistStandardEditor.cs")) continue;
                scriptPath = assetPath;
                break;
            }

            if (string.IsNullOrEmpty(scriptPath)) {
                return;
            }

            DirectoryInfo rootDirectory = new DirectoryInfo(scriptPath);
            rootDirectory = rootDirectory.Parent.Parent;
            
            DirectoryInfo unityRoot = new DirectoryInfo(Application.dataPath).Parent;

            _minimalistRoot = rootDirectory.ToString()
                .Replace(unityRoot.ToString() + Path.DirectorySeparatorChar, string.Empty) 
                + Path.DirectorySeparatorChar;
        }
    }
}