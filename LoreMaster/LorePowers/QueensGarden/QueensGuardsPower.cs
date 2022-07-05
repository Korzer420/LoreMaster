using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.QueensGarden;

public class QueensGuardsPower : Power
{
    #region Members

    private GameObject _thorns;

    private Sprite[] _sprites = new Sprite[2];  

    #endregion

    #region Constructors

    public QueensGuardsPower() : base("", Area.QueensGarden)
    {
        // Save the old thorns image and create a new one which can be used anytime.
        if (_sprites[1] == null)
        {
            string imageFile = Path.Combine(Path.GetDirectoryName(typeof(MindBlast).Assembly.Location), "Resources/Queens_Thorns.png");
            byte[] imageData = File.ReadAllBytes(imageFile);
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(tex, imageData, true);
            tex.filterMode = FilterMode.Bilinear;
            _sprites[0] = CharmIconList.Instance.spriteList[12];
            _sprites[1] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        }

        PlayMakerFSM fsm = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Thorn Counter");
        FsmState currentWorkingState = fsm.GetState("Counter Start");
        currentWorkingState.AddFirstAction(new Lambda(() =>
        {
            // Prevent the freeze in the air
            currentWorkingState.GetFirstActionOfType<SetPosition>().Enabled = !IsCurrentlyActive();
            // Prevent the gravity from getting removed (this would cause the hero to float until the thorns despawn... probably).
            currentWorkingState.GetActionsOfType<SendMessage>().Take(2).ToList().ForEach(x => x.Enabled = !IsCurrentlyActive());

        }));

        currentWorkingState = fsm.GetState("Counter");
        currentWorkingState.AddFirstAction(new Lambda(() =>
        {
            // Prevent the freeze in the air
            currentWorkingState.GetFirstActionOfType<SetPosition>().Enabled = !IsCurrentlyActive();
            // I'm unsure if this is needed, but it can't hurt, right?
            HeroController.instance.RegainControl();
        }));

        _thorns = fsm.transform.Find("Thorn Hit").gameObject;

        // Adjust thorn damage
        foreach (Transform child in _thorns.transform)
        {
            fsm = child.gameObject.LocateMyFSM("set_thorn_damage");
            fsm.GetState("Set").InsertAction(new Lambda(() => 
            {
                if (IsCurrentlyActive())
                    fsm.FsmVariables.GetFsmInt("Damage").Value *= 2;
            }), 1);
        }
    }

    #endregion

    #region Properties

    public bool CanHeal => PlayerData.instance.GetBool("equippedCharm_27") 
        || (PlayerData.instance.GetInt("Health") < PlayerData.instance.GetInt("maxHealth"));

    #endregion

    #region Public Methods

    public override void Enable()
    {
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        On.HealthManager.Die += HealthManager_Die;
        CharmIconList.Instance.spriteList[12] = _sprites[1];
    }

    public override void Disable()
    {
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
        On.HealthManager.Die -= HealthManager_Die;
        CharmIconList.Instance.spriteList[12] = _sprites[0];
    }

    #endregion

    #region Private Methods

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        orig(self, hitInstance);
        if (_thorns.activeSelf)
            HeroController.instance.AddMPCharge(7);
    }

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        // We assume that if an enemy dies while thorns is active, they killed them.
        if (_thorns.activeSelf && LoreMaster.Instance.Generator.Next(1,3) == 1 && CanHeal)
        {
            if (PlayerData.instance.GetBool("equippedCharm_27"))
                EventRegister.SendEvent("ADD BLUE HEALTH");
            else
                HeroController.instance.AddHealth(1);
        }
    }

    #endregion
}
