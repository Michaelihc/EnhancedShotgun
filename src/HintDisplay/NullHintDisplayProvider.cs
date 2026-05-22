using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace EnhancedShotgun.HintDisplay;

internal sealed class NullHintDisplayProvider : IHintDisplayProvider
{
    private const string LogPrefix = "[EnhancedShotgun:Hints]";

    private readonly string _reason;
    private bool _logged;

    public NullHintDisplayProvider(string reason)
    {
        _reason = reason;
    }

    public void Enable()
    {
        LogOnce();
    }

    public void Disable()
    {
    }

    public void Show(Player player, string tag, string message, float duration)
    {
        LogOnce();
    }

    public void Remove(Player player, string tag)
    {
    }

    public void Clear(Player player)
    {
    }

    private void LogOnce()
    {
        if (_logged)
        {
            return;
        }

        _logged = true;
        Logger.Warn($"{LogPrefix} {_reason} No hint text will be displayed.");
    }
}
