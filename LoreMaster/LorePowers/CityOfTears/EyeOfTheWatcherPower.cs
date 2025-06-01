using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.Helper;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

public class EyeOfTheWatcherPower : Power
{
    #region Members

    private Sprite _eyeSprite = SpriteHelper.CreateSprite<LoreMaster>("Base.EyeOfLurien");

    private GameObject _eye;

    private bool _adjustedDarkness = true;

    #endregion

    #region Constructors

    public EyeOfTheWatcherPower() : base("Eye Of The Watcher", Area.CityOfTears)
    {

    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public override Action SceneAction => () =>
    {
        if (_eye != null)
            _eye.GetComponent<SpriteRenderer>().color = Color.white;
        _adjustedDarkness = false;
    };

    /// <summary>
    /// Gets the indicator if the player can be revived.
    /// </summary>
    public bool CanRevive => EyeActive && PlayerData.instance.GetBool(nameof(PlayerData.instance.hasLantern));

    /// <summary>
    /// Gets or sets the value that indicates if the eye is active
    /// </summary>
    public bool EyeActive { get; set; } = true;

    /// <summary>
    /// Gets the eye object.
    /// </summary>
    public GameObject Eye
    {
        get
        {
            if (_eye == null)
            {
                _eye = new("Eye of Lurien");
                _eye.transform.SetParent(HeroController.instance?.transform);
                _eye.AddComponent<SpriteRenderer>().sprite = _eyeSprite;
                _eye.transform.localPosition = new(0, 2f);
                _eye.transform.localScale = new(.5f, .5f);
            }
            return _eye;
        }
    }

    #endregion

    #region Event Handler

    private IEnumerator HeroController_Die(On.HeroController.orig_Die orig, HeroController self)
    {
        if (!CanRevive)
            yield return orig(self);
        else
        {
            EyeActive = false;
            if (PlayerData.instance.GetBool("equippedCharm_27"))
            {
                HeroController.instance.AddHealth(1);
                for (int i = 0; i < 5; i++)
                    EventRegister.SendEvent("ADD BLUE HEALTH");
            }
            else
                HeroController.instance.AddHealth(PlayerData.instance.GetInt("maxHealth") - PlayerData.instance.GetInt("health"));
            LoreMaster.Instance.Handler.StartCoroutine(UpdateUI());
        }
    }

    private void OnActivateGameObjectAction(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_OnEnter orig, ActivateGameObject self)
    {
        if (self.IsCorrectContext("Darkness Control", "Vignette", "Scene Reset 2"))
        {
            if (State == PowerState.Active && !self.Fsm.FsmComponent.GetState("Scene Reset 2").Transitions.Any(x => string.Equals(x.ToState, "Lantern 2")))
                self.Fsm.FsmComponent.GetState("Scene Reset 2").AddTransition("LANTERN", "Lantern 2");
            if (State == PowerState.Active && EyeActive)
                self.Fsm.FsmComponent.SendEvent("LANTERN");
        }

        orig(self);
    }

    private void OnSendMessageAction(On.HutongGames.PlayMaker.Actions.SendMessage.orig_OnEnter orig, SendMessage self)
    {
        if (self.IsCorrectContext("Conversation Control", "Telescope Inspect", "Stop"))
            EyeActive = true;
        orig(self);
    }

    private void OnPlayerDataBoolTestAction(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, PlayerDataBoolTest self)
    {
        if (self.IsCorrectContext("Disable if No Lantern", "Toll Gate Machine", null))
            self.isFalse = (State == PowerState.Active && EyeActive) ? null : FsmEvent.GetFsmEvent("DISABLE");
        else if (self.IsCorrectContext("Deactivate in darkness without lantern", "Area Title Controller", null))
            self.isFalse = (State == PowerState.Active && EyeActive) ? null : FsmEvent.GetFsmEvent("NO LANTERN");
        else if (self.IsCorrectContext("FSM", "Ghost Warrior NPC", null))
            self.isFalse = (State == PowerState.Active && EyeActive) ? null : FsmEvent.GetFsmEvent("DEACTIVE");
        else if (self.IsCorrectContext("Vignette", "Darkness Control", null) && (string.Equals(self.State.Name, "Scene Reset")
            || string.Equals(self.State.Name, "Dark Lev Check")))
        {
            self.isTrue = State == PowerState.Twisted ? null : FsmEvent.GetFsmEvent("LANTERN");
            self.isFalse = (State == PowerState.Active && EyeActive) ? FsmEvent.GetFsmEvent("LANTERN") : null;
        }
        orig(self);
    }

    private void IntSwitch_OnEnter(On.HutongGames.PlayMaker.Actions.IntSwitch.orig_OnEnter orig, IntSwitch self)
    {
        if (self.IsCorrectContext("Darkness Control", "Vignette", null) && string.Equals(self.sendEvent[0].Name, "NORMAL"))
        {
            if (!_adjustedDarkness)
            {
                _adjustedDarkness = true;
                self.Fsm.Variables.FindFsmInt("Darkness Level").Value = Mathf.Min(self.Fsm.Variables.FindFsmInt("Darkness Level").Value + 1, 2);
            }
        }
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += OnActivateGameObjectAction;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += OnSendMessageAction;
        On.HeroController.Die += HeroController_Die;
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(Blink());
        Eye?.SetActive(EyeActive);
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter -= OnSendMessageAction;
        On.HeroController.Die -= HeroController_Die;
        Eye.SetActive(false);
    }

    /// <inheritdoc/>
    protected override void TwistEnable() => On.HutongGames.PlayMaker.Actions.IntSwitch.OnEnter += IntSwitch_OnEnter;

    /// <inheritdoc/>
    protected override void TwistDisable() => On.HutongGames.PlayMaker.Actions.IntSwitch.OnEnter -= IntSwitch_OnEnter;

    /// <inheritdoc/>
    protected override void Terminate()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter -= OnActivateGameObjectAction;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Change the size of the eye.
    /// </summary>
    private IEnumerator Blink()
    {
        float currentScale = Eye.transform.localScale.x;
        bool upscale = true;
        while (true)
        {
            yield return null;
            if (!EyeActive)
            {
                Eye.SetActive(false);
                yield return new WaitUntil(() => EyeActive);
                Eye.SetActive(true);
            }
            currentScale += .3f * Time.deltaTime * (upscale ? 1f : -1f);
            if (currentScale >= 1.2f)
                upscale = false;
            else if (currentScale <= .5f)
                upscale = true;
            Eye.transform.localScale = new(currentScale, currentScale);
        }
    }

    /// <summary>
    /// Wait a moment and then update the ui to display the health correctly.
    /// </summary>
    private IEnumerator UpdateUI()
    {
        yield return new WaitForSeconds(.2f);
        HeroController.instance.proxyFSM.SendEvent("HeroCtrl-Healed");
    }

    #endregion
}
