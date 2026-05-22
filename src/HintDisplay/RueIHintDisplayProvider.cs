using System;
using System.Linq;
using System.Reflection;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace EnhancedShotgun.HintDisplay;

internal sealed class RueIHintDisplayProvider : IHintDisplayProvider
{
    private const string RueIAssemblyName = "RueI";
    private const float NoticePosition = 760f;
    private const string LogPrefix = "[EnhancedShotgun:Hints]";

    private Type? _displayType;
    private Type? _tagType;
    private Type? _basicElementType;
    private Type? _elementType;
    private MethodInfo? _getDisplayMethod;
    private MethodInfo? _showDurationMethod;
    private MethodInfo? _removeMethod;
    private ConstructorInfo? _tagConstructor;
    private ConstructorInfo? _basicElementConstructor;
    private bool _available;
    private bool _loggedUnavailable;

    public static bool IsRueILoaded => AppDomain.CurrentDomain.GetAssemblies()
        .Any(asm => string.Equals(asm.GetName().Name, RueIAssemblyName, StringComparison.OrdinalIgnoreCase));

    public void Enable()
    {
        TryInitializeRueI();
    }

    public void Disable()
    {
    }

    public void Show(Player player, string tag, string message, float duration)
    {
        if (!EnsureAvailable("show hint"))
        {
            return;
        }

        object? display = GetDisplay(player);
        object? element = CreateBasicElement(message);
        object? rueTag = CreateTag(tag);

        if (display == null || element == null || rueTag == null)
        {
            return;
        }

        InvokeRueI(() => _showDurationMethod!.Invoke(display, new object[] { rueTag, element, duration }));
    }

    public void Remove(Player player, string tag)
    {
        if (!EnsureAvailable("remove hint"))
        {
            return;
        }

        object? display = GetDisplay(player);
        object? rueTag = CreateTag(tag);

        if (display == null || rueTag == null)
        {
            return;
        }

        InvokeRueI(() => _removeMethod!.Invoke(display, new object[] { rueTag }));
    }

    public void Clear(Player player)
    {
        Remove(player, HintTags.Pickup);
        Remove(player, HintTags.Replacement);
        Remove(player, HintTags.Gr18Denied);
    }

    private object? GetDisplay(Player player)
    {
        try
        {
            return _getDisplayMethod!.Invoke(null, new object[] { player });
        }
        catch (Exception ex)
        {
            Logger.Error($"{LogPrefix} RueI failed to get display for {player.UserId}: {ex.GetBaseException().Message}");
            return null;
        }
    }

    private object? CreateBasicElement(string message)
    {
        try
        {
            return _basicElementConstructor!.Invoke(new object[] { NoticePosition, message });
        }
        catch (Exception ex)
        {
            Logger.Error($"{LogPrefix} RueI failed to create hint element: {ex.GetBaseException().Message}");
            return null;
        }
    }

    private object? CreateTag(string tag)
    {
        try
        {
            return _tagConstructor!.Invoke(new object[] { tag });
        }
        catch (Exception ex)
        {
            Logger.Error($"{LogPrefix} RueI failed to create hint tag '{tag}': {ex.GetBaseException().Message}");
            return null;
        }
    }

    private void TryInitializeRueI()
    {
        Assembly? assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(asm => string.Equals(asm.GetName().Name, RueIAssemblyName, StringComparison.OrdinalIgnoreCase));

        if (assembly == null)
        {
            LogUnavailable("RueI.dll is not loaded. Install RueI as a LabAPI plugin; EnhancedShotgun will not display hints.");
            return;
        }

        _displayType = assembly.GetType("RueI.API.RueDisplay");
        _tagType = assembly.GetType("RueI.API.Elements.Tag");
        _basicElementType = assembly.GetType("RueI.API.Elements.BasicElement");
        _elementType = assembly.GetType("RueI.API.Elements.Element");

        if (_displayType == null || _tagType == null || _basicElementType == null || _elementType == null)
        {
            LogUnavailable("RueI is loaded but its public API types were not found. EnhancedShotgun will not display hints.");
            return;
        }

        _getDisplayMethod = _displayType.GetMethod("Get", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player) }, null);
        _showDurationMethod = _displayType.GetMethod("Show", BindingFlags.Public | BindingFlags.Instance, null, new[] { _tagType, _elementType, typeof(float) }, null);
        _removeMethod = _displayType.GetMethod("Remove", BindingFlags.Public | BindingFlags.Instance, null, new[] { _tagType }, null);
        _tagConstructor = _tagType.GetConstructor(new[] { typeof(string) });
        _basicElementConstructor = _basicElementType.GetConstructor(new[] { typeof(float), typeof(string) });

        if (_getDisplayMethod == null || _showDurationMethod == null || _removeMethod == null ||
            _tagConstructor == null || _basicElementConstructor == null)
        {
            LogUnavailable("RueI is loaded but required methods/constructors were not found. EnhancedShotgun will not display hints.");
            return;
        }

        _available = true;
        Logger.Info($"{LogPrefix} RueI detected; EnhancedShotgun hints will use RueI displays.");
    }

    private bool EnsureAvailable(string action)
    {
        if (_available)
        {
            return true;
        }

        LogUnavailable($"Cannot {action}: RueI is not available. No vanilla hint fallback will be displayed.");
        return false;
    }

    private void LogUnavailable(string message)
    {
        if (_loggedUnavailable)
        {
            return;
        }

        _loggedUnavailable = true;
        Logger.Error($"{LogPrefix} {message}");
    }

    private static void InvokeRueI(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Logger.Error($"{LogPrefix} RueI invocation failed: {ex.GetBaseException().Message}");
        }
    }
}
