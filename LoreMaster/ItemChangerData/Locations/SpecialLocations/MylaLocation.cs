using System.Linq;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

/// <summary>
/// Best girl <3
/// </summary>
internal class MylaLocation : DialogueLocation
{
    protected override void OnLoad()
    {
        base.OnLoad();
        On.DeactivateIfPlayerdataTrue.OnEnable += ForceMyla;
        On.DeactivateIfPlayerdataFalse.OnEnable += PreventMylaZombie;
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        On.DeactivateIfPlayerdataTrue.OnEnable -= ForceMyla;
        On.DeactivateIfPlayerdataFalse.OnEnable -= PreventMylaZombie;
    }

    /// <summary>
    /// Despawns mylas other versions.
    /// </summary>
    private void PreventMylaZombie(On.DeactivateIfPlayerdataFalse.orig_OnEnable orig, DeactivateIfPlayerdataFalse self)
    {
        if ((self.gameObject.name.Contains("Zombie Myla") || string.Equals(self.gameObject.name, "Myla Crazy NPC")) && !Placement.Items.All(x => x.IsObtained()))
        {
            self.gameObject.SetActive(false);
            return;
        }
        orig(self);
    }

    /// <summary>
    /// Forces Myla (best character btw) to always appear, like she should.
    /// </summary>
    private void ForceMyla(On.DeactivateIfPlayerdataTrue.orig_OnEnable orig, DeactivateIfPlayerdataTrue self)
    {
        if (string.Equals(self.gameObject.name, "Miner") && !Placement.Items.All(x => x.IsObtained()))
            return;
        orig(self);
    }
}
