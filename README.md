# LoreMaster
Hollow Knight Mod that adds abilities to the lore tablets. (Requires Item Changer)

# How it works
Reading a lore tablet displays the granted power name and a mostly vaguely description of what it does (or detailed description). The ability stays only active while you are in the zone, in which you found the lore tablet. If you have found all lore tablets in the zone, the powers are granted permanently. (Default)

# What does count?
I mostly oriented on the wiki page of lore tablets to see what actually counts. But there are a few addition, changes and removals to look out for:
- Menderbugs diary does nothing (I mean, you are granted "Regret" if you want to count it)
- Any of Bretta's diary entries does count for her power.
- Midwife and the mask maker grant abilities.
- The traitor's grave and the queen grant abilites.
- Hive Queen Vesper grants an ability. WARNING: Dreamnailing her before talking, denies the power forever.
- Joni grants an ability (same conditions as Hive Queen Vesper).
- The journal entry at the end of PoP grant an ability (Ability is only shown at the lore tablet at the start of PoP. This is the only power that by default is excluded (see below for more details))
- The world sense tablet in the black egg temple does count.

Currently not implemented:
- The shield on iselda's shop grants you a bonus but not ability. (Is not required for the dirtmouth loremaster)
- Lemm's sign on his shop (after getting a dreamer) grants a bonus but no ability (Is not required for city of tears loremaster)
- All Dream Warrior statues grant a bonus (not required).

The only zone with no power to obtain is crystal peaks.

# Settings
You can change these options in the mod menu ingame:
- Disable custom text (some text lines have been replaced with own ones, to make it more fitting for the ability)
- Use hints: If true, the powers will display a vaguely description instead of a detailed one. Default is true.

# Manual Setup
You also can set up a predefined pack of abilites or changing their existing properties. For this, create a option_{FileSlot}.txt file (option_1.txt for example) in the loremaster folder of the game. A default option file in added to the package.
Write %Override% or %Modify% in the first line:
Override: Will reset your progress and let's you start with only your configuration.
Modify: Loads the save data from the file and then modify the data with your configuration.

Write the name of a power like %MyPower% and add an tag behind it. E. g:
%WellFocused% global
%WorldSense% disable

You can use the name of the player displayed ingame or the name of the class from the code (Excluding the "Power" text. e.g: WeDontTalkAboutShadePower -> WeDontTalkAboutShade in the file.)

You have five tags available:
- global: This power is always available once obtained.
- local: Default behaviour, granted in the zone or globally if you have all required power in the zone.
- exclude: This power is granted like normal, but is not required to get the lore master of the zone. Same conditions as local for staying active.
- disable: This power doesn't work, but it's collection still behave normally.
- remove: This power doesn't work and is not required to get the lore master of the zone.

If you want to give yourself the power instantly, you can add "|add" to the end of the line. Note, that this will be still affected by the tag
A option file can look like this:
```cs
%Override%
%Imposter% global
%UnitedWeStand% global | add
%ScrewTheRules% disable
%True Form% exclude
```
This example would do the following:
- The Imposter Power will stay completely active once obtained.
- The United we Stand Power is acquired and permanently active.
- The Screw the Rules Power doesn't work at all (Only for the tracker).
- The True Form ability can be acquired as normal but it will not be a requirement to fully enable the Dirtmouth/King's Pass powers globally.

You can ignore a line if you insert a "#" somewhere in the line.
# Powers

Here are all powers listed with their description (This contains the detailed descriptions, but this mod also adds vaguely hints. If you don't want to spoil yourself don't read further):

Ancient Basin:

- We don't talk about Shade: You don't get the soul limit punishment, when dying. Your geo will still be on the shade.

City of Tears:

- Eye of the Watcher: Currently does nothing. Hit me up with any ideas. :)
- Hot Streak: When hitting an enemy with the nail, increases it's damage by 1 (max. 3 stacks per nail upgrade (15 total)). Resets if you don't hit an enemy.
- Marissas Audience: After 20 to 60 seconds spawn multiple companions (Weavers, Hatchlings, Grimmchilds) that persist in the current room or for 30 to 90 seconds. If    Marissa is dead, spawns Revek each 45 to 180 seconds, that persist in the current room or 20 to 60 seconds.
- Overwhelming Power: When you cast a spell while your soul vessel is full (not counting additional soul vessels), they deal twice as much damage and are twice as big.
- Soul Extract Efficiency: You gain 5 more soul per hit on enemies.
- Tourist: You can talk to the firefly by the statue of THK to teleport to black egg temple for 50 geo, or back to this room from the temple.

Crossroads:

- Reluctant Pilger: While standing of the ground, the grubberfly elegy effect is active (regardless of your HP). If you have grubberfly equipped, the damage on ground is doubled instead.
- Greater Mind: Activates the tracker, to show you how many powers you are missing. If the counter is green, you have global access to the power in the area.

Deepnest:

- Infested!: Killing an enemy grants 1 to 5 weaverlings that assist you in the current room. (Capped at 50)
- Mask Overcharge: Overcharge one of your mask (it glows in different colors), while you have exactly that much health, a circle gathers around you that deal damage and restore 5 soul each second. The overcharged mask changes every 30 seconds and may never select the full hp mask. Inactive while you have Joni's Blessing equipped.

Dirtmouth:

- Caring Shell: Enviroment Hazards (like spikes) don't deal damage to you anymore.
- Screw the Rules: Fury of the Fallen is now also active with 2 hp, but the damage buff is decreased to 50%. The debuff only affects normal nail hits.
- True Form: While your shade is active, you deal 30% more damage and increase your nail length by 25%. The effects are doubled, if you are in the same room as your shade.
- Well Focused: Focus is cast 30% faster.

Fog Canyon:

- Friend of the Jellyfishes: You're immune to jelly fishs enemies and explosions. Note: Non Jelly fish explosion enemies, still deal 2 damage on contact. You're just immune to the explosion itself.
- Belly of the Jelly(fish): Decrease your falling speed by 25% and triples the time needed in air, for a hard fall.
- Jellyfish Flow: You swim 3 times as fast.

Fungal Wastes:

- Eternal Valor: Each 30 hits on enemies, heal you for 1 mask.
- Glory of the Wealth: Enemies drop double geo.
- Imposter: While wearing spore shrooms, focus has a 20% chance to add a lifeblood (doesn't work if you have 3 or more lifeblood).
- Mantis Style: Increase your nail range by 50% (from base).
- One of Us: Every twelve seconds you cast the deep focus spore cloud. Hold the crystal dash button to prevent that (in case you want to do pogos for example).
- Pale Luck: When you would take damage, you have a 1% chance to be healed instead. Increased by 2% for each King's Brand and Kingssoul.
- United we Stand: Weavers are bigger, Grimmchild shoots faster (capped at 12) and Hatchling deal more damage for each companion (of those three) you have.

Greenpath:

- Camouflage: After standing still for 5 seconds, you gain invincibility until you do something. If you are wearing Shape of Unn, you keep the invincibility while focusing and moving as a slug.
- Gift on Unn: Gain the shape of Unn effect for focusing. If you're wearing Shape of Unn, focus restores 15 soul on a successful cast.
- Mindblast of Unn: Hitting an enemy with the dream nail permanently increases the taken damage by 2. Bonus for wearing charm: (+1 Dreamwielder; +2 Dreamshield; +3 Shape of Unn)
- Rooted: You can hang on walls, without sliding down. Pressing down let's you move down. Also you can cast focus on walls. If you have Shape on Unn equipped, you can also move down on walls during focus.
- Return to Unn: Your Movement speed is increased by 3 and your dash cooldown is reduces by 0.5 seconds, while you facing left.
- Touch Grass: Every 10 seconds standing on grass, you heal 1 mask. Decreased to 5 seconds if wearing Shape of Unn.

Howling Cliffs:

- Joni's Protection: When going to another area, you will be granted 5 life blood (10 if you have Joni's equipped). Each 3 seconds a lifeblood will fade away.
- Lifeblood Wings: For each lifeblood that you have, you can jump an additional time. Requires Wings.

Kingdom's Edge:

- Concussive Strike: Great Slash and Dash Slash cause Concussion on their target for 3 seconds. Concussed enemies take 10% more damage from nail attacks and increase their knockback by 50% (66% of Heavy Blow). Nail hits on the target extend the duration by 0.5 seconds each. Cyclone Slash is not counted as a nail slash in this case. Also cause enemies to glance 10% of their hits, decreasing their damage by 1.
- Wisdom of the Sage: For each Mr. Mushroom stage that you completed, spells cost 1 soul less.
- You like Jazz?: You can now have 10 hatchlings at a time, they spawn twice at fast and cost only 25% of their normal soul.

Queen's Garden:

- Ring of Flowers: Increase the damage of your nail arts by 10% for each recipient for the flower.
- Queen's Thorns: Thorns of Agony are now "Queen's Thorns", which removes the freeze on hit. restore soul if it hits an enemy and has a 33% chance to restore 1 hp if it kills an enemy.

Resting Grounds:

- Defeated Dreamers grant the dream nail an additional effect. Lurien: Roots the target for 3 seconds (15 seconds cooldown) Herrah: Spawn 2 weavers for 15 seconds. Monomon: Per 100 Essence you have a 1% chance to instant kill the enemy (capped at 200 damage). Capped at 2400 Essence for 24%. (This is granted, if you enter the room)

Waterways:

- Eternal Sentinel: Defender's Crest clouds are 150% bigger and tick twice as fast. Baldur shell now takes 10 hits instead of four. When getting hit, while baldur shell is up, you gain 15 soul if you are also wearing Defender's Crest.
- Relentless Swarm: Hits with flukes restore 2 soul, or 5 if the enemy died.

White Palace:

- Diminishing Curse: If you take 15 hits, you will no longer count as overcharmed, resets if you sit on a bench. The UI only updates if you open the charm screen.
- Sacred Shell: You can longer take more than one damage per hit (excluding overcharmed). Default is excluded.
- Shadow Forged: Decrease the cooldown of shade cloak by 0.4 seconds and increases sharp shadow damage by 100%. Warning: The cooldown is not on sync with the animation! Shortly after the orbs spawn, the dash is ready again.
- Shining Bound: For each charm you're wearing, you gain 1 soul per 2 seconds.
