using System;
using EnhancedShotgun.HintDisplay;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;

namespace EnhancedShotgun;

public sealed class EnhancedShotgunPlugin : Plugin<PluginConfig>
{
    private Localization? _localization;
    private IHintDisplayProvider? _hintDisplayProvider;
    private ShotgunTracker? _tracker;
    private Gr18Service? _gr18Service;
    private ReplacementService? _replacementService;
    private EnhancedShotgunService? _shotgunService;

    public override string Name => "EnhancedShotgun";

    public override string Description => "Adds one GR-18 enhanced shotgun and replaces normal shotgun acquisition with AKs.";

    public override string Author => "Codex";

    public override Version Version => new(1, 0, 0);

    public override Version RequiredApiVersion => new(LabApiProperties.CompiledVersion);

    public override void Enable()
    {
        if (!Config.IsEnabled)
        {
            Logger.Info($"{Name} is disabled by config.");
            return;
        }

        _localization = new Localization(Config);
        _hintDisplayProvider = HintDisplayProviderFactory.Create(Config.HintDisplay);
        _tracker = new ShotgunTracker();
        _gr18Service = new Gr18Service(Config, _tracker, _localization, _hintDisplayProvider);
        _replacementService = new ReplacementService(Config, _tracker, _localization, _hintDisplayProvider);
        _shotgunService = new EnhancedShotgunService(Config, _tracker, _localization, _hintDisplayProvider);

        _hintDisplayProvider.Enable();
        _gr18Service.Enable();
        _replacementService.Enable();
        _shotgunService.Enable();
    }

    public override void Disable()
    {
        _shotgunService?.Disable();
        _replacementService?.Disable();
        _gr18Service?.Disable();
        _hintDisplayProvider?.Disable();

        _shotgunService = null;
        _replacementService = null;
        _gr18Service = null;
        _tracker = null;
        _hintDisplayProvider = null;
        _localization = null;
    }
}
