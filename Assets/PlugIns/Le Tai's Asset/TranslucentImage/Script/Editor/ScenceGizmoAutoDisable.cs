using System;
using System.Linq;
using System.Reflection;
using UnityEditor.Callbacks;

namespace LeTai.Asset.TranslucentImage.Editor
{
    public static class ScenceGizmoAutoDisable
    {
        private static readonly string[] NO_GIZMOS_CLASSES =
        {
            "TranslucentImage",
            "TranslucentImageSource"
        };

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var Annotation = Type.GetType("UnityEditor.Annotation, UnityEditor");
            if (Annotation == null) return;

            var ClassId     = Annotation.GetField("classID");
            var ScriptClass = Annotation.GetField("scriptClass");
            var Flags       = Annotation.GetField("flags");
            var IconEnabled = Annotation.GetField("iconEnabled");

            var AnnotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            if (AnnotationUtility == null) return;

            var GetAnnotations = AnnotationUtility.GetMethod("GetAnnotations",
                                                             BindingFlags.NonPublic | BindingFlags.Public |
                                                             BindingFlags.Static);
            if (GetAnnotations == null) return;
            var SetIconEnabled = AnnotationUtility.GetMethod("SetIconEnabled",
                                                             BindingFlags.NonPublic | BindingFlags.Public |
                                                             BindingFlags.Static);
            if (SetIconEnabled == null) return;


            var annotations = (Array) GetAnnotations.Invoke(null, null);
            foreach (var a in annotations)
            {
                var classId     = (int) ClassId.GetValue(a);
                var scriptClass = (string) ScriptClass.GetValue(a);
                var flags       = (int) Flags.GetValue(a);
                var iconEnabled = (int) IconEnabled.GetValue(a);

                // built in types
                if (string.IsNullOrEmpty(scriptClass)) continue;

                // load a json or text file with class names

                const int HasIcon     = 1;
                var       hasIconFlag = (flags & HasIcon) == HasIcon;

                if (hasIconFlag      &&
                    iconEnabled != 0 &&
                    NO_GIZMOS_CLASSES.Contains(scriptClass))
                    SetIconEnabled.Invoke(null, new object[] {classId, scriptClass, 0});
            }
        }
    }
}