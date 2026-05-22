using LabApi.Features.Wrappers;

namespace EnhancedShotgun.HintDisplay;

public interface IHintDisplayProvider
{
    void Enable();

    void Disable();

    void Show(Player player, string tag, string message, float duration);

    void Remove(Player player, string tag);

    void Clear(Player player);
}
