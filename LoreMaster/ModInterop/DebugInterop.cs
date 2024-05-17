﻿using DebugMod;
using LoreMaster.LorePowers;
using LoreMaster.Manager;

namespace LoreMaster.ModInterop;

public static class DebugInterop
{
	#region Control

	internal static void Initialize() => DebugMod.DebugMod.AddToKeyBindList(typeof(DebugInterop));

    #endregion

    #region Methods

    /// <summary>
    /// Adds all powers and glyph slots.
    /// </summary>
    [BindableMethod(name = "Unlock all", category = "LoreMaster")]
    public static void UnlockAll()
	{
        LoreManager.Module.MajorGlyphSlots = 3;
        LoreManager.Module.MinorGlyphSlots = 5;
        LoreManager.Module.SmallGlyphSlots = 8;
        LoreManager.Module.AcquiredPowers.Clear();
        foreach (Power power in PowerManager.GetAllPowers())
            LoreManager.Module.AcquiredPowers.Add(power.PowerName);
        LorePage.UpdateLorePage();
        Console.AddLine("Unlocked all powers and glyphs.");
    }

    /// <summary>
    /// Disable all active powers, remove all glyph slots and powers.
    /// </summary>
    [BindableMethod(name = "Remove all", category = "LoreMaster")]
    public static void RemoveAll()
    {
        LoreManager.Module.MajorGlyphSlots = 0;
        LoreManager.Module.MinorGlyphSlots = 0;
        LoreManager.Module.SmallGlyphSlots = 0;
        foreach (Power power in PowerManager.GetAllActivePowers())
            power.DisablePower();
        LoreManager.Module.AcquiredPowers.Clear();
        LorePage.UpdateLorePage();
        Console.AddLine("Removed all powers and glyphs");
    }

    #endregion
}
