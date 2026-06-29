using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class DifficultyAndBackgroundTests
{
    private const string UnlockedMaxPref = "Diff_UnlockedMax";
    private const string SelectedPref = "Diff_Selected";

    [SetUp]
    public void ClearDifficultyPrefs()
    {
        PlayerPrefs.DeleteKey(UnlockedMaxPref);
        PlayerPrefs.DeleteKey(SelectedPref);
    }

    [TearDown]
    public void CleanupDifficultyPrefs()
    {
        PlayerPrefs.DeleteKey(UnlockedMaxPref);
        PlayerPrefs.DeleteKey(SelectedPref);
    }

    [Test]
    public void FreshDifficultyProgressStartsWithEveryDifficultyUnlocked()
    {
        var host = new GameObject("DifficultyControllerTest");
        try
        {
            var controller = host.AddComponent<DifficultyController>();
            InvokePrivate(controller, "LoadProgress");

            Assert.AreEqual((int)Difficulty.HIGH, GetPrivateInt(controller, "_unlockedMax"));
            Assert.AreEqual((int)Difficulty.LOW, GetPrivateInt(controller, "_selected"));
            Assert.AreEqual((int)Difficulty.HIGH, PlayerPrefs.GetInt(UnlockedMaxPref));
        }
        finally
        {
            Object.DestroyImmediate(host);
        }
    }

    [Test]
    public void BackgroundColorCycleReturnsToBlueEveryTenSeconds()
    {
        Color blue = BackgroundColorCycle.EvaluateColor(0f, 10f);
        Color halfway = BackgroundColorCycle.EvaluateColor(5f, 10f);
        Color nextCycle = BackgroundColorCycle.EvaluateColor(10f, 10f);

        Assert.AreEqual(new Color(0.08f, 0.30f, 1f, 1f), blue);
        Assert.AreEqual(new Color(0.05f, 0.78f, 0.32f, 1f), halfway);
        Assert.AreEqual(blue, nextCycle);
    }

    private static void InvokePrivate(object target, string methodName)
    {
        target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
    }

    private static int GetPrivateInt(object target, string fieldName)
    {
        return (int)target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    }
}
