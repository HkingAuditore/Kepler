using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Dreamteck
{
    public static class ScriptableObjectUtility
    {
        public static T CreateAsset<T>(string name = "", bool selectAfterCreation = true) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            SaveAsset(asset, name, selectAfterCreation);
            return asset;
        }

        public static ScriptableObject CreateAsset(string type, string name = "", bool selectAfterCreation = true)
        {
            var asset = ScriptableObject.CreateInstance(type);
            SaveAsset(asset, name, selectAfterCreation);
            return asset;
        }

        private static void SaveAsset<T>(T asset, string name = "", bool selectAfterCreation = true)
            where T : ScriptableObject
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
                path = "Assets";
            else if (Path.GetExtension(path) != "")
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            var assetName             = "New " + typeof(T);
            if (name != "") assetName = name;
            var assetPathAndName      = AssetDatabase.GenerateUniqueAssetPath(path + "/" + assetName + ".asset");
            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            if (selectAfterCreation) Selection.activeObject = asset;
        }
    }
}
#endif