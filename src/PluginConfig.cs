namespace EnhancedShotgun;

public sealed class PluginConfig
{
    public bool IsEnabled { get; set; } = true;

    public string Language { get; set; } = string.Empty;

    public HintDisplayConfig HintDisplay { get; set; } = new();

    public float DamageMultiplier { get; set; } = 4f;

    public float PickupScale { get; set; } = 2f;

    public float PickupHintDuration { get; set; } = 5f;

    public float ReplacementHintDuration { get; set; } = 4f;

    public bool LockGr18InnerDoor { get; set; } = true;
}

public sealed class HintDisplayConfig
{
    public bool CompatibilityMode { get; set; } = false;
}
