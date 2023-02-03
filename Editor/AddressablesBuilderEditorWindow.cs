using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

public class AddressablesBuilderEditorWindow : EditorWindow
{
    public bool IsBuildingAddressables
    {
        get
        {
            return EditorPrefs.GetBool("EAddressBuildFlag", false);
        }
        set
        {
            EditorPrefs.SetBool("EAddressBuildFlag", value);
        }
    }
    
    public OperationQueue OpQueue
    {
        get{
            var json = EditorPrefs.GetString("AOpQueue",JsonUtility.ToJson(new OperationQueue()));
            return JsonUtility.FromJson<OperationQueue>(json);
        }
        set
        {
            EditorPrefs.SetString("AOpQueue",JsonUtility.ToJson(value));
        }
    }

    [MenuItem("Building/Build Content...")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        var window = (AddressablesBuilderEditorWindow)EditorWindow.GetWindow(typeof(AddressablesBuilderEditorWindow));
        window.Show();
    }

    private bool[] selectedPlatforms;
    private bool cleanBuild;

    BuildTarget[] platforms = new BuildTarget[]{BuildTarget.StandaloneOSX,BuildTarget.StandaloneWindows};

    void OnGUI()
    {
        GUILayout.BeginVertical();

        if (GUILayout.Button("Clear cache"))
        {
            UnityEngine.Caching.ClearCache();
        }
        
        GUILayout.Label("Select Platforms", EditorStyles.boldLabel);

        if(selectedPlatforms==null)
        {
            selectedPlatforms = new bool[platforms.Length];
        }

        for (int i = 0; i < platforms.Length; i++)
        {
            BuildTarget p = platforms[i];
            selectedPlatforms[i] = EditorGUILayout.Toggle(p.ToString(),selectedPlatforms[i]);
        }

        GUILayout.Space(5);

        cleanBuild = EditorGUILayout.Toggle("Clean Build",cleanBuild);
        cleanBuild = !EditorGUILayout.Toggle("Update Build",!cleanBuild);

        GUILayout.Space(5);
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.enabled = IsValidOptionSelection();
        var title =cleanBuild?"Clean Build":"Update Build";
        var buildNames = "";
        for (int i = 0; i < platforms.Length; i++)
            if(selectedPlatforms[i])    buildNames += platforms[i].ToString() + " ";

        bool scheduleBuild = false; //required to avoid EndLayout GUI error
        if(GUILayout.Button(title, GUILayout.Width(100)))
        {
            if(EditorUtility.DisplayDialog("Start " + title,$"Start {title} for {buildNames}","Yes","No"))
            {
                var queue = new List<AddressableOperationInfo>();
                for (int i = 0; i < platforms.Length; i++)
                {
                    if(selectedPlatforms[i])
                    {
                        var newOp = new AddressableOperationInfo();
                        newOp.buildTarget = platforms[i];
                        newOp.isCleanBuild = cleanBuild;
                        queue.Add(newOp);
                    }
                }
                OpQueue =   new OperationQueue(){ operations = queue};
                scheduleBuild = true;
            }
        }
        GUI.enabled=false;
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.Space(20);
        GUILayout.EndVertical();

        if (scheduleBuild)
        {
            scheduleBuild = false;
            IsBuildingAddressables = true;
            Next();
        }

    }

    private bool IsValidOptionSelection()
    {
        foreach(var s in selectedPlatforms)
        {
            if(s)
            {
                return true;
            }
        }
        return false;
    }

    public void Next()
    {
        var queue = new List<AddressableOperationInfo>(OpQueue.operations);
        if(queue.Count>0)
        {
            Debug.Log("processing " + queue[0]);
            var buildTarget = queue[0].buildTarget;
            var isCleanBuild = queue[0].isCleanBuild;
            queue.RemoveAt(0);
            OpQueue = new OperationQueue(){operations = queue};
            
            if(isCleanBuild)
            {
                AddressablesBuildLauncher.BuildPlatformAddressables(buildTarget);
            }
            else
            {
                AddressablesBuildLauncher.UpdateBuildPlatformAddressables(buildTarget);
            }

        }
        else
        {
            if (IsBuildingAddressables)
            {
                var runtimePlatform = Application.platform== RuntimePlatform.OSXEditor?BuildTarget.StandaloneOSX:BuildTarget.StandaloneWindows;
                Debug.Log("Reverting back to runtime platform");
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, runtimePlatform);
                IsBuildingAddressables = false;
            }
        }
    }
}

[System.Serializable]
public class OperationQueue
{
    public List<AddressableOperationInfo> operations;
}

[System.Serializable]
public class AddressableOperationInfo
{
    public BuildTarget buildTarget;
    public bool isCleanBuild;

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}
