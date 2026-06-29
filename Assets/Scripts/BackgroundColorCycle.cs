using UnityEngine;
using UnityEngine.UI;

public class BackgroundColorCycle : MonoBehaviour
{
    public const float DefaultCycleSeconds = 10f;

    private static readonly Color Blue = new Color(0.08f, 0.30f, 1f, 1f);
    private static readonly Color Green = new Color(0.05f, 0.78f, 0.32f, 1f);

    [SerializeField] private float cycleSeconds = DefaultCycleSeconds;
    [SerializeField] private Image targetImage;
    [SerializeField] private Image[] targetImages;
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private SpriteRenderer[] targetSpriteRenderers;
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetSpriteRenderer == null)
            targetSpriteRenderer = GetComponent<SpriteRenderer>();

        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        AutoBindVisibleBackgrounds();
    }

    private void Update()
    {
        Color color = EvaluateColor(Time.time, cycleSeconds);

        if (targetImage != null)
            targetImage.color = color;

        Apply(color, targetImages);

        if (targetSpriteRenderer != null)
            targetSpriteRenderer.color = color;

        Apply(color, targetSpriteRenderers);

        if (targetCamera != null)
            targetCamera.backgroundColor = color;
    }

    public static Color EvaluateColor(float timeSeconds, float cycleDurationSeconds)
    {
        float duration = Mathf.Max(0.01f, cycleDurationSeconds);
        float phase = Mathf.Repeat(timeSeconds, duration) / duration;
        float t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(phase * 2f, 1f));
        return Color.Lerp(Blue, Green, t);
    }

    private void AutoBindVisibleBackgrounds()
    {
        if (targetImage == null)
            targetImage = FindCanvasBackgroundImage();

        if (targetImages == null || targetImages.Length == 0)
        {
            GameObject canvasBg = GameObject.Find("Canvas BG");
            if (canvasBg != null)
                targetImages = canvasBg.GetComponentsInChildren<Image>(true);
        }
    }

    private static Image FindCanvasBackgroundImage()
    {
        GameObject canvasBg = GameObject.Find("Canvas BG");
        if (canvasBg == null)
            return null;

        return canvasBg.GetComponentInChildren<Image>(true);
    }

    private static void Apply(Color color, Image[] images)
    {
        if (images == null)
            return;

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null)
                images[i].color = color;
        }
    }

    private static void Apply(Color color, SpriteRenderer[] renderers)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = color;
        }
    }
}
