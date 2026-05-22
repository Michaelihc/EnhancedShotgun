namespace EnhancedShotgun.HintDisplay;

internal static class HintDisplayProviderFactory
{
    public static IHintDisplayProvider Create(HintDisplayConfig config)
    {
        if (RueIHintDisplayProvider.IsRueILoaded)
        {
            return new RueIHintDisplayProvider();
        }

        if (config.CompatibilityMode)
        {
            return new VanillaCompatibilityHintProvider();
        }

        return new NullHintDisplayProvider("RueI.dll is not loaded or was not loaded before EnhancedShotgun.");
    }
}
