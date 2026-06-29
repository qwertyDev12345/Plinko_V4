using System;
using System.Collections;
using UnityEngine;
using ZeyWinAds;
using ZeyWinAds.Core;

public class ZeyWinProvider : MonoBehaviour
{
    [SerializeField] private string apiKey;
    [SerializeField] private LogLevel logLevel = LogLevel.Info;
    [SerializeField] private float bannerHeightPx = 150f;

    private bool _adsEnabled = true;

    public bool IsInitialized => ZeyWinAds.ZeyWinAds.IsInitialized;
    public bool IsBannerReady => ZeyWinAds.ZeyWinAds.IsNativeReady() || ZeyWinAds.ZeyWinAds.IsBannerReady();
    public bool IsInterstitialReady => ZeyWinAds.ZeyWinAds.IsInterstitialReady();
    public bool IsRewardedReady => ZeyWinAds.ZeyWinAds.IsRewardedReady();
    
    private Coroutine _waitBannerRoutine;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log("[ZEYWIN] Start");

        ZeyWinAds.ZeyWinAds.SetLogLevel(logLevel);

        var resolvedApiKey = ResolveApiKey();

        Debug.Log($"[ZEYWIN] ApiKey = {resolvedApiKey}");

        if (!ZeyWinAds.ZeyWinAds.IsInitialized && !string.IsNullOrEmpty(resolvedApiKey))
        {
            Debug.Log("[ZEYWIN] Initialize");
            ZeyWinAds.ZeyWinAds.Initialize(resolvedApiKey);
        }
    }

    private string ResolveApiKey()
    {
        if (!string.IsNullOrWhiteSpace(apiKey))
            return apiKey;

        return !string.IsNullOrWhiteSpace(ZeyWinBuildConfig.ApiKey) ? ZeyWinBuildConfig.ApiKey : null;
    }

    public void Init(bool enabled)
    {
        Debug.Log($"[ZEYWIN] Init enabled={enabled}");

        _adsEnabled = enabled;

        if (!_adsEnabled)
        {
            Debug.Log("[ZEYWIN] Ads disabled");
            SetAdsDisabled(true);
            return;
        }

        Debug.Log("[ZEYWIN] Load all ads");

        LoadBanner();
        LoadInterstitial();
        LoadRewarded();
    }

    public void SetAdsDisabled(bool disabled)
    {
        _adsEnabled = !disabled;

        if (disabled)
        {
            HideBanner();
            return;
        }

        LoadBanner();
        LoadInterstitial();
        LoadRewarded();
    }

    public void LoadBanner()
    {
        Debug.Log("[ZEYWIN] LoadBanner");
        
        if (!_adsEnabled)
            return;

        ZeyWinAds.ZeyWinAds.LoadNative();
        ZeyWinAds.ZeyWinAds.LoadBanner();
    }

    public void LoadInterstitial()
    {
        Debug.Log("[ZEYWIN] LoadInterstitial");
        
        if (_adsEnabled)
            ZeyWinAds.ZeyWinAds.LoadInterstitial();
    }

    public void LoadRewarded()
    {
        Debug.Log("[ZEYWIN] LoadRewarded");
        
        if (_adsEnabled)
            ZeyWinAds.ZeyWinAds.LoadRewarded();
    }

    public void ShowBannerBottom()
    {
        Debug.Log("[ZEYWIN] ShowBannerBottom()");
        
        ShowBanner(BannerPosition.Bottom);
    }

    public void ShowBanner(BannerPosition position)
    {
        Debug.Log(
            $"Provider ShowBanner " +
            $"Native={ZeyWinAds.ZeyWinAds.IsNativeReady()} " +
            $"Banner={ZeyWinAds.ZeyWinAds.IsBannerReady()}"
        );
        
        if (!_adsEnabled)
            return;

        ZeyWinAds.ZeyWinAds.SetBannerHeights(bannerHeightPx, bannerHeightPx);
        ZeyWinAds.ZeyWinAds.EnableBanner();

        if (ZeyWinAds.ZeyWinAds.IsNativeReady())
        {
            ZeyWinAds.ZeyWinAds.ShowNative(position);
            return;
        }

        if (ZeyWinAds.ZeyWinAds.IsBannerReady())
        {
            ZeyWinAds.ZeyWinAds.ShowBanner(position);
            return;
        }

        LoadBanner();
        
        if (_waitBannerRoutine == null)
            _waitBannerRoutine = StartCoroutine(WaitBannerAndShow(position));
    }

    public void HideBanner()
    {
        ZeyWinAds.ZeyWinAds.HideNative();
        ZeyWinAds.ZeyWinAds.HideBanner();
    }

    public void HideSdkBannerInternal()
    {
        ZeyWinAds.ZeyWinAds.HideBanner();
    }

    public void ShowInterstitial(Action onClose = null)
    {
        if (!_adsEnabled)
        {
            onClose?.Invoke();
            return;
        }

        ZeyWinAds.ZeyWinAds.ShowInterstitial(onClose);
    }

    public void ShowRewarded(Action onReward = null, Action onClose = null)
    {
        if (!_adsEnabled)
        {
            onClose?.Invoke();
            return;
        }

        ZeyWinAds.ZeyWinAds.ShowRewarded(_ => onReward?.Invoke(), onClose);
    }
    
    private IEnumerator WaitBannerAndShow(BannerPosition position)
    {
        Debug.Log("[ZEYWIN] Waiting banner ready...");

        float timeout = 10f;
        float t = 0f;

        while (t < timeout)
        {
            if (ZeyWinAds.ZeyWinAds.IsNativeReady())
            {
                Debug.Log("[ZEYWIN] Native became ready -> ShowNative");
                ZeyWinAds.ZeyWinAds.ShowNative(position);
                _waitBannerRoutine = null;
                yield break;
            }

            if (ZeyWinAds.ZeyWinAds.IsBannerReady())
            {
                Debug.Log("[ZEYWIN] Banner became ready -> ShowBanner");
                ZeyWinAds.ZeyWinAds.ShowBanner(position);
                _waitBannerRoutine = null;
                yield break;
            }

            t += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        Debug.LogWarning("[ZEYWIN] Banner wait timeout");

        _waitBannerRoutine = null;
    }
}
