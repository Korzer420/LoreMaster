using LoreMaster.Enums;
using Modding;
using MonoMod.Cil;
using System;

namespace LoreMaster.LorePowers.Crossroads;

public class ReluctantPilgerPower : Power
{
    #region Constructors

    public ReluctantPilgerPower() : base("Reluctant Pilger", Area.Crossroads)
    {
        Hint = "While you stay on the path, your nail shall receive the gift of the grubfather.";
        Description = "While standing of the ground, the grubberfly elegy effect is active regardless of your hp (joni conditions are still normal). If you have grubberfly equipped, the damage on ground is doubled instead.";
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the flag that determines, if the player is touching the ground
    /// </summary>
    public bool IsPlayerGrounded => (int)HeroController.instance.hero_state < 3;

    #endregion

    #region Event Handler

    /// <summary>
    /// Event handler that modifies the attack methods.
    /// </summary>
    /// <param name="il"></param>
    private void Attack_Modify(ILContext il)
    {
        ILCursor cursor = new ILCursor(il).Goto(0);

        // Modify left and right slash
        if (cursor.TryGotoNext(MoveType.After,
            x => x.MatchLdfld<HeroController>("playerData"),
            x => x.MatchLdstr("equippedCharm_35"),
            x => x.MatchCallvirt<PlayerData>("GetBool")
            ))
        {
            // Ignore the grubber fly charm condition
            cursor.EmitDelegate<Func<bool, bool>>((result) =>
                result || IsPlayerGrounded);

            if (cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdfld<HeroController>("playerData"),
                x => x.MatchLdstr("health"),
                x => x.MatchCallvirt<PlayerData>("GetInt")))
            {
                // Modify the health condition
                cursor.EmitDelegate<Func<int, int>>((x) =>
                {
                    if (!IsPlayerGrounded)
                        return x;

                    // We check for fury, in case we have one hp (or two with screw the rules), we want the fury version of grubber fly instead of the normal one. 
                    if (PlayerData.instance.GetBool("equippedCharm_6") 
                    && (x == 1 || (x == 2 && LoreMaster.Instance.ActivePowers.ContainsKey("TUT_TAB_02") && LoreMaster.Instance.ActivePowers["TUT_TAB_02"].Active)))
                        return x;
                    // Always causes grubberfly to trigger, if joni's is not equipped at least.
                    return PlayerData.instance.GetInt("maxHealth");
                });

                // Modify upslash
                if (cursor.TryGotoNext(MoveType.After,
                    x => x.MatchLdfld<HeroController>("playerData"),
                    x => x.MatchLdstr("equippedCharm_35"),
                    x => x.MatchCallvirt<PlayerData>("GetBool")
                    ))
                {
                    // Ignore the grubber fly charm condition
                    cursor.EmitDelegate<Func<bool, bool>>((result) =>
                        result || IsPlayerGrounded);

                    if (cursor.TryGotoNext(MoveType.After,
                        i => i.MatchLdfld<HeroController>("playerData"),
                        x => x.MatchLdstr("health"),
                        x => x.MatchCallvirt<PlayerData>("GetInt")))
                    {
                        // Modify the health condition
                        cursor.EmitDelegate<Func<int, int>>((x) =>
                        {
                            if (!IsPlayerGrounded)
                                return x;
                            if (PlayerData.instance.GetBool("equippedCharm_6") && x == 1)
                                return x;
                            return PlayerData.instance.GetInt("maxHealth");
                        });

                    }
                }
            }
        }
    }

    /// <summary>
    /// Event handler for getting the beam damage of grubber fly. If the player is grounded, it damage get's doubled.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="orig"></param>
    /// <returns></returns>
    private int GetBeamDamage(string name, int orig)
        => name.Equals("beamDamage") && IsPlayerGrounded && PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_35)) ? orig * 2 : orig;

    #endregion

    #region Protected Methods

    protected override void Enable()
    {
        ModHooks.GetPlayerIntHook += GetBeamDamage;
        IL.HeroController.Attack += Attack_Modify;
    }

    protected override void Disable()
    {
        ModHooks.GetPlayerIntHook -= GetBeamDamage;
        IL.HeroController.Attack -= Attack_Modify;
    }

    #endregion

}
