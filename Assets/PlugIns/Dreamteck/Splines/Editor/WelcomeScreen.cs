using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [InitializeOnLoad]
    public static class PluginInfo
    {
        public static           string version = "2.07";
        private static readonly bool   open;

        static PluginInfo()
        {
            if (open) return;
            var showInfo = EditorPrefs.GetString("Dreamteck.Splines.Info.version", "") != version;
            if (!showInfo) return;
            EditorWindow.GetWindow<WelcomeScreen>(true);
            EditorPrefs.SetString("Dreamteck.Splines.Info.version", version);
            open = true;
        }
    }

    [InitializeOnLoad]
    public static class AddScriptingDefines
    {
        static AddScriptingDefines()
        {
            AddScriptingDefine("DREAMTECK_SPLINES", EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        private static void AddScriptingDefine(string newDefine, BuildTargetGroup target)
        {
            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            if (definesString.Contains(newDefine)) return;
            Debug.Log("Added \"DREAMTECK_SPLINES\" to " + EditorUserBuildSettings.selectedBuildTargetGroup +
                      " Scripting define in Player Settings");
            var allDefines = definesString.Split(';');
            UnityEditor.ArrayUtility.Add(ref allDefines, newDefine);
            definesString = string.Join(";", allDefines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, definesString);
        }
    }

    public class WelcomeScreen : WelcomeWindow
    {
        [MenuItem("Help/Dreamteck/About Dreamteck Splines")]
        public static void OpenWindow()
        {
            GetWindow<WelcomeScreen>(true);
        }

        protected override void GetHeader()
        {
            header = ResourceUtility.EditorLoadTexture("Splines/Editor/Icons", "plugin_header");
        }

        public override void Load()
        {
            base.Load();
            minSize = maxSize = new Vector2(450, 550);
            SetTitle("Dreamteck Splines " + PluginInfo.version, "");
            panels    = new WindowPanel[6];
            panels[0] = new WindowPanel("Home",      true,  0.25f);
            panels[1] = new WindowPanel("Changelog", false, panels[0], 0.25f);
            panels[2] = new WindowPanel("Learn",     false, panels[0], 0.25f);
            panels[3] = new WindowPanel("Support",   false, panels[0], 0.25f);
            panels[4] = new WindowPanel("Examples",  false, panels[2], 0.25f);
            panels[5] = new WindowPanel("Playmaker", false, panels[0], 0.25f);


            panels[0].elements.Add(new WindowPanel.Space(400, 10));
            panels[0].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "changelog", "What's new?",
                                                             "See all new features, important changes and bugfixes in " +
                                                             PluginInfo.version, new ActionLink(panels[1], panels[0])));
            panels[0].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "get_started", "Get Started",
                                                             "Learn how to use Dreamteck Splines in a matter of minutes",
                                                             new ActionLink(panels[2], panels[0])));
            panels[0].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "support",
                                                             "Community & Support",
                                                             "Got a problem or a feature request? Our support is here to help!",
                                                             new ActionLink(panels[3], panels[0])));
            panels[0].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "rate", "Rate",
                                                             "If you like Dreamteck Splines, please consider rating it on the Asset Store",
                                                             new ActionLink("http://u3d.as/sLk")));
            panels[0].elements.Add(new WindowPanel.Space(400, 20));
            panels[0].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "playmaker",
                                                             "Playmaker Actions",
                                                             "Install Playmaker actions for Dreamteck Splines",
                                                             new ActionLink(panels[5], panels[0])));
            panels[0].elements.Add(new WindowPanel.Thumbnail("Splines/Editor/Icons", "forever", "Forever",
                                                             "Creating endless runners has never been easier. Forever is here to change the game!",
                                                             new ActionLink("http://u3d.as/1t9T")));
            panels[0].elements.Add(new WindowPanel.Space(400, 10));
            panels[0].elements
                     .Add(new
                              WindowPanel.Label("This window will not appear again automatically. To open it manually go to Help/Dreamteck/About Dreamteck Splines",
                                                wrapText, new Color(1f, 1f, 1f, 0.5f), 400, 100));


            var path          = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/Editor");
            var changelogText = "Changelog file not found.";
            if (Directory.Exists(path))
                if (File.Exists(path + "/changelog.txt"))
                {
                    var lines = File.ReadAllLines(path + "/changelog.txt");
                    changelogText = "";
                    for (var i = 0; i < lines.Length; i++) changelogText += lines[i] + "\r\n";
                }

            panels[1].elements.Add(new WindowPanel.Space(400, 10));
            panels[1].elements.Add(new WindowPanel.ScrollText(400, 400, changelogText));

            panels[2].elements.Add(new WindowPanel.Space(400, 10));
            panels[2].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "manual", "User Manual",
                                                             "Read a thorough documentation of the whole package along with a list of API methods.",
                                                             new
                                                                 ActionLink("http://dreamteck.io/page/dreamteck_splines/user_manual.pdf")));
            panels[2].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "tutorials", "Video Tutorials",
                                                             "Watch a series of Youtube videos to get started.",
                                                             new
                                                                 ActionLink("https://www.youtube.com/playlist?list=PLkZqalQdFIQ6zym8RwSWWl3PZJuUdvNK6")));
            panels[2].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "examples", "Examples",
                                                             "Install example scenes",
                                                             new ActionLink(panels[4], panels[2])));

            panels[3].elements.Add(new WindowPanel.Space(400, 10));
            panels[3].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "discord", "Discord Server",
                                                             "Join our Discord community and chat with other developers who use Dreamteck Splines.",
                                                             new ActionLink("https://discord.gg/bkYDq8v")));
            panels[3].elements.Add(new WindowPanel.Button(400, 30, "Contact Support",
                                                          new ActionLink("http://dreamteck.io/support/contact.php")));

            panels[4].elements.Add(new WindowPanel.Space(400, 10));
            var packagExists = false;
            var dir          = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/");
            if (Directory.Exists(dir))
                if (File.Exists(dir + "/Examples.unitypackage"))
                    packagExists = true;
            if (packagExists)
                panels[4].elements
                         .Add(new WindowPanel.Button(400, 30, "Install Examples", new ActionLink(InstallExamples)));
            else panels[4].elements.Add(new WindowPanel.Label("Examples package not found", null, Color.white));

            panels[5].elements.Add(new WindowPanel.Space(400, 10));
            packagExists = false;
            dir          = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/");
            if (Directory.Exists(dir))
                if (File.Exists(dir + "/PlaymakerActions.unitypackage"))
                    packagExists = true;
            if (packagExists)
                panels[5].elements
                         .Add(new WindowPanel.Button(400, 30, "Install Actions", new ActionLink(InstallPlaymaker)));
            else panels[5].elements.Add(new WindowPanel.Label("Playmaker actions not found", null, Color.white));
        }

        private void InstallExamples()
        {
            var dir = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/");
            AssetDatabase.ImportPackage(dir + "/Examples.unitypackage", false);
            EditorUtility.DisplayDialog("Import Complete", "Example scenes have been added to Dreamteck/Splines",
                                        "Yey!");
            panels[5].Back();
        }

        private void InstallPlaymaker()
        {
            var dir = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/");
            AssetDatabase.ImportPackage(dir + "/PlaymakerActions.unitypackage", false);
            EditorUtility.DisplayDialog("Import Complete",
                                        "Playmaker actions for Dreamteck Splines have been installed.", "Yey!");
            panels[4].Back();
        }

        private static bool IsBuildTargetInstalled(BuildTarget buildTarget)
        {
            var moduleManager = Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
            var isPlatformSupportLoaded =
                moduleManager.GetMethod("IsPlatformSupportLoaded", BindingFlags.Static | BindingFlags.NonPublic);
            var getTargetStringFromBuildTarget =
                moduleManager.GetMethod("GetTargetStringFromBuildTarget", BindingFlags.Static | BindingFlags.NonPublic);
            return (bool) isPlatformSupportLoaded.Invoke(null,
                                                         new object[]
                                                         {
                                                             (string) getTargetStringFromBuildTarget
                                                                .Invoke(null, new object[] {buildTarget})
                                                         });
        }
    }
}