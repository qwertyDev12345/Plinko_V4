using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace ScriptedBuild
{
    public class BuildMobile : AbstractBuild
    {
        public new static void BuildOptions()
        {
            SetScenes(new List<string>
            {
                "Assets/Scenes/Start.unity",
                "Assets/Scenes/Game.unity",
            });

            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.WebGL.template = "PROJECT:Dbd";

            ParseCommandLineArguments(out var options);
            if (!options.TryGetValue("customBuildPath", out var buildPath))
                options.TryGetValue("buildPath", out buildPath);

            EditorUserBuildSettings.buildAppBundle =
                !string.IsNullOrEmpty(buildPath) &&
                Path.GetExtension(buildPath).ToLowerInvariant() == ".aab";

            AbstractBuild.BuildOptions();
        }
    }
}
