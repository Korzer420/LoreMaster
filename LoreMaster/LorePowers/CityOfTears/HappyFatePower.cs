using LoreMaster.Enums;
using Modding;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

public class HappyFatePower : Power
{
    #region Members

    private bool _isHappy = true; // :)
    private Transform[] _nailObjects = new Transform[5]; 

    #endregion
    
    #region Constructors

    public HappyFatePower() : base("Charming Trap", Area.CityOfTears)
    {
        Hint = "[Not Implemented] Your \"happiness\" increases all your abilities slightly. Getting hit makes you sad :c. A good rest may restore your hapiness.";
        Description = "[Not Implemented] After sitting on a bench, your nail damage, nail range, running speed, dash speed and cdash charge up speed is increased. You also gain 1 soul per second.";
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        GameObject attackDirections = GameObject.Find("Knight/Attacks");
        _nailObjects[0] =attackDirections.transform.Find("Slash");
        _nailObjects[1] =attackDirections.transform.Find("AltSlash");
        _nailObjects[2] =attackDirections.transform.Find("UpSlash");
        _nailObjects[3] =attackDirections.transform.Find("DownSlash");
        _nailObjects[4] =attackDirections.transform.Find("WallSlash");
    }

    protected override void Enable()
    {
        On.HeroController.TakeHealth += HeroController_TakeHealth;
        On.HeroController.SetBenchRespawn += HeroController_SetBenchRespawn;
        ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
        _isHappy = true;
        HappynessChange();
    }

    private int ModHooks_GetPlayerIntHook(string name, int orig)
    {
        if (name.Equals("nailDamage") && _isHappy)
            orig += 3;
        return orig;
    }

    protected override void Disable()
    {
        On.HeroController.TakeHealth -= HeroController_TakeHealth;
        On.HeroController.SetBenchRespawn -= HeroController_SetBenchRespawn;
        ModHooks.GetPlayerIntHook -= ModHooks_GetPlayerIntHook;
        _isHappy = false;
        HappynessChange();
    }

    private void HeroController_SetBenchRespawn(On.HeroController.orig_SetBenchRespawn orig, HeroController self, string spawnMarker, string sceneName, int spawnType, bool facingRight)
    {
        orig(self, spawnMarker, sceneName, spawnType, facingRight);
        if (!_isHappy) 
        {
            _isHappy = true;
            HappynessChange();
        }
    }

    private void HeroController_TakeHealth(On.HeroController.orig_TakeHealth orig, HeroController self, int amount)
    {
        orig(self, amount);
        if (amount > 0 && _isHappy)
        {
            _isHappy = false;
            HappynessChange();
        }
    }

    #endregion

    #region Private Methods

    private void HappynessChange()
    {
        if(_isHappy)
        {
            foreach (Transform child in _nailObjects)
            {
                Vector3 currentScale = child.GetComponent<NailSlash>().scale;
                child.GetComponent<NailSlash>().scale = new Vector3(currentScale.x + .2f, currentScale.y + .2f, currentScale.z + .2f);
            }

            HeroController.instance.WALK_SPEED += 1.5f;
            HeroController.instance.RUN_SPEED += 1.5f;
            HeroController.instance.RUN_SPEED_CH += 1.5f;
            HeroController.instance.RUN_SPEED_CH_COMBO += 1.5f;
            HeroController.instance.DASH_COOLDOWN -= .2f;
            HeroController.instance.DASH_COOLDOWN_CH -= .2f;
            HeroController.instance.superDash.FsmVariables.FindFsmFloat("Charge Time").Value -= .1f;
            LoreMaster.Instance.Handler.StartCoroutine(GainHappySoul());
        }
        else
        {
            foreach (Transform child in _nailObjects)
            {
                Vector3 currentScale = child.GetComponent<NailSlash>().scale;
                child.GetComponent<NailSlash>().scale = new Vector3(currentScale.x - .2f, currentScale.y - .2f, currentScale.z - .2f);
            }
            HeroController.instance.WALK_SPEED -= 1.5f;
            HeroController.instance.RUN_SPEED -= 1.5f;
            HeroController.instance.RUN_SPEED_CH -= 1.5f;
            HeroController.instance.RUN_SPEED_CH_COMBO -= 1.5f;
            HeroController.instance.DASH_COOLDOWN += .2f;
            HeroController.instance.DASH_COOLDOWN_CH += .2f;
            HeroController.instance.superDash.FsmVariables.FindFsmFloat("Charge Time").Value += .1f;
        }
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    private IEnumerator GainHappySoul()
    {
        while(_isHappy)
        {
            yield return new WaitForSeconds(1f);
            HeroController.instance.AddMPCharge(1);
        }
    }

    #endregion
}
