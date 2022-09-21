using LoreMaster.Enums;
using System;
using UnityEngine;

namespace LoreMaster.LorePowers.FogCanyon;

public class FriendOfTheJellyfishPower : Power
{
    #region Constructors

    public FriendOfTheJellyfishPower() : base("Friend of the Jellyfishes", Area.FogCanyon)
    {
        CustomText = "He's my twelfth catch of the day. I'm gonna call him \"Twelvey.\"  Coochie coochie coo! Bye, Twelvey! Oh! It's him! Well, it's just him and me again, I've caught and named every jellyfish in Fog Canyon at least once. Except you, No Name.";
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public override Action SceneAction => () => MakeJellyfishHarmless();

    #endregion

    #region Event Handler

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (string.Equals(self.FsmName,"Explosion Control"))
        {
            if (Active)
            {
                // This is for godhome oomas
                if (self.gameObject.name.Contains("Gas Explosion Uumuu"))
                    HeroController.Destroy(self.transform.Find("Hero Damage").GetComponent<DamageHero>());
                else
                    HeroController.Destroy(self.GetComponent<DamageHero>());
            }
            else if (self.GetComponent<DamageHero>() == null)
            {
                DamageHero damageHero = self.gameObject.AddComponent<DamageHero>();
                damageHero.damageDealt = 2;
                damageHero.hazardType = 1;
            }
        }
        else if (string.Equals(self.FsmName,"Lil Jelly"))
            self.GetComponent<DamageHero>().damageDealt = 0;
        else if (string.Equals(self.FsmName,"Jellyfish") && self.gameObject.name.Contains("Jellyfish GG"))
        {
            HeroController.Destroy(self.GetComponent<DamageHero>());
            HeroController.Destroy(self.transform.Find("Tentacle Box").GetComponent<DamageHero>());
        }

        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        // For the room where you obtained this power.
        MakeJellyfishHarmless();
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Modifies all jelly fish, so that they are harmless. (Except Uumuu of course)
    /// </summary>
    private void MakeJellyfishHarmless()
    {
        HealthManager[] enemies = GameObject.FindObjectsOfType<HealthManager>();
        if (enemies.Length == 0)
            return;
        foreach (HealthManager item in enemies)
            // Uumuu is called Mega Jellyfish in the files, that's why we check for jelly fish without mega in their name
            if (item.gameObject.name.Contains("Jellyfish") && !item.gameObject.name.Contains("Mega"))
            {
                HeroController.Destroy(item.gameObject.GetComponent<DamageHero>());
                // The tentacles of the jelly have their own component.
                if (!item.gameObject.name.Contains("Baby"))
                    HeroController.Destroy(item.transform.Find("Tentacle Box").GetComponent<DamageHero>());
            }
    }

    #endregion
}
