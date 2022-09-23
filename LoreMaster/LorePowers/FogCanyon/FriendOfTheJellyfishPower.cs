using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using System;
using System.Collections;
using System.Linq;
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
        if (State == PowerState.Active)
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
        else
        {
            // We create an area in which the jellyfish can spawn based on the transitions
            Transform[] points = GameObject.FindObjectsOfType<TransitionPoint>().Select(x => x.transform).ToArray();
            // Less than two makes it impossible to create a field.
            if (points.Length < 2)
                return;
            float[] borderPoints = new float[4];
            borderPoints[0] = points.Min(x => x.position.x);
            borderPoints[1] = points.Max(x => x.position.x); 
            borderPoints[2] = points.Min(x => x.position.y);
            borderPoints[3] = points.Max(x => x.position.y);
            for (int i = 0; i < LoreMaster.Instance.Generator.Next(3, 11); i++)
            {
                GameObject ooma = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Jellyfish"]);
                // To prevent many of them getting stuck in terrain they ignore it.
                ooma.AddComponent<IgnoreTerrain>();
                ooma.SetActive(true);
                Vector3 position;
                do
                    position = new Vector3(LoreMaster.Instance.Generator.Next((int)borderPoints[0], (int)borderPoints[1]), LoreMaster.Instance.Generator.Next((int)borderPoints[2], (int)borderPoints[3]));
                while (Vector3.Distance(position, HeroController.instance.transform.position) < 5);
                ooma.transform.position = position;
            }

            for (int i = 0; i < LoreMaster.Instance.Generator.Next(10, 31); i++)
            {
                GameObject babyJelly = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Jellyfish Baby"]);
                // To prevent many of them getting stuck in terrain they ignore it.
                babyJelly.AddComponent<IgnoreTerrain>();
                babyJelly.SetActive(true);
                Vector3 position;
                do
                    position = new Vector3(LoreMaster.Instance.Generator.Next((int)borderPoints[0], (int)borderPoints[1]), LoreMaster.Instance.Generator.Next((int)borderPoints[2], (int)borderPoints[3]));
                while (Vector3.Distance(position, HeroController.instance.transform.position) < 5);
                babyJelly.transform.position = position;
            }
        }
    }

    #endregion
}
