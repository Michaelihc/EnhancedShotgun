using LabApi.Features.Wrappers;

namespace EnhancedShotgun;

public enum MessageKey
{
    PickedUpEnhancedShotgun,
    NormalShotgunReplacedWithAk,
    NormalShotgunReplacedWithA7,
    Gr18NeedsO5,
}

public sealed class Localization
{
    private readonly PluginConfig _config;

    public Localization(PluginConfig config)
    {
        _config = config;
    }

    public string Get(Player? player, MessageKey key)
    {
        return ResolveLanguage(player) == "en" ? GetEnglish(key) : GetChinese(key);
    }

    private string ResolveLanguage(Player? player)
    {
        string configured = (_config.Language ?? string.Empty).Trim().ToLowerInvariant();
        if (configured is "en" or "english")
        {
            return "en";
        }

        if (configured is "cn" or "zh" or "chinese")
        {
            return "cn";
        }

        return "cn";
    }

    private static string GetChinese(MessageKey key)
    {
        return key switch
        {
            MessageKey.PickedUpEnhancedShotgun => "你已拾取超级无敌散弹枪，伤害四倍",
            MessageKey.NormalShotgunReplacedWithAk => "普通散弹枪已被禁用，已替换为 AK。",
            MessageKey.NormalShotgunReplacedWithA7 => "掠夺者的普通散弹枪已被禁用，已替换为 A7。",
            MessageKey.Gr18NeedsO5 => "GR-18 的门需要 O5 级权限。",
            _ => string.Empty,
        };
    }

    private static string GetEnglish(MessageKey key)
    {
        return key switch
        {
            MessageKey.PickedUpEnhancedShotgun => "You picked up the super shotgun. Damage is quadrupled.",
            MessageKey.NormalShotgunReplacedWithAk => "Normal shotguns are disabled and were replaced with an AK.",
            MessageKey.NormalShotgunReplacedWithA7 => "The Marauder shotgun is disabled and was replaced with an A7.",
            MessageKey.Gr18NeedsO5 => "The GR-18 gate requires O5-level access.",
            _ => string.Empty,
        };
    }
}
