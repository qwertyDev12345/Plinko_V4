using NUnit.Framework;

public sealed class MoneyLoadingOverlayVisualTests
{
    [Test]
    public void ProgressCurveRunsForwardForEightSecondsWithUnevenSteps()
    {
        Assert.AreEqual(8f, MoneyLoadingOverlayVisual.FillDurationSeconds);
        Assert.AreEqual(0f, MoneyLoadingOverlayVisual.EvaluateSteppedProgress(-0.25f));
        Assert.AreEqual(1f, MoneyLoadingOverlayVisual.EvaluateSteppedProgress(1.25f));

        float previous = MoneyLoadingOverlayVisual.EvaluateSteppedProgress(0f);
        float minStep = float.MaxValue;
        float maxStep = float.MinValue;

        for (int i = 1; i <= 16; i++)
        {
            float current = MoneyLoadingOverlayVisual.EvaluateSteppedProgress(i / 16f);
            Assert.GreaterOrEqual(current, previous);

            float step = current - previous;
            if (step > 0.0001f)
            {
                minStep = System.Math.Min(minStep, step);
                maxStep = System.Math.Max(maxStep, step);
            }

            previous = current;
        }

        Assert.Greater(maxStep, minStep * 1.5f);
    }
}
