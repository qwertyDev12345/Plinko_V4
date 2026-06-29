using UnityEngine;

// ZeyWinAds compatibility stub. The visible loader is SDK-owned Java-native UI.
public sealed class LoadingProgressUI : MonoBehaviour
{
    private void Awake()
    {
        HideLegacyLoaderHierarchy();
    }

    private void OnEnable()
    {
        HideLegacyLoaderHierarchy();
    }

    public void Show()
    {
        HideLegacyLoaderHierarchy();
    }

    public void Hide()
    {
        HideLegacyLoaderHierarchy();
    }

    private void HideLegacyLoaderHierarchy()
    {
        var canvas = GetComponentInParent<Canvas>(true);
        if (canvas != null && canvas.gameObject.activeSelf)
        {
            canvas.gameObject.SetActive(false);
            return;
        }

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}
