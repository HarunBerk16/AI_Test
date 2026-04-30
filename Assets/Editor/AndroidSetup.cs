using UnityEditor;
using UnityEngine;

public class AndroidSetup
{
    public static void Execute()
    {
        // Switch to Android platform
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        // Player Settings
        PlayerSettings.companyName = "KamikazeDev";
        PlayerSettings.productName = "Kamikaze Evolution";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.kamikazedev.evolution");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        // Portrait orientation for mobile
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        // Graphics
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.gpuSkinning = true;

        AssetDatabase.SaveAssets();
        Debug.Log("Android platform ayarlari tamamlandi!");
    }
}
