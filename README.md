# LoreMaster
Hollow Knight Mod that adds abilities to the lore tablets. (Requires Item Changer and SFCore)

## How it works
Reading a lore tablet displays the granted power name and a mostly vaguely description of what it does (or detailed description). The ability stays only active while you are in the zone, in which you found the lore tablet. If you have found all lore tablets in the zone, the powers are granted permanently. (Default)

## What does count?
I mostly oriented on the wiki page of lore tablets to see what actually counts. But there are a few addition, changes and removals to look out for:
- Menderbugs diary does nothing (I mean, you are granted "Regret" if you want to count it)
- Any of Bretta's diary entries does count for her power.
- Midwife and the mask maker grant abilities.
- The traitor's grave, Moss prophet and the queen grant abilites.
- Badoon and Hive Queen Vesper grant an ability.
- Joni grants an ability.
- Poggy (The ghost in the elevator room of Pleasure Houses) grants an ability.
- The Gravedigger grants an ability.
- The journal entry at the end of PoP grant an ability (Ability is only shown at the lore tablet at the start of PoP. This is the only power that by default is excluded (see below for more details))
- The world sense tablet in the black egg temple does count.
- Willow grants an ability.
- Myla grants an ability. (Myla doesn't disappear)
- Quirrel in Crystal Peaks grants an ability. (Always present)
- Emilitia in the city grants a power.
- The statue of the Hollow Knight in the city grants a ability.

Currently not implemented:
- The shield on iselda's shop grants you a bonus but not ability. (Is not required for the dirtmouth loremaster)
- Lemm's sign on his shop (after getting a dreamer) grants a bonus but no ability (Is not required for city of tears loremaster)
- All Dream Warrior statues grant a bonus (not required).
- Elderbug grants the final power. (requires all other)

## Settings
You can change these options in the mod menu ingame:
- Use custom text: some text lines have been replaced with own ones, to make it more fitting for the ability.
- Use hints: If true, the powers will display a vaguely description instead of a detailed one. Default is true.

## Randomizer Use
This mod has compability with the Randomizer 4 and extension mod. The acquired power is displayed when you pick up the lore tablet, even if you don't get the text box, the name of the power will be displayed in the item message (bottom left when you pick up an item). The acquired local powers stay active until you change the area and then behave normally. If you didn't get the power somehow, you can do a workaround:
- Quit to Main Menu
- Open your tracker log (or spoiler log if you don't have the tracker file) and look for the tablet name.
- Search for the matching power. Currently you have to do it a bit more tedious:
  - Look at the RandomizerHelper.cs and find the ingame tablet name.
  - Look at the LoreMaster.cs and match the (ingame) tablet name from the power list (first in the file)
  - Copy the power name and proceed (e.g if your Tablet is ABYSS_TUT_TAB_01 you need to take WeDontTalkAboutShade from the {"ABYSS_TUT_TAB_01", new WeDontTalkAboutShadePower() } part.
- Create an options_Whatever your file slot is.txt (e. g. options_1.txt)
- Write %modify% in the first line.
- Write %PowerName% local | add in the second line (e. g. %WeDontTalkAboutShade% local | add)
- Save the file and continue the game.
This should grant you the ability. Note that the power may not be active because of the mod behavior, but it will still be added.

If you can't find the matching tablet name and the Modlog file prints out "Unrecognizable lore tablet name", please contact the mod developer!

## Manual Setup
You also can set up a predefined pack of abilites or changing their tag. For this, create a options_{FileSlot}.txt file (options_1.txt for example) in the loremaster folder of the game. A default option file in added to the package.
Write %Override% or %Modify% in the first line:
Override: Will reset your progress and let's you start with only your configuration.
Modify: Loads the save data from the file and then modify the data with your configuration.

Write the name of a power like %MyPower% and add an tag behind it. E. g:
```
%WellFocused% global
%GreaterMind% disable
```
You can use the name of the power displayed in game or the name of the class from the code (Excluding the "Power" text. e.g: WeDontTalkAboutShadePower -> WeDontTalkAboutShade in the file.)

You have five tags available:
- global: This power is always available once obtained.
- local: Default behaviour, granted in the zone or globally if you have all required power in the zone.
- exclude: This power is granted like normal, but is not required to get the lore master of the zone. Same conditions as local for staying active.
- disable: This power doesn't work, but it's collection still behave normally.
- remove: This power doesn't work and is not required to get the lore master of the zone.

If you want to give yourself the power instantly, you can add "|add" to the end of the line. Note, that this power will be still affected by the tag. So if you give you an ability with the local tag, it will only be available in the zone where it would normally be (or globally if you have all required in the zone).

A option file can look like this:
```
%Override%
%Imposter% global
%UnitedWeStand% global | add
%ScrewTheRules% disable
%True Form% exclude
```
This example would do the following:
- The Imposter Power will stay completely active once obtained.
- The United we Stand Power is acquired and permanently active.
- The Screw the Rules Power is a requirement for globally activating the powers in Dirtmouth/King's Pass but itself does nothing.
- The True Form ability can be acquired as normal but it will not be a requirement to fully enable the Dirtmouth/King's Pass powers globally.

You can ignore a line if you insert a "#" somewhere in the line.

## Powers

Here are all powers listed with their description (This contains the detailed descriptions, but this mod also adds vaguely hints. If you don't want to spoil yourself don't read further):

### Ancient Basin:

- We don't talk about Shade: You don't get the soul limit punishment, when dying. Your geo will still be on the shade.

### City of Tears:

- Eye of the Watcher: Grants the lantern effect. If you already have the lantern effect and would take lethal damage, you will be healed to full hp instead (with joni's you gaining 5 lifeblood instead). Once triggered, has to be restored by looking through the telescope by lurien.
- Hot Streak: When hitting an enemy with the nail, increases it's damage by 1 (max. 3 stacks per nail upgrade (15 total)). Resets if you don't hit an enemy.
- Marissas Audience: After 20 to 60 seconds spawn multiple companions (Weavers, Hatchlings, Grimmchilds) that persist in the current room or for 30 to 90 seconds. If    Marissa is dead, spawns Revek each 45 to 180 seconds, that persist in the current room or 20 to 60 seconds.
- Overwhelming Power: When you cast a spell while your soul vessel is full (not counting additional soul vessels), they deal twice as much damage and are twice as big.
- Soul Extract Efficiency: You gain 5 more soul per hit on enemies.
- Tourist: You can talk to the firefly by the statue of THK to teleport to black egg temple for 50 geo, or back to this room from the temple.
- Happy Fate: After sitting on a bench, your nail damage, nail range, running speed, dash speed and cdash charge up speed is increased. You also gain 1 soul per second. Fades away if you are getting hit.
- Delicious Meal: You can consume a rancid egg to heal you for 1 mask every 12 seconds and gain a +20% damage buff for 3 minutes. Press jump to consume the egg.

### Crossroads:

- Reluctant Pilger: While standing of the ground, the grubberfly elegy effect is active (regardless of your HP). If you have grubberfly equipped, the damage on ground is doubled instead.
- Greater Mind: Activates the tracker, to show you how many powers you are missing. If the counter is green, you have global access to the power in the area.
- Diamond Dash: Crystal Heart is cast 0.3 seconds faster and be hold midair with up press. Doubled, if you have Diamond Core unlocked. Drains 20 Soul per second (or 10 if you have Diamond Core).

### Deepnest:

- Infested!: Killing an enemy grants 1 to 5 weaverlings that assist you in the current room, despawn after 15 seconds or leaving the room
- Mask Overcharge: Overcharge one of your mask (it glows in different colors), while you have exactly that much health, a circle gathers around you that deal damage and restore 8 soul each second. The overcharged mask changes every 30 seconds and may never select the full hp mask. Inactive while you have Joni's Blessing equipped.

### Dirtmouth:

- Caring Shell: Enviroment Hazards (like spikes) don't deal damage to you anymore.
- Screw the Rules: Fury of the Fallen is now also active with 2 hp, but the damage buff is decreased to 50%. Grubberfly Beams are only nerfed by 40%.
- True Form: While your shade is active, you deal 30% more damage and increase your nail length by 25%. The effects are doubled, if you are in the same room as your shade.
- Well Focused: Focus is cast 30% faster.
- Requiem: Holding focus, while you dying will spawn you in dirtmouth instead of your bench. Holding dream nail will spawn you at spirit's glade instead (when the glade is open). Holding cDash will spawn you at hallownests crown.

### Fog Canyon:

- Friend of the Jellyfishes: You're immune to jelly fishs enemies and explosions. Note: Non Jelly fish explosion enemies, still deal 2 damage on contact. You're just immune to the explosion itself.
- Belly of the Jelly(fish): Decrease your falling speed by 25% and triples the time needed in air, for a hard fall.
- Jellyfish Flow: You swim 3 times as fast.

### Fungal Wastes:

- Eternal Valor: Each 30 hits on enemies, heal you for 1 mask.
- Glory of the Wealth: Enemies drop double geo.
- Imposter: While wearing spore shrooms, focus has a 20% chance to add a lifeblood (doesn't work if you have 3 or more lifeblood).
- Mantis Style: Increase your nail range by 50% (from base).
- One of Us: Every twelve seconds you cast the deep focus spore cloud. Hold the crystal dash button to prevent that (in case you want to do pogos for example).
- Pale Luck: When you would take damage, you have a 1% chance to be healed instead. Increased by 2% for each King's Brand and Kingssoul.
- United we Stand: Weavers are bigger, Grimmchild shoots faster (capped at 12) and Hatchling deal more damage for each companion (of those three) you have.
- Bag of Mushrooms: [BETA] Allows you to pick a mushroom to consume each 180 seconds. White shroom: Increases the speed of the game by 40%. Yellow shroom: Generates 8 soul each second, but causes nausea. Red shroom: Gives you 4 extra health, heals you fully and increases your nail damage by 20%, but you can't dash. Green shroom: Makes you small, decrease the gravity by 50% and doubles all damage taken. Taking the same mushroom twice in a row nerfs it's positive effect by 50%. Taking the same mushroom three times in a row, deals 2 damage to you instead. Press CDash to select another and quick map to consume the mushroom.

### Greenpath:

- Camouflage: After standing still for 5 seconds, you gain invincibility until you do something. If you are wearing Shape of Unn, you keep the invincibility while focusing and moving as a slug.
- Gift on Unn: Gain the shape of Unn effect for focusing. If you're wearing Shape of Unn, focus restores 15 soul on a successful cast.
- Mindblast of Unn: Hitting an enemy with the dream nail permanently increases the taken damage by 2. Bonus for wearing charm: (+1 Dreamwielder; +2 Dreamshield; +3 Shape of Unn)
- Grasp of Life: While running on the ground spawn a grass patch each second. Every 10th one is pale grass which, upon destroying, causes all other spawned grass by you to cast wraith that deal 10 damage (20 if shape of unn is equipped). Grass despawns after 10 seconds..
- Return to Unn: Your Movement speed is increased by 3 and your dash cooldown is reduces by 0.5 seconds, while you facing left.
- Touch Grass: Every 10 seconds standing on grass, you heal 1 mask. Decreased to 5 seconds if wearing Shape of Unn.

### Howling Cliffs:

- Joni's Protection: When going to another area, you will be granted 5 life blood (10 if you have Joni's equipped). Each 3 seconds a lifeblood will fade away.
- Lifeblood Wings: Currently does nothing.

### Kingdom's Edge:

- Concussive Strike: Great Slash and Dash Slash cause Concussion on their target for 3 seconds. Concussed enemies take 10% more damage from nail attacks and increase their knockback by 50% (66% of Heavy Blow). Nail hits on the target extend the duration by 0.5 seconds each. Cyclone Slash is not counted as a nail slash in this case. Also cause enemies to glance 10% of their hits, decreasing their damage by 1.
- Wisdom of the Sage: For each Mr. Mushroom stage that you completed, spells cost 1 soul less.
- You like Jazz?: You can now have 10 hatchlings at a time, they spawn twice at fast and cost only 25% of their normal soul.

### Queen's Garden:

- Ring of Flowers: Increase the damage of your nail arts by 10% for each recipient for the flower.
- Queen's Thorns: Thorns of Agony are now "Queen's Thorns", which removes the freeze on hit. restore soul if it hits an enemy and has a 33% chance to restore 1 hp if it kills an enemy.
- Follow the Light: When you hold left while casting dreamnail, it will spawn an orange portal by your side, which you can travel to from anywhere in the room. Hold right while casting dreamnail to warp to the portal. Warping will consume 1 essence and the portal. Neither get's consumed if you have awoken Dreamnail (the portal will swap position with you instead).

### Resting Grounds:

- Defeated Dreamers grant the dream nail an additional effect. Lurien: Roots the target for 3 seconds (15 seconds cooldown) Herrah: Spawn 2 weavers. Monomon: Per 100 Essence you have a 1% chance to instant kill the enemy (capped at 200 damage). Capped at 2400 Essence for 24%. (This is granted, if you enter the room)

### Crystal Peak:
- Diamond Core : Crystal Heart snares all enemies in the room if you hit a wall. The duration of the stun and c dash damage increases with the c dash duration.(Stun duration is capped at 10 seconds, gain 5 damage and 20% speed per second, gain invincibility after 3 seconds.)

### Waterways:

- Eternal Sentinel: Defender's Crest clouds are 150% bigger and tick twice as fast. Baldur shell now takes 10 hits instead of four. When getting hit, while baldur shell is up, you gain 15 soul if you are also wearing Defender's Crest.
- Relentless Swarm: Hits with flukes restore 2 soul, or 5 if the enemy died.

### White Palace:

- Diminishing Curse: If you take 15 hits, you will no longer count as overcharmed, resets if you sit on a bench. The UI only updates if you open the charm screen.
- Sacred Shell: You can longer take more than one damage per hit (excluding overcharmed). Default is excluded.
- Shadow Forged: Decrease the cooldown of shade cloak by 0.4 seconds and increases sharp shadow damage by 100%.
- Shining Bound: For each charm you're wearing, you gain 1 soul per 2 seconds.
