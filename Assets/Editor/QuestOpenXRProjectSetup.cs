#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

public static class QuestOpenXRProjectSetup
{
    private const string SettingsDirectory = "Assets/XR/Settings";
    private const string SettingsPath = SettingsDirectory + "/XRGeneralSettingsPerBuildTarget.asset";
    private const string AndroidPlayerRoot =
        "C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/PlaybackEngines/AndroidPlayer";

    [MenuItem("Tools/Semantic Spatial Audio/Configure Meta Quest OpenXR")]
    public static void ConfigureFromMenu()
    {
        Configure();
    }

    public static void ConfigureForBatch()
    {
        bool succeeded = Configure();
        EditorApplication.Exit(succeeded ? 0 : 1);
    }

    private static bool Configure()
    {
        Directory.CreateDirectory(SettingsDirectory);
        XRGeneralSettingsPerBuildTarget perTarget =
            AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(SettingsPath);
        if (perTarget == null)
        {
            perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(perTarget, SettingsPath);
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perTarget, true);
        }

        if (!perTarget.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
            perTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);

        XRManagerSettings manager = perTarget.ManagerSettingsForBuildTarget(BuildTargetGroup.Android);
        bool loaderAssigned = XRPackageMetadataStore.AssignLoader(
            manager, "UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android);

        UnityEditor.XR.OpenXR.Features.FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
        OpenXRSettings openXR = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        if (openXR == null)
        {
            Debug.LogError("[QuestOpenXRSetup] Android OpenXR settings were not created.");
            return false;
        }

        OculusTouchControllerProfile touch = openXR.GetFeature<OculusTouchControllerProfile>();
        MetaQuestFeature meta = openXR.GetFeature<MetaQuestFeature>();
        if (touch != null) touch.enabled = true;
        if (meta != null) meta.enabled = true;
        if (touch != null) EditorUtility.SetDirty(touch);
        if (meta != null) EditorUtility.SetDirty(meta);

        SerializedObject playerSettings = (SerializedObject)typeof(PlayerSettings)
            .GetMethod("GetSerializedObject", BindingFlags.NonPublic | BindingFlags.Static)
            ?.Invoke(null, null);
        SerializedProperty activeInput = playerSettings?.FindProperty("activeInputHandler");
        if (activeInput != null)
        {
            activeInput.intValue = 2; // Both legacy Input Manager and the new Input System.
            playerSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,
            "com.semanticspatialaudio.adaptivedemo");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        ConfigureAndroidExternalTools();

        EditorUtility.SetDirty(perTarget);
        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(openXR);
        AssetDatabase.SaveAssets();

        Debug.Log(string.Format(
            "[QuestOpenXRSetup] Configured. loader={0}, touch={1}, metaQuest={2}, ARM64=true",
            loaderAssigned, touch != null && touch.enabled, meta != null && meta.enabled));
        return loaderAssigned && touch != null && touch.enabled && meta != null && meta.enabled;
    }

    private static void ConfigureAndroidExternalTools()
    {
        System.Type settingsType = System.Type.GetType(
            "UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions");
        if (settingsType == null)
        {
            Debug.LogWarning("[QuestOpenXRSetup] AndroidExternalToolsSettings type not available.");
            return;
        }

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        SetStaticProperty(settingsType, "sdkRootPath", AndroidPlayerRoot + "/SDK", flags);
        SetStaticProperty(settingsType, "ndkRootPath", AndroidPlayerRoot + "/NDK", flags);
        SetStaticProperty(settingsType, "jdkRootPath", AndroidPlayerRoot + "/OpenJDK", flags);
    }

    private static void SetStaticProperty(
        System.Type type, string propertyName, string value, BindingFlags flags)
    {
        PropertyInfo property = type.GetProperty(propertyName, flags);
        if (property != null && property.CanWrite) property.SetValue(null, value);
    }
}
#endif
