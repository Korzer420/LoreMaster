using DebugMod;
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
        PowerManager.PermanentPowers[0] = "Tourist";
        PowerManager.PermanentPowers[1] = "Greater Mind";
        PowerManager.PermanentPowers[2] = "Requiem";
        PowerManager.PermanentPowers[3] = "Stag Adoption";
        PowerManager.PermanentPowers[4] = "Follow the Light";

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
        for (int i = 0; i < 5; i++)
            PowerManager.PermanentPowers[i] = null;
        
        LoreManager.Module.AcquiredPowers.Clear();
        LorePage.UpdateLorePage();
        Console.AddLine("Removed all powers and glyphs");
    }

    #endregion
}
