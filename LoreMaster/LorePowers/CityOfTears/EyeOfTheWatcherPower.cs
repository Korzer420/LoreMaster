using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using System;
using System.Collections;
using UnityEngine;
using SFCore.Utils;
using HutongGames.PlayMaker;

namespace LoreMaster.LorePowers.CityOfTears;

public class EyeOfTheWatcherPower : Power
{
    #region Members

    private Sprite _eyeSprite;

    private GameObject _eye;
    
    #endregion

    #region Constructors

    public EyeOfTheWatcherPower() : base("Eye Of The Watcher", Area.CityOfTears)
    {
        _eyeSprite = SpriteHelper.CreateSprite("EyeOfLurien");
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public override Action SceneAction => () =>
    {
        _eye.GetComponent<SpriteRenderer>().color = Color.white;
    };

    /// <summary>
    /// Gets the indicator if the player can be revived.
    /// </summary>
    public bool CanRevive => EyeActive && PlayerData.instance.GetBool(nameof(PlayerData.instance.hasLantern));

    /// <summary>
    /// Gets or sets the value that indicates if the eye is active.
    /// </summary>
    public bool EyeActive { get; set; } = true;

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

    private void OnActivateGameObjectAction(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_OnEnter orig, HutongGames.PlayMaker.Actions.ActivateGameObject self)
    {
        if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Vignette") && string.Equals(self.Fsm.FsmComponent.FsmName, "Darkness Control") && string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Scene Reset 2"))
        {
            self.Fsm.FsmComponent.AddTransition("Scene Reset 2", "LANTERN", "Lantern 2");
            if (Active && EyeActive)
            {
                self.Fsm.FsmComponent.SendEvent("LANTERN");
            }
        }

        orig(self);
    }

    private void OnSendMessageAction(On.HutongGames.PlayMaker.Actions.SendMessage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SendMessage self)
    {
        if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Telescope Inspect") && string.Equals(self.Fsm.FsmComponent.FsmName, "Conversation Control") && string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Stop"))
        {
            EyeActive = true;
        }

        orig(self);
    }

    private void OnPlayerDataBoolTestAction(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.PlayerDataBoolTest self)
    {
        if (self.Fsm.FsmComponent.gameObject.name.Contains("Toll Gate Machine") && string.Equals(self.Fsm.FsmComponent.FsmName, "Disable if No Lantern"))
        {
            self.isFalse = (Active && EyeActive) ? null : FsmEvent.GetFsmEvent("DISABLE");
        }

        else if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Area Title Controller") && string.Equals(self.Fsm.FsmComponent.FsmName, "Deactivate in darkness without lantern"))
        {
            self.isFalse = (Active && EyeActive) ? null : FsmEvent.GetFsmEvent("NO LANTERN");
        }

        else if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Ghost Warrior NPC") && string.Equals(self.Fsm.FsmComponent.FsmName, "FSM"))
        {
            self.isFalse = (Active && EyeActive) ? null : FsmEvent.GetFsmEvent("DEACTIVE");
        }

        else if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Vignette") && string.Equals(self.Fsm.FsmComponent.FsmName, "Darkness Control") && (string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Scene Reset") || string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Dark Lev Check")))
        {
            self.isFalse = (Active && EyeActive) ? FsmEvent.GetFsmEvent("LANTERN") : null;
        }

        orig(self);
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        _eye = new("Eye of Lurien");
        _eye.transform.SetParent(HeroController.instance.transform);
        _eye.AddComponent<SpriteRenderer>().sprite = _eyeSprite;
        _eye.transform.localPosition = new(0, 2f);
        _eye.transform.localScale = new(.5f, .5f);
        GameObject.DontDestroyOnLoad(_eye);
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += OnSendMessageAction;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += OnActivateGameObjectAction;
        On.HeroController.Die += HeroController_Die;
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(Blink());
        if (_eye != null)
            _eye?.SetActive(EyeActive);
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter -= OnSendMessageAction;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter -= OnActivateGameObjectAction;
        On.HeroController.Die -= HeroController_Die;
        _eye?.SetActive(false);
    }

    #endregion

    #region Private Methods

    private IEnumerator Blink()
    {
        float currentScale = _eye.transform.localScale.x;
        bool upscale = true;
        while (true)
        {
            yield return null;
            if (!EyeActive)
            {
                _eye.SetActive(false);
                yield return new WaitUntil(() => EyeActive);
                _eye.SetActive(true);
            }
            currentScale += .3f * Time.deltaTime * (upscale ? 1f : -1f);
            if (currentScale >= 1.2f)
                upscale = false;
            else if (currentScale <= .5f)
                upscale = true;
            _eye.transform.localScale = new(currentScale, currentScale);
        }
    }

    private IEnumerator UpdateUI()
    {
        yield return new WaitForSeconds(.2f);
        HeroController.instance.proxyFSM.SendEvent("HeroCtrl-Healed");
    }

    #endregion
}
