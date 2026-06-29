using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using ZeyWinAds;
using ZeyWinAds.Editor;

public static class BuildGithubActionsApk
{
    private const string DefaultPackageName = "com.playsocialgames.plinkofun";
    private const string DefaultProductName = "Plinko Real Money";
    private const string DefaultCompanyName = "zeywin";
    private const string DefaultApiKey = "zw_5deabf1b2a9e7b450d9e9b5260a31374dcb6e62d20053c4b";
    private const string DefaultAdMobAppId = "ca-app-pub-6988952582458184~6613433438";
    private const string DefaultAdMobBanner = "ca-app-pub-6988952582458184/4530693980";
    private const string DefaultAdMobInterstitial = "ca-app-pub-6988952582458184/9211185343";
    private const string DefaultAdMobRewarded = "ca-app-pub-6988952582458184/7061184633";

    public static void BuildAndroid()
    {
        var outputPath = GetConfig("APK_OUTPUT_PATH", "apkOutputPath", null);
        if (string.IsNullOrEmpty(outputPath))
            outputPath = Path.GetFullPath("build/android/PlinkoRealMoney_com.playsocialgames.plinkofun_v1_c1.apk");

        BuildAndroidPlayer(outputPath, false, "APK");
    }

    public static void BuildAndroidAppBundle()
    {
        var outputPath = GetConfig("AAB_OUTPUT_PATH", "aabOutputPath", null);
        if (string.IsNullOrEmpty(outputPath))
            outputPath = Path.GetFullPath("build/android/PlinkoRealMoney_com.playsocialgames.plinkofun_v1.2.6_c10.aab");

        BuildAndroidPlayer(outputPath, true, "AAB");
    }

    private static void BuildAndroidPlayer(string outputPath, bool appBundle, string artifactKind)
    {
        ConfigureProject();

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        EditorUserBuildSettings.buildAppBundle = appBundle;

        try
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Start.unity", "Assets/Scenes/Game.unity" },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });

            var summary = report.summary;
            Debug.Log($"[ZeyWinActions] Build result: {summary.result}, size={summary.totalSize}, {artifactKind.ToLowerInvariant()}={outputPath}");

            if (summary.result != BuildResult.Succeeded)
                throw new Exception($"Android {artifactKind} build failed: {summary.result}");
        }
        finally
        {
            EditorUserBuildSettings.buildAppBundle = false;
        }
    }

    private static void ConfigureProject()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        PlayerSettings.companyName = DefaultCompanyName;
        var productName = GetConfig("ANDROID_PRODUCT_NAME", "androidProductName", null);
        if (string.IsNullOrEmpty(productName))
            productName = DefaultProductName;

        productName = productName.Replace('_', ' ');
        PlayerSettings.productName = productName;
        var versionName = GetConfig("ANDROID_VERSION_NAME", "androidVersionName", null);
        if (string.IsNullOrEmpty(versionName))
            versionName = "1.2.6";

        var versionCodeText = GetConfig("ANDROID_VERSION_CODE", "androidVersionCode", null);
        if (!int.TryParse(versionCodeText, out var versionCode))
            versionCode = 10;

        PlayerSettings.bundleVersion = versionName;
        var packageName = GetConfig("ANDROID_PACKAGE_NAME", "androidPackageName", null);
        if (string.IsNullOrEmpty(packageName))
            packageName = DefaultPackageName;

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, packageName);
        PlayerSettings.Android.bundleVersionCode = versionCode;
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;

        ConfigureAndroidCompatibility();
        ConfigureTextMeshProRendering();

        ConfigureKeystore();
        ConfigureZeyWinAds();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ConfigureAndroidCompatibility()
    {
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);

        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

        PlayerSettings.MTRendering = false;
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.Android.optimizedFramePacing = false;
        QualitySettings.vSyncCount = 0;

        Debug.Log("[ZeyWinActions] Android compatibility profile: minSdk=23, IL2CPP ARMv7+ARM64, OpenGLES3 only, Vulkan disabled.");
    }

    private static void ConfigureTextMeshProRendering()
    {
        var tmpMobileShader = Shader.Find("TextMeshPro/Mobile/Distance Field");
        if (tmpMobileShader == null)
            throw new Exception("[ZeyWinActions] Required TMP mobile shader was not found.");

        foreach (var guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fontAsset?.material == null)
                continue;

            if (fontAsset.material.shader != tmpMobileShader)
            {
                fontAsset.material.shader = tmpMobileShader;
                EditorUtility.SetDirty(fontAsset.material);
                EditorUtility.SetDirty(fontAsset);
            }
        }

        foreach (var guid in AssetDatabase.FindAssets("t:Material"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || material.shader == null)
                continue;

            if (material.shader.name.StartsWith("TextMeshPro/", StringComparison.Ordinal) &&
                material.shader != tmpMobileShader)
            {
                material.shader = tmpMobileShader;
                EditorUtility.SetDirty(material);
            }
        }

        Debug.Log("[ZeyWinActions] TMP font rendering locked to TextMeshPro/Mobile/Distance Field for Android.");
    }

    private static void ConfigureKeystore()
    {
        var keystorePath = GetConfig("ANDROID_KEYSTORE_PATH", "androidKeystorePath", null);
        var keystorePass = GetConfig("ANDROID_KEYSTORE_PASS", "androidKeystorePass", null);
        var keyAlias = GetConfig("ANDROID_KEYALIAS_NAME", "androidKeyaliasName", null);
        var keyAliasPass = GetConfig("ANDROID_KEYALIAS_PASS", "androidKeyaliasPass", null);

        if (string.IsNullOrEmpty(keystorePath) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTION")))
            keystorePath = "/github/workspace/.ci/keystore/user.keystore";
        if (string.IsNullOrEmpty(keystorePath))
            keystorePath = "/Volumes/Work/Plinko/ZeyWinSDK/user.keystore";
        if (string.IsNullOrEmpty(keystorePass))
            keystorePass = "12345654321";
        if (string.IsNullOrEmpty(keyAlias))
            keyAlias = "play max solutions ";
        if (string.IsNullOrEmpty(keyAliasPass))
            keyAliasPass = keystorePass;

        if (!File.Exists(keystorePath))
        {
            throw new FileNotFoundException($"[ZeyWinActions] Keystore not found at {keystorePath}. Product APK builds must not use Unity debug signing.", keystorePath);
        }

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = keystorePath;
        PlayerSettings.Android.keystorePass = keystorePass;
        PlayerSettings.Android.keyaliasName = keyAlias;
        PlayerSettings.Android.keyaliasPass = keyAliasPass;
        Debug.Log($"[ZeyWinActions] Android release signing configured with keystore: {keystorePath}, alias: {keyAlias}");
    }

    private static void ConfigureZeyWinAds()
    {
        var apiKey = GetConfig("ZEYWIN_API_KEY", "zeywinApiKey", null);
        if (string.IsNullOrEmpty(apiKey))
            apiKey = DefaultApiKey;
        if (string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTION")))
            throw new Exception("[ZeyWinActions] ZEYWIN_API_KEY is required for CI builds.");

        var args = new Dictionary<string, string>
        {
            ["productName"] = PlayerSettings.productName,
            ["companyName"] = DefaultCompanyName,
            ["androidPackageId"] = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android),
            ["androidVersionName"] = PlayerSettings.bundleVersion,
            ["androidVersionCode"] = PlayerSettings.Android.bundleVersionCode.ToString(),
            ["zeywinApiKey"] = apiKey,
            ["enableAdMob"] = "true",
            ["enableUmpConsent"] = "false",
            ["admobAndroidAppId"] = GetConfig("ADMOB_ANDROID_APP_ID", "admobAndroidAppId", DefaultAdMobAppId),
            ["admobAndroidBanner"] = GetConfig("ADMOB_ANDROID_BANNER_ID", "admobAndroidBannerId", DefaultAdMobBanner),
            ["admobAndroidInterstitial"] = GetConfig("ADMOB_ANDROID_INTERSTITIAL_ID", "admobAndroidInterstitialId", DefaultAdMobInterstitial),
            ["admobAndroidRewarded"] = GetConfig("ADMOB_ANDROID_REWARDED_ID", "admobAndroidRewardedId", DefaultAdMobRewarded),
        };

        ZeyWinAdsProjectConfigurator.Apply(args);

        Debug.Log("[ZeyWinActions] ZeyWin auto-start enabled before splash screen; Unity splash disabled.");
    }

    private static string GetConfig(string envName, string argName, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(envName);
        if (HasValue(value))
            return value;

        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-" + argName)
            {
                value = args[i + 1];
                if (HasValue(value))
                    return value;

                Debug.LogWarning($"[ZeyWinActions] Ignoring empty or invalid command line value for {argName}; using fallback.");
                return fallback;
            }
        }

        return fallback;
    }

    private static bool HasValue(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && !value.TrimStart().StartsWith("-", StringComparison.Ordinal);
    }
}
