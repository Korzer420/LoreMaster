# LoreMaster
Hollow Knight Mod that adds abilities to the lore tablets.

# How it works
Reading a lore tablet displays the granted power name and a mostly vaguely description of what it does. The ability stays only active while you are in the zone, in which you found the lore tablet. If you have found all lore tablets in the zone, the powers are granted permanently.

# What does count?
I mostly oriented on the wiki page of lore tablets to see what actually counts. But there are a few addition, changes and removals to look out for:
- Menderbugs diary does nothing (I mean, you are granted "Regret" if you want to count it)
- Any of Bretta's diary entries does count for her power.
- The shield on iselda's shop grants you a bonus but not ability. (Is not required for the dirtmouth loremaster)
- Lemm's sign on his shop (after getting a dreamer) grants a bonus but no ability (Is not required for city of tears loremaster)
- Midwife and the mask maker grant abilities.
- The traitor's grave and the queen grant abilites.
- Hive Queen Vesper grants an ability. (Is required for Kingdom's edge lore master) WARNING: Dreamnailing her before talking, denies the power forever.
- Joni grants an ability (same conditions as Hive Queen Vesper).
- All Dream Warrior statues grant a bonus (not required).
- The journal entry at the end of PoP grant an ability (Ability is only shown at the lore tablet at the start of PoP)
- The world sense tablet in the black egg temple does count.

The only zone with no power to obtain is crystal peaks.

# Settings
You can change a few options in the mod menu ingame:
- Disable custom text (some text lines have been replaced with own ones, to make it more fitting for the ability)
- Only show power name (self explaining)
- Clearer hints (if enabled, instead of vaguely descriping the ability, it will show you exactly what it does (I'd recommend to don't play with this on for a better experience)).
- Disable all abilites (self explaining)

# Set up
You also can set up a predefined pack of abilites or changing their properties. For this, create a option.txt file in the loremaster directory of the game.
Write %Override% or %Modify% in the first line:
Override: Will reset your progress and let's you start with only your configuration.
Modify: Loads the save data from the mod and then modify the data with your configuration.

Write the name of a power like %MyPower% and add an state behind it. E. g:
%FocusPower% global
%WorldSense% disable

You have five states available:
- global: This power is always available.
- local: This power has been granted, but behavious normal.
- exclude: This power is granted like normal, but is not required to get the lore master of the zone.
- disable: This power doesn't work, but it's collection still behave normally.
- remove: This power doesn't work and is not considered in the logic at all.

A option file can look like this:
%Override%
%AmongUs% global
%UnitedWeStand% global
%ScrewTheRules% disable
%True Form% exclude

Take note that adding white spaces in between the % don't matter.

# Powers

Here are all powers listed:
