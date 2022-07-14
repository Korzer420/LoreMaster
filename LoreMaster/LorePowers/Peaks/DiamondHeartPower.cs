using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using Modding;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace LoreMaster.LorePowers.Peaks;

public class DiamondHeartPower : Power
{
    #region Members

    private HealthManager[] _enemies;
    private SpriteRenderer _crystalHeartSprite;
    private Sprite _originalSprite;
    private Sprite _corelessSprite;
    private Sprite _shelllessSprite;
    private Sprite _diamondSprite;

    #endregion

    #region Constructors

    public DiamondHeartPower() : base("Diamond Heart", Area.Peaks)
    {
        Hint = "The crystal heart's core absorbed the power of diamond and got even stronger. If you hit a wall, all foes may be stunned shortly. The power of the diamond may crush enemies in your path even more.";
        Description = "Crystal Heart snares all enemies in the room for 1.5 seconds if you hit a wall and doubles the damage. Doubled if you have Diamant Dash Power.";
        CustomText = "Isn't the view just beautiful? When I just look at this, all my thought feel way less heavier than before. It feels almost... empty. Have you already looked around here a bit? " +
            "These crystals here contain an mysterious power. Although it seemed to me, that they people actually looked for something even more powerful. Here, I found that crystal from the remains of another adventurer." +
            " It emits a power far beyond everything else that you probably can mine here. It only needs a fitting vessel, but for an adventurer like you, this shouldn't be a problem.";
    }

    #endregion

    #region Properties

    public bool HasDiamondDash => LoreMaster.Instance.ActivePowers.ContainsKey("MYLA") && LoreMaster.Instance.ActivePowers["MYLA"].Active;

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        _crystalHeartSprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Equipment/Super Dash").GetComponent<SpriteRenderer>();
        _originalSprite = _crystalHeartSprite.sprite;
        _corelessSprite = SpriteHelper.CreateSprite("DiamondHeart_Coreless");
        _shelllessSprite = SpriteHelper.CreateSprite("DiamondHeart_Shellless");
        _diamondSprite = SpriteHelper.CreateSprite("DiamondHeart");
        HeroController.instance.superDash.GetState("Hit Wall").ReplaceAction(new Lambda(() => 
        {
            if (Active)
                foreach (HealthManager enemy in _enemies)
                    LoreMaster.Instance.Handler.StartCoroutine(StunEnemy(enemy.gameObject));
                
            HeroController.instance.superDash.FsmVariables.FindFsmGameObject("SuperDash Damage").Value.SetActive(false);
        }) { Name = "Wall Crash" }, 8);
    }

    protected override void Enable()
    {
        _enemies = GameObject.FindObjectsOfType<HealthManager>();
        LoreMaster.Instance.SceneActions.Add(PowerName, () =>
        {
            _enemies = GameObject.FindObjectsOfType<HealthManager>();
        });
        if (HasDiamondDash)
        {
            _crystalHeartSprite.sprite = _diamondSprite;
            HeroController.instance.superDash.FsmVariables.FindFsmGameObject("SuperDash Damage").Value.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = 30;
        }
        else
        {
            _crystalHeartSprite.sprite = _shelllessSprite;
            HeroController.instance.superDash.FsmVariables.FindFsmGameObject("SuperDash Damage").Value.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = 20;
        }
    }

    protected override void Disable()
    {
        LoreMaster.Instance.SceneActions.Remove(PowerName);
        if (HasDiamondDash)
            _crystalHeartSprite.sprite = _corelessSprite;
        else
            _crystalHeartSprite.sprite = _originalSprite;
        HeroController.instance.superDash.FsmVariables.FindFsmGameObject("SuperDash Damage").Value.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = 10;
        _enemies = null;
    }

    #endregion

    #region Private Methods

    private IEnumerator StunEnemy(GameObject enemy)
    {
        float passedTime = 0f;
        float stunTime = HasDiamondDash ? 3f : 1.5f;
        Vector3 positionToLock = enemy.transform.localPosition;
        while(passedTime <=  stunTime)
        {
            yield return null;
            passedTime += Time.deltaTime;
            enemy.transform.localPosition = positionToLock;
        }
    }

    #endregion
}
