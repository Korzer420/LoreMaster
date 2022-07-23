using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
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

    /// <summary>
    /// Gets or sets the indicator if the player can be revived.
    /// </summary>
    public bool CanRevive => _eye.activeSelf && PlayerData.instance.GetBool("hasLantern");

    #endregion

    #region Event Handler

    private void HeroController_SetDarkness(On.HeroController.orig_SetDarkness orig, HeroController self, int darkness)
    {
        orig(self, darkness);
        _darknessLevel = darkness;
    }

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
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HeroController.SetDarkness += HeroController_SetDarkness;
        On.PlayMakerFSM.OnEnable += RefreshEyeOfTheWatcher;
        On.HeroController.Die += HeroController_Die;
        LoreMaster.Instance.SceneActions.Add(PowerName, () =>
        {
            if (_darknessLevel > 0)
            {
                HeroController.instance.wieldingLantern = true;
                if (_eye.activeSelf)
                    _eye.GetComponent<SpriteRenderer>().color = new(1f, 1f, 0f);
            }
            else if (_eye.activeSelf)
                _eye.GetComponent<SpriteRenderer>().color = Color.white;
        });
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(Blink());
        _eye?.SetActive(true);
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HeroController.SetDarkness -= HeroController_SetDarkness;
        On.PlayMakerFSM.OnEnable -= RefreshEyeOfTheWatcher;
        On.HeroController.Die -= HeroController_Die;
        LoreMaster.Instance.SceneActions.Remove(PowerName);
        LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
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
