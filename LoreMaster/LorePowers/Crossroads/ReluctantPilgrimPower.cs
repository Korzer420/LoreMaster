using LoreMaster.Enums;
using LoreMaster.Manager;
using Modding;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace LoreMaster.LorePowers.Crossroads;

public class ReluctantPilgrimPower : Power
{
    #region Constructors

    public ReluctantPilgrimPower() : base("Reluctant Pilgrim", Area.Crossroads) { }

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
                    && (x == 1 || (x == 2 && PowerManager.HasObtainedPower("TUT_TAB_02"))))
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

    /// <summary>
    /// Mute elegy objects.
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private GameObject MuteElegy(GameObject arg)
    {
        if (arg.name.Contains("Grubberfly Beam"))
        {
            AudioSource source = arg.GetComponent<AudioSource>();
            if (source != null)
                source.playOnAwake = false;
        }
        return arg;
    }

    private bool HeroController_CanAttack(On.HeroController.orig_CanAttack orig, HeroController self) => self && HeroController.instance.cState.onGround;

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        ModHooks.GetPlayerIntHook += GetBeamDamage;
        IL.HeroController.Attack += Attack_Modify;
        ModHooks.ObjectPoolSpawnHook += MuteElegy;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.GetPlayerIntHook -= GetBeamDamage;
        IL.HeroController.Attack -= Attack_Modify;
        ModHooks.ObjectPoolSpawnHook -= MuteElegy;
    }

    protected override void TwistEnable()
    {
        On.HeroController.CanAttack += HeroController_CanAttack;
    }

    protected override void TwistDisable()
    {
        On.HeroController.CanAttack -= HeroController_CanAttack;
    }

    #endregion
}
