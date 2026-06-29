using UnityEngine;

// ZeyWinAds native-loader bridge. Installed by the SDK configurator.
public sealed class OfferBootLoadingOverlay : MonoBehaviour
{
    public static void Show() => SetNativeOverlayVisible(true);
    public static void HoldForPossibleSdkWebView() => SetNativeOverlayVisible(true);
    public static void ExpectLocalWebView() => SetNativeOverlayVisible(true);
    public static void HideAfterNativeWebViewLocked() => SetNativeOverlayVisible(false);
    public static void HideNow() => SetNativeOverlayVisible(false);

    private static void SetNativeOverlayVisible(bool visible)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var startupOverlay = new AndroidJavaClass("com.zeywinads.unity.ZeyWinAdsStartupOverlay"))
            {
                startupOverlay.CallStatic("setLoadingOverlayVisible", visible);
            }
        }
        catch
        {
        }
#endif
    }
}
