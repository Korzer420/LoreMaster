using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoreMaster.LorePowers.Dirtmouth;

public class TrueFormPower : Power
{
    #region Members

    private int _shadeState = 0;

    private List<Transform> _attackTransform = new List<Transform>();

    #endregion

    #region Constructors

    public TrueFormPower() : base("True Form", Area.Dirtmouth)
    {
        Hint = "While the true form is revealed, its vessels nail gets more powerful. Especially near your true self.";
        Description = "While your shade is active, you deal 30% more damage and increase your nail length by 25%. The effects are doubled, if you are in the same room as your shade.";
    }

    #endregion

    #region Event Handler

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.gameObject.name.Contains("Hollow Shade Death") && self.FsmName.Equals("Shade Control"))
            if (self.GetState("Blow").GetFirstActionOfType<Lambda>() == null)
            {
                self.GetState("Blow").AddLastAction(new Lambda(() =>
                {
                    ModifyNailLength(-.5f);
                    _shadeState = 0;
                    PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                }));
            }
        orig(self);
    }

    private int NailDamageUpdate(string name, int orig)
    {
        if (name.Equals("nailDamage"))
        {
            int dmgIncrease = 0;
            if (_shadeState != 0)
                dmgIncrease = Convert.ToInt32(orig * (_shadeState == 1 ? .3f : .6f));
            orig += dmgIncrease;
        }

        return orig;
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        GameObject attackDirections = GameObject.Find("Knight/Attacks");
        _attackTransform.Add(attackDirections.transform.Find("Slash"));
        _attackTransform.Add(attackDirections.transform.Find("AltSlash"));
        _attackTransform.Add(attackDirections.transform.Find("UpSlash"));
        _attackTransform.Add(attackDirections.transform.Find("DownSlash"));
        _attackTransform.Add(attackDirections.transform.Find("WallSlash"));
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        ModHooks.GetPlayerIntHook += NailDamageUpdate;
        LoreMaster.Instance.SceneActions.Add(PowerName, () =>
        {
            if (!PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)).Equals("None"))
            {
                float multiplier = .25f;
                // If we are going in the shade room, we increase the range buff
                if (PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)).Equals(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
                {
                    // If we have been already in the shade room (like dying in a room with the save bench), we ignore the multiplier.
                    if (_shadeState == 2)
                        return;
                    multiplier *= _shadeState == 1 ? 1 : 2;
                    _shadeState = 2;
                }
                else if (_shadeState != 1)
                {
                    // Depending where we are getting from, we lower or increase the range
                    multiplier *= _shadeState == 2 ? -1 : 1;
                    _shadeState = 1;
                }
                else
                    return;
                ModifyNailLength(multiplier);
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                return;
            }
        });

        LoreMaster.Instance.SceneActions[PowerName].Invoke();
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
        ModHooks.GetPlayerIntHook -= NailDamageUpdate;
        LoreMaster.Instance.SceneActions.Remove(PowerName);
        if (_shadeState != 0)
            ModifyNailLength(_shadeState == 1 ? -.25f : -.5f);
        _shadeState = 0;
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Change the nail hit ranges.
    /// </summary>
    /// <param name="multiplier"></param>
    private void ModifyNailLength(float multiplier)
    {
        try
        {
            for (int index = 0; index < 4; index++)
            {
                Vector3 currentScale = _attackTransform[index].GetComponent<NailSlash>().scale;
                _attackTransform[index].GetComponent<NailSlash>().scale = new Vector3(currentScale.x + multiplier, currentScale.y + multiplier, currentScale.z + multiplier);
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Couldn't update nail length: " + exception.Message);
        }
    }

    #endregion
}
