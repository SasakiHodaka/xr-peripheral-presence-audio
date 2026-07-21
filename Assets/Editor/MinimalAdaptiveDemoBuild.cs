#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MinimalAdaptiveDemoBuild
{
    private const string ScenePath = "Assets/Scenes/SceneTokenMock.unity";
    private const string OutputPath = "Builds/MinimalAdaptiveDemo/MinimalAdaptiveDemo.exe";
    private const string QuestOutputPath = "Builds/Quest/MinimalAdaptiveDemo.apk";

    [MenuItem("Tools/Semantic Spatial Audio/Build Minimal Windows Demo")]
    public static void BuildFromMenu()
    {
        BuildWindows();
    }

    public static void BuildForBatch()
    {
        bool succeeded = BuildWindows();
        EditorApplication.Exit(succeeded ? 0 : 1);
    }

    [MenuItem("Tools/Semantic Spatial Audio/Build Meta Quest APK")]
    public static void BuildQuestFromMenu()
    {
        BuildQuest();
    }

    public static void BuildQuestForBatch()
    {
        bool succeeded = BuildQuest();
        EditorApplication.Exit(succeeded ? 0 : 1);
    }

    private static bool BuildWindows()
    {
        if (!File.Exists(ScenePath))
        {
            Debug.LogError("Minimal demo scene not found: " + ScenePath);
            return false;
        }

        string directory = Path.GetDirectoryName(OutputPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = OutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        if (summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("[MinimalAdaptiveDemoBuild] Failed: " + summary.result);
            return false;
        }

        Debug.Log(string.Format(
            "[MinimalAdaptiveDemoBuild] Succeeded: {0} bytes, {1:F1} seconds, output={2}",
            summary.totalSize, summary.totalTime.TotalSeconds, OutputPath));
        return true;
    }

    private static bool BuildQuest()
    {
        if (!File.Exists(ScenePath))
        {
            Debug.LogError("Minimal demo scene not found: " + ScenePath);
            return false;
        }

        string directory = Path.GetDirectoryName(QuestOutputPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
        {
            Debug.LogError("[MinimalAdaptiveDemoBuild] Could not switch to Android target.");
            return false;
        }

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = QuestOutputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        if (summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("[MinimalAdaptiveDemoBuild] Quest build failed: " + summary.result);
            return false;
        }

        Debug.Log(string.Format(
            "[MinimalAdaptiveDemoBuild] Quest APK succeeded: {0} bytes, {1:F1} seconds, output={2}",
            summary.totalSize, summary.totalTime.TotalSeconds, QuestOutputPath));
        return true;
    }
}
#endif
