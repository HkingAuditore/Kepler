using System.IO;
using UnityEditor;
using UnityEngine;

namespace Minimalist
{
    [InitializeOnLoad]
    public class MinimalistEditorHelper
    {
        private static string _minimalistRoot = string.Empty;

        static MinimalistEditorHelper()
        {
            FindMinimalistDirectory();
        }

        public static string MinimalistRootDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_minimalistRoot))
                    FindMinimalistDirectory();
                return _minimalistRoot;
            }
        }

        private static void FindMinimalistDirectory()
        {
            var ids        = AssetDatabase.FindAssets("MinimalistStandardEditor t:Script");
            var scriptPath = string.Empty;
            foreach (var id in ids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(id);
                if (!assetPath.Contains("Minimalist/Editor/MinimalistStandardEditor.cs") &&
                    !assetPath.Contains("Minimalist\\Editor\\MinimalistStandardEditor.cs")) continue;
                scriptPath = assetPath;
                break;
            }

            if (string.IsNullOrEmpty(scriptPath)) return;

            var rootDirectory = new DirectoryInfo(scriptPath);
            rootDirectory = rootDirectory.Parent.Parent;

            var unityRoot = new DirectoryInfo(Application.dataPath).Parent;

            _minimalistRoot = rootDirectory.ToString()
                                           .Replace(unityRoot.ToString() + Path.DirectorySeparatorChar, string.Empty)
                            + Path.DirectorySeparatorChar;
        }
    }
}