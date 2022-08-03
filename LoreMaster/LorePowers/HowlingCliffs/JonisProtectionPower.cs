using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using Modding;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace LoreMaster.LorePowers.HowlingCliffs;

public class JonisProtectionPower : Power
{
    #region Members

    private int _currentLifebloodBonus = 0;

    private PlayMakerFSM[] _dialogueFSM = new PlayMakerFSM[3];

    private bool _immune;

    private bool _takingJoniBonus;

    #endregion

    #region Constructors

    public JonisProtectionPower() : base("Joni's Protection", Area.Cliffs)
    {
        CustomText = "Did you just took my blessing? How rude of you. First you banish me here and now that? Not cool, dude. Well, now that you have already took it, my prayers are with you....................... please don't dream nail me, dude.";
        Hint = "When going to a new area, you will receive the gift of Joni, which will quickly fade away.";
        Description = "When going to another area, you will be granted 5 life blood (10 if you have Joni's equipped). Each 3 seconds a lifeblood will fade away.";
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the indicator if the bonus health can be taken.
    /// </summary>
    public bool ShouldEffectFreeze => _immune || _dialogueFSM.Any(x => x.ActiveStateName.Equals("Box Up")) || !HeroController.instance.CanInput();

    /// <inheritdoc/>
    public override Action SceneAction => () =>
    {
        _immune = false;
        if (_runningCoroutine != null)
            LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(FadingLifeblood());
    };

    #endregion

    #region Event handler

    /// <summary>
    /// Event handler when the charms are updated.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="controller"></param>
    private void CharmUpdate(PlayerData data, HeroController controller)
    {
        if (_runningCoroutine != null)
        {
            LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
            _currentLifebloodBonus = 0;
        }
    }

    /// <summary>
    /// Event handler which takes away the health.
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    private int TakeHealth(int damage)
    {
        if (_currentLifebloodBonus > 0)
        {
            if (_currentLifebloodBonus - damage < 0)
                _currentLifebloodBonus = 0;
            else
                _currentLifebloodBonus -= damage;
        }
        return damage;
    }

    /// <summary>
    /// Remove the inventory invincibility.
    /// </summary>
    private void InvAnimateUpAndDown_AnimateDown(On.InvAnimateUpAndDown.orig_AnimateDown orig, InvAnimateUpAndDown self)
    {
        _immune = false;
        orig(self);
    }

    /// <summary>
    /// Grants immunity while the inventory is up, to prevent cancelling it.
    /// </summary>
    private void InvAnimateUpAndDown_AnimateUp(On.InvAnimateUpAndDown.orig_AnimateUp orig, InvAnimateUpAndDown self)
    {
        _immune = true;
        orig(self);
    }

    private void ModifyFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        // Shops and Stags should not be interrupted by this effect.
        if (self.FsmName.Equals("Shop Region") || self.FsmName.Equals("Stag Control"))
        {
            if (self.GetState("Take Control").GetFirstActionOfType<Lambda>() == null)
                self.GetState("Take Control").AddLastAction(new Lambda(() =>
                {
                    _immune = true;
                }));
        }
        else if (self.FsmName.Equals("Inspection") && self.gameObject.name.Contains("tablet"))
        {
            FsmState fsmState = self.GetState("Prompt Up");
            fsmState.ReplaceAction(new Lambda(() =>
            {
                PlayMakerFSM.BroadcastEvent("LORE PROMPT UP");
                _immune = true;
            })
            { Name = "Prompt Up Immunity" }, 7);

            fsmState = self.GetState("Regain Control");
            fsmState.ReplaceAction(new Lambda(() =>
            {
                _immune = false;
                PlayerData.instance.SetBool("disablePause", false);
            })
            { Name = "Remove Immunity" }, 1);
        }

        orig(self);
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        _dialogueFSM[0] = GameObject.Find("_GameCameras/HudCamera/DialogueManager").LocateMyFSM("Box Open");
        _dialogueFSM[1] = GameObject.Find("_GameCameras/HudCamera/DialogueManager").LocateMyFSM("Box Open YN");
        _dialogueFSM[2] = GameObject.Find("_GameCameras/HudCamera/DialogueManager").LocateMyFSM("Box Open Dream");

        // Ensures that the flower doesn't get destroyed instantly.
        PlayMakerFSM fsm = GameObject.Find("Knight").LocateMyFSM("ProxyFSM");
        fsm.GetState("Flower?").ReplaceAction(new Lambda(() =>
        {
            if ((Active && _takingJoniBonus)
            || !PlayerData.instance.GetBool(nameof(PlayerData.instance.hasXunFlower))
            || PlayerData.instance.GetBool(nameof(PlayerData.instance.xunFlowerBroken)))
            {
                if (_takingJoniBonus)
                    _takingJoniBonus = false;
                fsm.SendEvent("FINISHED");
            }
        })
        { Name = "Joni Block" }, 0);
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        ModHooks.CharmUpdateHook += CharmUpdate;
        ModHooks.TakeHealthHook += TakeHealth;
        On.PlayMakerFSM.OnEnable += ModifyFSM;
        On.InvAnimateUpAndDown.AnimateUp += InvAnimateUpAndDown_AnimateUp;
        On.InvAnimateUpAndDown.AnimateDown += InvAnimateUpAndDown_AnimateDown;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.CharmUpdateHook -= CharmUpdate;
        ModHooks.TakeHealthHook -= TakeHealth;
        On.PlayMakerFSM.OnEnable -= ModifyFSM;
        On.InvAnimateUpAndDown.AnimateUp -= InvAnimateUpAndDown_AnimateUp;
        On.InvAnimateUpAndDown.AnimateDown -= InvAnimateUpAndDown_AnimateDown;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Quickly fades away lifeblood.
    /// </summary>
    private IEnumerator FadingLifeblood()
    {
        int currentBonus = _currentLifebloodBonus;
        for (int i = 0; i < (PlayerData.instance.GetBool("equippedCharm_27") ? 10 : 5) - currentBonus; i++)
        {
            EventRegister.SendEvent("ADD BLUE HEALTH");
            _currentLifebloodBonus++;
        }

        while (_currentLifebloodBonus > 0)
        {
            yield return new WaitForSeconds(3f);
            if (ShouldEffectFreeze)
                yield return new WaitWhile(() => ShouldEffectFreeze);
            // This is an extra check for the case, that the lifeblood gets taken from other sources to prevent removing real masks.
            if (_currentLifebloodBonus > 0 && PlayerData.instance.GetInt(nameof(PlayerData.instance.healthBlue)) > 0)
            {
                _takingJoniBonus = true;
                HeroController.instance.TakeHealth(1);
            }
        }
    }

    #endregion
}
