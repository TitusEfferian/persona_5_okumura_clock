using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class OkumuraWebPublish
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string BuildDir = "Builds/Web";
    private const string PcRpAssetPath = "Assets/Settings/PC_RPAsset.asset";
    private const string QualitySettingsPath = "ProjectSettings/QualitySettings.asset";
    private const string WebQualityPlatformKey = "WebGL";
    private const int WebQualityLevel = 1;

    [MenuItem("Okumura/Web/Apply Settings", false, 1)]
    public static void ApplySettings()
    {
        ApplyPlayerSettings();
        ApplyQualitySettings();
        ApplyUrpAssetSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("[Okumura] Web settings applied.");
    }

    [MenuItem("Okumura/Web/Build", false, 2)]
    public static void Build()
    {
        RunBuild();
    }

    [MenuItem("Okumura/Web/Apply Settings and Build", false, 3)]
    public static void ApplySettingsAndBuild()
    {
        ApplySettings();
        RunBuild();
    }

    // https://docs.unity3d.com/6000.5/Documentation/Manual/class-PlayerSettingsWebGL.html
    // https://docs.unity3d.com/6000.5/Documentation/Manual/web-optimization-player.html
    // https://docs.unity3d.com/6000.5/Documentation/ScriptReference/PlayerSettings.html
    // https://docs.unity3d.com/6000.5/Documentation/ScriptReference/PlayerSettings.WebGL.html
    // https://docs.unity3d.com/6000.5/Documentation/ScriptReference/WebGL.WasmCodeOptimization.html
    private static void ApplyPlayerSettings()
    {
        NamedBuildTarget web = NamedBuildTarget.WebGL;

        PlayerSettings.defaultWebScreenWidth = 1920;
        PlayerSettings.defaultWebScreenHeight = 1080;
        PlayerSettings.runInBackground = true;

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.nameFilesAsHashes = false;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        PlayerSettings.WebGL.showDiagnostics = false;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.WebGL.powerPreference = WebGLPowerPreference.Default;

        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.wasm2023 = true;
        PlayerSettings.WebGL.webAssemblyTable = true;
        PlayerSettings.WebGL.webAssemblyBigInt = true;
        PlayerSettings.WebGL.threadsSupport = false;
        PlayerSettings.WebGL.initialMemorySize = 32;
        PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;

        PlayerSettings.SetScriptingBackend(web, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(web, ApiCompatibilityLevel.NET_Standard);
        PlayerSettings.SetManagedStrippingLevel(web, ManagedStrippingLevel.High);
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.SetIl2CppCodeGeneration(web, Il2CppCodeGeneration.OptimizeSize);
        PlayerSettings.SetIl2CppCompilerConfiguration(web, Il2CppCompilerConfiguration.Master);
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, true);

#if UNITY_WEBGL
        UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;
#else
        Debug.LogWarning("[Okumura] Active platform is not Web. Switch to Web in Build Profiles and run Apply Settings again to set Code Optimization.");
#endif
    }

    // https://docs.unity3d.com/6000.5/Documentation/Manual/class-QualitySettings.html
    // https://docs.unity3d.com/6000.5/Documentation/ScriptReference/QualitySettings.html
    // https://docs.unity3d.com/6000.5/Documentation/ScriptReference/SerializedObject.html
    private static void ApplyQualitySettings()
    {
        UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath(QualitySettingsPath);
        if (objects == null || objects.Length == 0)
        {
            throw new InvalidOperationException("QualitySettings asset not found at " + QualitySettingsPath);
        }

        SerializedObject so = new SerializedObject(objects[0]);
        SerializedProperty map = so.FindProperty("m_PerPlatformDefaultQuality");
        if (map == null)
        {
            throw new InvalidOperationException("m_PerPlatformDefaultQuality not found on QualitySettings asset.");
        }

        bool found = false;
        for (int i = 0; i < map.arraySize; i++)
        {
            SerializedProperty entry = map.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("first").stringValue != WebQualityPlatformKey)
            {
                continue;
            }

            entry.FindPropertyRelative("second").intValue = WebQualityLevel;
            found = true;
            break;
        }

        if (!found)
        {
            map.arraySize++;
            SerializedProperty entry = map.GetArrayElementAtIndex(map.arraySize - 1);
            entry.FindPropertyRelative("first").stringValue = WebQualityPlatformKey;
            entry.FindPropertyRelative("second").intValue = WebQualityLevel;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(objects[0]);
    }

    // https://docs.unity3d.com/6000.5/Documentation/Manual/urp/universalrp-asset.html
    // https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.5/api/UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset.html
    private static void ApplyUrpAssetSettings()
    {
        UniversalRenderPipelineAsset rp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PcRpAssetPath);
        if (rp == null)
        {
            throw new InvalidOperationException("URP asset not found at " + PcRpAssetPath);
        }

        rp.supportsHDR = false;

        SerializedObject so = new SerializedObject(rp);
        SetBool(so, "m_MainLightShadowsSupported", false);
        SetBool(so, "m_AdditionalLightShadowsSupported", false);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(rp);
    }

    private static void SetBool(SerializedObject so, string name, bool value)
    {
        SerializedProperty p = so.FindProperty(name);
        if (p == null)
        {
            throw new InvalidOperationException("Property not found on URP asset: " + name);
        }

        p.boolValue = value;
    }

    // https://docs.unity3d.com/6000.5/Documentation/Manual/web-build-settings.html
    // https://docs.unity3d.com/6000.5/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html
    // https://docs.unity3d.com/6000.5/Documentation/ScriptReference/BuildPlayerOptions.html
    private static void RunBuild()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("Scene not found", ScenePath);
        }

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Directory.CreateDirectory(BuildDir);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = BuildDir,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            string message = "Web build " + summary.result + ": " + summary.totalErrors + " error(s).";
            EditorUtility.DisplayDialog("Okumura Web Build", message, "OK");
            throw new BuildFailedException(message);
        }

        Debug.Log("[Okumura] Web build succeeded: " + (summary.totalSize / (1024f * 1024f)).ToString("F1") + " MB in " + summary.totalTime + ". Output: " + Path.GetFullPath(BuildDir));
        EditorUtility.DisplayDialog("Okumura Web Build", "Build succeeded. Open File > Build Profiles and click Publish to Play to upload Builds/Web.", "OK");
    }
}
