#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine;
using System.Net;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Linq;

public enum AddressableOperation
{
    None =0,
    CleanBuild =1,
    UpdateBuild =2
}

public class AddressablesBuildLauncher : UnityEditor.Build.IActiveBuildTargetChanged
{

    public const string Host = "ftp://192.168.1.88:21";
    public const string UserId = "robert";
    public const string Password = "recoil1979";
    private static FTPUtils ftpClient;    
    public static string build_script 
        = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
    public static string settings_asset 
        = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
    public static string profile_name = "Default";
    private static AddressableAssetSettings settings;

    public static AddressableOperation NextAddressableOperation
    {
        get{
            return (AddressableOperation)EditorPrefs.GetInt("cbuildTarget",0);
        }
        set
        {
            EditorPrefs.SetInt("cbuildTarget",(int)value);
        }
    }
    public int callbackOrder { get { return 0; } }

    #region Addressables Settup
    static void GetSettingsObject(string settingsAsset) {
        // This step is optional, you can also use the default settings:
        //settings = AddressableAssetSettingsDefaultObject.Settings;
    
        settings
            = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset)
                as AddressableAssetSettings;
        
        if (settings == null)
            Debug.LogError($"{settingsAsset} couldn't be found or isn't " +
                            $"a settings object.");
    }

    static void SetProfile(string profile) {
        string profileId = settings.profileSettings.GetProfileId(profile);
        if (String.IsNullOrEmpty(profileId))
            Debug.LogWarning($"Couldn't find a profile named, {profile}, " +
                                $"using current profile instead.");
        else
            settings.activeProfileId = profileId;
    }

    static void SetBuilder(IDataBuilder builder) {
        int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);

        if (index > 0)
            settings.ActivePlayerDataBuilderIndex = index;
        else
            Debug.LogWarning($"{builder} must be added to the " +
                                $"DataBuilders list before it can be made " +
                                $"active. Using last run builder instead.");
    }

    private static string GetBuildPath()
    {
        GetSettingsObject(settings_asset);
        SetProfile(profile_name);
        var path = settings.RemoteCatalogBuildPath.GetValue(settings);
        return Application.dataPath.Replace("Assets",path);
    }

    #endregion

    [MenuItem("Addressables/Build Addressables only/Win")]
    public static void BuildWindowsAddressables() {
        BuildPlatformAddressables(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Addressables/Build Addressables only/Mac")]
    public static void BuildMacAddressables()
    {
        BuildPlatformAddressables(BuildTarget.StandaloneOSX);
    }

    public static void BuildPlatformAddressables(BuildTarget buildTarget)
    {
        NextAddressableOperation = AddressableOperation.CleanBuild;
        if(EditorUserBuildSettings.activeBuildTarget == buildTarget)
        {
            BuildTargetChanged(BuildTarget.StandaloneOSX);
            return;
        }
        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, buildTarget);
        Debug.Log("Switching to " + buildTarget);
    }

    public static void UpdateBuildPlatformAddressables(BuildTarget buildTarget)
    {
        NextAddressableOperation = AddressableOperation.UpdateBuild;
        if(EditorUserBuildSettings.activeBuildTarget == buildTarget)
        {
            BuildTargetChanged(BuildTarget.StandaloneOSX);
            return;
        }
        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, buildTarget);
        Debug.Log("Switching to " + buildTarget);
    }

    private static bool CleanBuildAddressables() {
        GetSettingsObject(settings_asset);
        SetProfile(profile_name);

        if(Directory.Exists(GetBuildPath()))
        {
            Directory.Delete(GetBuildPath(),true); //Clear build folder
        }

        IDataBuilder builderScript = AssetDatabase.LoadAssetAtPath<ScriptableObject>(build_script) as IDataBuilder;
        
        if (builderScript == null) {
            Debug.LogError(build_script + " couldn't be found or isn't a build script.");
            return false;
        }

        builderScript.ClearCachedData();
        AddressableAssetSettings.CleanPlayerContent(builderScript);
        SetBuilder(builderScript);

        AddressableAssetSettings
            .BuildPlayerContent(out AddressablesPlayerBuildResult result);
        bool success = string.IsNullOrEmpty(result.Error);

        if (!success) {
            Debug.LogError("Addressables build error encountered: " + result.Error);
        }
        return success;
    }

    private static bool UpdateBuildAddressables() {
        GetSettingsObject(settings_asset);
        SetProfile(profile_name);

        IDataBuilder builderScript = AssetDatabase.LoadAssetAtPath<ScriptableObject>(build_script) as IDataBuilder;
        
        if (builderScript == null) {
            Debug.LogError(build_script + " couldn't be found or isn't a build script.");
            return false;
        }

        builderScript.ClearCachedData();
        AddressableAssetSettings.CleanPlayerContent(builderScript);
        SetBuilder(builderScript);

        var path = ContentUpdateScript.GetContentStateDataPath(false);
        Debug.Log(path);
        if (!string.IsNullOrEmpty(path))
        {
            var result = ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);
             return string.IsNullOrEmpty(result.Error);
        }
        else
        {
            return false;
        }
    }

    public async static Task UploadPlatform(string platformTarget)
    {
        ftpClient = new FTPUtils(Host,UserId,Password);
        ftpClient.DeleteFolder($"cdn/{platformTarget}");
        ftpClient.createDirectory($"cdn/{platformTarget}");
        var ftp = new FTPUtils(Host,UserId,Password);
        await ftp.UploadDirectory(GetBuildPath(),$"cdn/{platformTarget}");
        Debug.Log("Done! " + Application.platform);
    }


    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    {
        BuildTargetChanged(newTarget);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public async static void BuildTargetChanged(BuildTarget newTarget)
    {
        if(NextAddressableOperation == AddressableOperation.CleanBuild) //change this to a queued process name so we can start a new build or just an updae
        {
            NextAddressableOperation = AddressableOperation.None;
            AssetDatabase.SaveAssets();
            if(CleanBuildAddressables())
            {
                await UploadPlatform(newTarget.ToString());
            }
        }

        if(NextAddressableOperation == AddressableOperation.UpdateBuild)
        {
            NextAddressableOperation = AddressableOperation.None;
            AssetDatabase.SaveAssets();
            if(UpdateBuildAddressables())
            {
                await UploadPlatform(newTarget.ToString());  
            }
        }

        var s = (AddressablesBuilderEditorWindow)EditorWindow.GetWindow(typeof(AddressablesBuilderEditorWindow));
        if(s!=null)
        {
            s.Next();
        }
    }
}
#endif
