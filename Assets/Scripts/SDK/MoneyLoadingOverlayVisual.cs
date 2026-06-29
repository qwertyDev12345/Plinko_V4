using UnityEngine;

// ZeyWinAds compatibility stub. Money loader drawing is Java-native now.
public sealed class MoneyLoadingOverlayVisual : MonoBehaviour
{
    public const float FillDurationSeconds = 8f;

    public static float EvaluateSteppedProgress(float value)
    {
        return Mathf.Clamp01(value);
    }

    public void Build(RectTransform _)
    {
    }
}
