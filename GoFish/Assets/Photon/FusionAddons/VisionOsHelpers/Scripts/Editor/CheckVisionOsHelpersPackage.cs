#if UNITY_EDITOR
using Fusion.XRShared.Tools;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class CheckVisionOsHelpersPackage
{
    static string[] packagesToSearch = new string[] { "com.unity.polyspatial", "com.unity.polyspatial.visionos", "com.unity.polyspatial.xr" };
    static string[] packagesToInstall = new string[] { "com.unity.polyspatial", "com.unity.polyspatial.visionos", "com.unity.polyspatial.xr" };
    const string DEFINE = "POLYSPATIAL_SDK_AVAILABLE";

    static CheckVisionOsHelpersPackage()
    {
        var packageRequest = new PackagePresenceCheck(packagesToSearch, (packageInfoList) => {
            if(packageInfoList.Count == packagesToSearch.Length)
            {
                // all packages are available
                var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                if (defines.Contains(DEFINE) == false) { defines = $"{defines};{DEFINE}"; }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
            } else
            {
                string packagesStr = (packagesToInstall.Length <= 1) ? "package " : "packages ";
                packagesStr += string.Join(", ", packagesToInstall);
                Debug.LogError($"For the VisionOS helpers work, you need to install {packagesStr} (available in Unity 2022.x)");
            }
        });
    }
}
#endif
