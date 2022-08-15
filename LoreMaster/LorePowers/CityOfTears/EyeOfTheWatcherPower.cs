using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using MonoMod.Cil;
using System;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

public class EyeOfTheWatcherPower : Power
{
    #region Members

    private Sprite _eyeSprite;

    private GameObject _eye;

    private int _darknessLevel = 0;

    #endregion

    #region Constructors

    public EyeOfTheWatcherPower() : base("Eye Of The Watcher", Area.CityOfTears)
    {
        Hint = "The eye of the watcher will protect you and share it's sight, allowing you to see in the dark. If the eye can fully see you, it gaze may once prevent a fatal blow on you. To call the eye again, look through the tool of the watcher.";
        Description = "Grants the lantern effect. If you already have the lantern effect and would take lethal damage, you will be healed to full hp instead (with joni's you gaining 5 lifeblood instead)." +
            " Once triggered, has to be restored by looking through the telescope by lurien.";
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
    public bool CanRevive => _eye.activeSelf && PlayerData.instance.GetBool(nameof(PlayerData.instance.hasLantern));

    #endregion

    #region Event Handler

    private IEnumerator HeroController_Die(On.HeroController.orig_Die orig, HeroController self)
    {
        if (!CanRevive)
            yield return orig(self);
        else
        {
            _eye.SetActive(false);
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

    private void RefreshEyeOfTheWatcher(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.gameObject.name.Equals("Telescope Inspect") && self.FsmName.Equals("Conversation Control"))
            self.GetState("Stop").ReplaceAction(new Lambda(() => _eye.SetActive(true)) { Name = "Restore Eye" });
        else if (self.gameObject.name.Contains("Toll Gate Machine") && self.FsmName.Equals("Disable if No Lantern"))
            self.GetState("Check").ReplaceAction(new Lambda(() => 
            {
                if (Active && _eye != null && _eye.activeSelf)
                    self.SendEvent("FINISHED");
                else if(!PlayerData.instance.GetBool(nameof(PlayerData.instance.hasLantern)))
                    self.SendEvent("DISABLE");
            })
            { Name = "Enable Toll"},0);
        orig(self);
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        // Modify all darkness checks
        PlayMakerFSM fsm = HeroController.instance.transform.Find("Vignette").gameObject.LocateMyFSM("Darkness Control");
        fsm.GetState("Scene Reset").ReplaceAction(new Lambda(() =>
        {
            if ((Active && _eye != null && _eye.activeSelf) || PlayerData.instance.GetBool(nameof(PlayerData.instance.hasLantern)))
            {
                _eye.GetComponent<SpriteRenderer>().color = new(1f, 1f, 0f);
                fsm.SendEvent("LANTERN");
            }
        })
        { Name = "Force Lantern" }, 1);

        fsm.GetState("Scene Reset 2").AddTransition("LANTERN", "Lantern 2");
        fsm.GetState("Scene Reset 2").ReplaceAction(new Lambda(() =>
        {
            if (Active && _eye != null && _eye.activeSelf)
            {
                _eye.GetComponent<SpriteRenderer>().color = new(1f, 1f, 0f);
                fsm.SendEvent("LANTERN");
            }
            else if (fsm.FsmVariables.FindFsmInt("Darkness Level").Value == 0)
                fsm.SendEvent("NORMAL");
            else if (fsm.FsmVariables.FindFsmInt("Darkness Level").Value == 0)
                fsm.SendEvent("DARK -1");
        })
        { Name = "Force Lantern" }, 1);

        fsm.GetState("Dark Lev Check").ReplaceAction(new Lambda(() =>
        {
            if ((Active && _eye != null && _eye.activeSelf) || PlayerData.instance.GetBool(nameof(PlayerData.instance.hasLantern)))
            {
                _eye.GetComponent<SpriteRenderer>().color = new(1f, 1f, 0f);
                fsm.SendEvent("LANTERN");
            }
        })
        { Name = "Force Lantern" }, 3);
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
        On.PlayMakerFSM.OnEnable += RefreshEyeOfTheWatcher;
        On.HeroController.Die += HeroController_Die;
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(Blink());
        if (_eye != null && !_eye.activeSelf)
            _eye?.SetActive(true);
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.PlayMakerFSM.OnEnable -= RefreshEyeOfTheWatcher;
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
            if (!_eye.activeSelf)
                yield return new WaitUntil(() => _eye.activeSelf);
            currentScale += .3f * Time.deltaTime * (upscale ? 1f : -1f);
            if (currentScale >= 1.3f)
                upscale = false;
            else if (currentScale <= .8f)
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
