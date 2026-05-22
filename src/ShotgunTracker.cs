using System.Collections.Generic;
using LabApi.Features.Wrappers;

namespace EnhancedShotgun;

public sealed class ShotgunTracker
{
    private readonly HashSet<ushort> _specialSerials = new();
    private int _specialCreationDepth;

    public bool HasSpecialShotgun => _specialSerials.Count > 0;

    public bool IsCreatingSpecial => _specialCreationDepth > 0;

    public void BeginSpecialCreation() => _specialCreationDepth++;

    public void EndSpecialCreation()
    {
        if (_specialCreationDepth > 0)
        {
            _specialCreationDepth--;
        }
    }

    public void Mark(ushort serial)
    {
        if (serial != 0)
        {
            _specialSerials.Add(serial);
        }
    }

    public void Unmark(ushort serial)
    {
        if (serial != 0)
        {
            _specialSerials.Remove(serial);
        }
    }

    public bool IsSpecial(ushort serial) => serial != 0 && _specialSerials.Contains(serial);

    public bool IsSpecial(Item? item) => item?.Type == ItemType.GunShotgun && IsSpecial(item.Serial);

    public bool IsSpecial(Pickup? pickup) => pickup?.Type == ItemType.GunShotgun && IsSpecial(pickup.Serial);
}
