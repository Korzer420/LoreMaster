using HutongGames.PlayMaker.Actions;
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

    #endregion

    #region Constructors

    public JonisProtectionPower() : base("Joni's Protection", Area.Cliffs)
    {
        CustomText = "Did you just took my blessing? How rude of you. First you banish me here and now that? Not cool, dude. Well, now that you have already took it, my prayers are with you....................... please don't dream nail me, dude.";
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the indicator if the bonus health can be taken.
    /// </summary>
    public bool ShouldEffectFreeze => _immune || DialogueFsm.Any(x => x.ActiveStateName.Equals("Box Up")) || !HeroController.instance.CanInput();

    /// <inheritdoc/>
    public override Action SceneAction => () =>
    {
        _immune = false;
        if (_runningCoroutine != null)
            LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(FadingLifeblood());
    };

    /// <summary>
    /// Get the dialogue fsms.
    /// </summary>
    public PlayMakerFSM[] DialogueFsm
    {
        get
        {
            if (_dialogueFSM == null)
                Initialize();
            return _dialogueFSM;
        }
    }

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
        if (string.Equals(self.FsmName, "Shop Region") || string.Equals(self.FsmName, "Stag Control"))
            self.GetState("Take Control").ReplaceAction(new Lambda(() =>_immune = true)
            { Name = "Force Immunity" });
        orig(self);
    }

    private void OnSendEventByNameAction(On.HutongGames.PlayMaker.Actions.SendEventByName.orig_OnEnter orig, SendEventByName self)
    {
        if (self.Fsm.GameObjectName.Contains("tablet") && string.Equals(self.Fsm.Name, "Inspection") && string.Equals(self.State.Name, "Prompt Up") && string.Equals(self.sendEvent.Value, "LORE PROMPT UP"))
            _immune = true;
        orig(self);
    }

    private void OnSetPlayerDataBoolAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.orig_OnEnter orig, SetPlayerDataBool self)
    {
        if (self.Fsm.GameObjectName.Contains("tablet") && string.Equals(self.Fsm.Name, "Inspection") && string.Equals(self.State.Name, "Regain Control"))
            _immune = false;
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        _dialogueFSM[0] = GameObject.Find("_GameCameras/HudCamera/DialogueManager").LocateMyFSM("Box Open");
        _dialogueFSM[1] = GameObject.Find("_GameCameras/HudCamera/DialogueManager").LocateMyFSM("Box Open YN");
        _dialogueFSM[2] = GameObject.Find("_GameCameras/HudCamera/DialogueManager").LocateMyFSM("Box Open Dream");
        On.HutongGames.PlayMaker.Actions.SendEventByName.OnEnter += OnSendEventByNameAction;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += OnSetPlayerDataBoolAction;
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
        FakeDamage = false;
    }

    /// <inheritdoc/>
    protected override void Terminate()
    {
        On.HutongGames.PlayMaker.Actions.SendEventByName.OnEnter -= OnSendEventByNameAction;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter -= OnSetPlayerDataBoolAction;
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
                FakeDamage = true;
                HeroController.instance.TakeHealth(1);
            }
        }
    }

    #endregion
}
