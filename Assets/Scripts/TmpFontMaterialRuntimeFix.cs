using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TmpFontMaterialRuntimeFix
{
    private static Shader _tmpMobileShader;
    private static TMP_FontAsset _safeFontAsset;
    private static TMP_FontAsset _fallbackFontAsset;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        _tmpMobileShader = Shader.Find("TextMeshPro/Mobile/Distance Field") ??
                           Shader.Find("TextMeshPro/Distance Field");

        SceneManager.sceneLoaded += (_, __) => ApplyOnNextFrames();
        ApplyOnNextFrames();
    }

    private static void ApplyOnNextFrames()
    {
        var runner = new GameObject("TMP Font Material Runtime Fix");
        Object.DontDestroyOnLoad(runner);
        runner.hideFlags = HideFlags.HideAndDontSave;
        runner.AddComponent<Runner>().Run();
    }

    private static void Apply()
    {
        if (_tmpMobileShader == null)
            return;

        _safeFontAsset = FindSafeFontAsset();
        _fallbackFontAsset = FindFallbackFontAsset();

        foreach (var fontAsset in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
        {
            if (fontAsset == null)
                continue;

            ApplyMaterial(fontAsset.material);
            EnsureFallback(fontAsset);
        }

        foreach (var text in Resources.FindObjectsOfTypeAll<TMP_Text>())
        {
            if (text == null)
                continue;

            if (_safeFontAsset != null && ShouldReplaceFont(text.font))
            {
                text.font = _safeFontAsset;
                text.fontSharedMaterial = _safeFontAsset.material;
            }

            if (text.font != null)
                ApplyMaterial(text.font.material);

            ApplyTextMaterial(text);
            text.UpdateMeshPadding();
            text.SetAllDirty();
        }
    }

    private static TMP_FontAsset FindSafeFontAsset()
    {
        if (_safeFontAsset != null)
            return _safeFontAsset;

        TMP_FontAsset fallback = null;

        foreach (var fontAsset in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
        {
            if (fontAsset == null)
                continue;

            if (fontAsset.name.StartsWith("Roboto-VariableFont"))
                return fontAsset;

            if (fallback == null &&
                (fontAsset.name == "NotoSans-Regular SDF" ||
                 fontAsset.name == "Outfit-Regular SDF"))
            {
                fallback = fontAsset;
            }
        }

        return fallback ?? TMP_Settings.defaultFontAsset;
    }

    private static TMP_FontAsset FindFallbackFontAsset()
    {
        foreach (var fontAsset in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
        {
            if (fontAsset != null && fontAsset.name == "NotoSans-Regular SDF")
                return fontAsset;
        }

        return null;
    }

    private static bool ShouldReplaceFont(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null)
            return true;

        return fontAsset.name == "LiberationSans SDF";
    }

    private static void EnsureFallback(TMP_FontAsset fontAsset)
    {
        if (_fallbackFontAsset == null || fontAsset == _fallbackFontAsset)
            return;

        if (fontAsset.fallbackFontAssetTable == null)
            return;

        if (!fontAsset.fallbackFontAssetTable.Contains(_fallbackFontAsset))
            fontAsset.fallbackFontAssetTable.Add(_fallbackFontAsset);
    }

    private static void ApplyMaterial(Material material)
    {
        if (material == null || material.shader == null)
            return;

        if (material.shader.name.StartsWith("TextMeshPro/") && material.shader != _tmpMobileShader)
            material.shader = _tmpMobileShader;
    }

    private static void ApplyTextMaterial(TMP_Text text)
    {
        Material sharedMaterial = text.fontSharedMaterial;
        ApplyMaterial(sharedMaterial);

        if (sharedMaterial == null)
            return;

        try
        {
            ApplyMaterial(text.fontMaterial);
        }
        catch (System.ArgumentNullException)
        {
            // TMP may lazily clone a null source material on broken imported text objects.
        }
    }

    private sealed class Runner : MonoBehaviour
    {
        public void Run()
        {
            StartCoroutine(ApplyCoroutine());
        }

        private IEnumerator ApplyCoroutine()
        {
            Apply();
            yield return null;
            Apply();
            yield return null;
            Apply();
            Destroy(gameObject);
        }
    }
}
