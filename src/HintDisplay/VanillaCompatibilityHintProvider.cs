using System.Collections.Generic;
using LabApi.Features.Wrappers;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace EnhancedShotgun.HintDisplay;

internal sealed class VanillaCompatibilityHintProvider : IHintDisplayProvider
{
    private const string LogPrefix = "[EnhancedShotgun:Hints]";
    private const float MinimumSendIntervalSeconds = 0.75f;

    private readonly Dictionary<string, float> _nextSendAt = new();

    public void Enable()
    {
        Logger.Warn($"{LogPrefix} RueI is unavailable and hint compatibility mode is enabled. Falling back to throttled vanilla hints.");
    }

    public void Disable()
    {
        _nextSendAt.Clear();
    }

    public void Show(Player player, string tag, string message, float duration)
    {
        string key = $"{player.ReferenceHub.netId}:{tag}";
        float now = Time.timeSinceLevelLoad;

        if (_nextSendAt.TryGetValue(key, out float nextAt) && now < nextAt)
        {
            return;
        }

        _nextSendAt[key] = now + MinimumSendIntervalSeconds;
        player.SendHint(message, duration);
    }

    public void Remove(Player player, string tag)
    {
        _nextSendAt.Remove($"{player.ReferenceHub.netId}:{tag}");
    }

    public void Clear(Player player)
    {
        string prefix = $"{player.ReferenceHub.netId}:";
        List<string> keysToRemove = new();

        foreach (string key in _nextSendAt.Keys)
        {
            if (key.StartsWith(prefix))
            {
                keysToRemove.Add(key);
            }
        }

        foreach (string key in keysToRemove)
        {
            _nextSendAt.Remove(key);
        }
    }
}
