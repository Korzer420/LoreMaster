using LoreMaster.Enums;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class GloryOfTheWealthPower : Power
{
    #region Members

    private Dictionary<string, int[]> _enemyGeoValues = new();

    #endregion

    #region Constructors

    public GloryOfTheWealthPower() : base("Glory of the Wealth", Area.FungalWastes)
    {
        Hint = "Your enemies may \"share\" more of their wealth with you.";
        Description = "Enemies drop double geo.";
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public override Action SceneAction => () =>
    {
        HealthManager[] enemies = GameObject.FindObjectsOfType<HealthManager>();

        foreach (HealthManager enemy in enemies)
        {
            // Get the enemy name. We need to use a regex to prevent flouding the dictionary with reduntant data. For example: If enemy is called Crawler 1, the entry for "Crawler" doesn't work.
            string enemyName = Regex.Match(enemy.name, @"^[^0-9]*").Value.Trim();

            // Check if we already have registered the enemy type. This action takes a lot of loading time, therefore we want to avoid it, as much as we can.
            if (!_enemyGeoValues.ContainsKey(enemyName))
            {
                int[] geoValues = new int[3];

                geoValues[0] = ReflectionHelper.GetField<HealthManager, int>(enemy, "smallGeoDrops");
                geoValues[1] = ReflectionHelper.GetField<HealthManager, int>(enemy, "mediumGeoDrops");
                geoValues[2] = ReflectionHelper.GetField<HealthManager, int>(enemy, "largeGeoDrops");

                _enemyGeoValues.Add(enemyName, geoValues);
            }

            int[] geoDrops = _enemyGeoValues[enemyName];
            // We only increase if it would drop geo anyway
            if (geoDrops.Any(x => x != 0))
            {
                enemy.SetGeoSmall(geoDrops[0] * 2);
                enemy.SetGeoMedium(geoDrops[1] * 2);
                enemy.SetGeoLarge(geoDrops[2] * 2);
            }
        }
    };

    #endregion
}
