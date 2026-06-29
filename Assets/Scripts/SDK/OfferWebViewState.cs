using System;
using System.Collections.Generic;

public static class OfferWebViewState
{
    private static readonly HashSet<string> ActiveSources = new HashSet<string>();

    public static event Action<bool, string, string> Changed;

    public static bool IsActive => ActiveSources.Count > 0;

    public static void SetActive(string source, bool active, string url = null)
    {
        if (string.IsNullOrEmpty(source))
            source = "unknown";

        bool wasActive = IsActive;
        bool changed = active
            ? ActiveSources.Add(source)
            : ActiveSources.Remove(source);

        if (!changed && wasActive == IsActive)
            return;

        Changed?.Invoke(IsActive, source, url);
    }
}
