# Lore Master
Hollow Knight Mod that adds abilities to the lore tablets. (Requires Item Changer and SFCore)

## How it works
Reading a lore tablet displays the granted power name and a mostly vaguely description of what it does (or detailed description). The ability stays only active while you are in the zone, in which you found the lore tablet. If you have found all lore tablets in the zone, the powers are granted permanently. (Default)

## What does count?
I mostly oriented on the wiki page of lore tablets to see what actually counts. But there are a few addition, changes and removals to look out for:
- Menderbugs diary does nothing (I mean, you are granted "Regret" if you want to count it)
- Any of Bretta's diary entries does count for her power.
- Midwife and the mask maker grant abilities.
- The traitor's grave, Moss prophet and the queen grant abilites.
- Badoon grant an ability.
- The journal entry at the end of PoP grant an ability (Ability is only shown at the lore tablet at the start of PoP. This is the only power that by default is excluded (see below for more details))
- The world sense tablet in the black egg temple does count.
- Willow grants an ability.
- Myla grants an ability. (Myla doesn't disappear)
- Quirrel in Crystal Peaks grants an ability. (Always present)
- Emilitia in the city grants a power.
- The statue of the Hollow Knight in the city grants a ability.
- All ghosts outside glade except Cloth grant abilities.
- Record Bela grants an ability (extra lore tablet)

Currently not implemented:
- The shield on iselda's shop grants you a bonus but not ability. (Is not required for the dirtmouth loremaster)
- Lemm's sign on his shop (after getting a dreamer) grants a bonus but no ability (Is not required for city of tears loremaster)
- All Dream Warrior statues grant a bonus (not required).
- Elderbug grants the final power. (requires all other)

## Settings
You can change these options in the mod menu ingame:
- Custom Text: Some text lines can be replaced with own ones, to make it more fitting for the ability.
- Power Description: Determines if the powers should be descripted vaguely or precise.
- Disable yellow mushroom: If on, the yellow mushroom effect of the Bag of Mushroom ability will be disabled (Replaced with just a very very small soul gain).
- Allow Bomb Quickcast: If on, the bomb spell from "Grass Bombardement" can be cast via quick cast. If you want to do fireball skips it is recommended to turn this off.

## Inventory Page
Talking to Elderbug (or if randomized, finding the item) gives you access to the lore power inventory page in which you can see the hints/descriptions of the powers again and toggle them on/off while sitting on a bench.

- If the power shows a lore tablet image, you have obtained this power and it is active.
- If the power shows a red "x", you have obtained this power but it is disabled. (Through you or because you don't have the ability unlocked in the zone)
- If the power shows a gray "x", the power tag is "remove" which means the power doesn't work. After obtaining the power you can still activate it.
- If the power shows the dot, you haven't obtained the power yet.

If the power name is green you have global access to the power.

## Randomizer Use

This mod has compability with the Randomizer 4 and extension mod. The acquired power is displayed when you pick up the lore tablet, even if you don't get the text box, the name of the power will be displayed in the item message (bottom left when you pick up an item). The mod also offers a few extra settings:

- Randomize NPC: Randomizes the text of NPC (only the ones that would give you powers + Elderbug). Also randomizes the lore inventory page.
- Randomize Warrior Statues: Randomizes the inspect text of Dream Warrior Statues/Corpses.
- Cursed Reading: Randomizes the ability to read lore tablets and dream warrior statues/corpses.
- Cursed Listening: Randomizes the ability to talk to NPC. This includes all Merchants and Stags. Note that this also prevents you from talking to Grimm + Hornet 2 after fights, which results in having to redo the fight if you don't have the abilities.
- Power Behaviour: Determine how all powers should behave. You can use this if you just want the rando settings without the actual abilities or if you want to ignore the whole condition thing on abilities.
- Black Egg Temple Condition: Allows you to change the requirement of the Black Egg Temple Door to Lore (or both). The needed lore is capped at 31 (+ 18 if you randomize NPC, + 7 if you randomize Warrior statues). Note that the fountain, dreamer tablet and Record Bela are not considered for the logic even though they count for the door.

The fountain, Record Bela and the dreamer tablet cannot be randomized!

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

### Ancient Basin

### We don't talk about the Shade
- Hint: Shade? What should that be? Can you prove, that the "Shade" exist? Your soul vessel and geo? What are you talking about?
- Description: You don't get the soul limit punishment, when dying. Your geo will still be on the shade. When dying while the shade is active, your shade only loose 50% of your geo.

### City of Tears

### Hot Streak
- Hint: Successfully striking enemies shall increase your power. The power will fade away from your weapon if it doesn't hit a foe.
- Description: When hitting an enemy with the nail, increases it's damage by 1 (max. 3 stacks per nail upgrade (15 total)). Resets if you don't hit an enemy.

### Tourist
- Hint: Want to see the incredible black egg temple? Talk to the firefly to the right. Drinks are not included in the price. Also offers back travel.
- Description: You can talk to the firefly to the right to teleport to black egg temple for 50 geo, or back to this room from the temple.

### Marissas Audience
- Hint: While Marissa sings on stage, occasionally spawns a crowd that helps you. If you killed her however... you will be haunted by her biggest fan.
- Description: After 45 to 120 seconds spawn multiple companions (Weavers, Hatchlings, Grimmchilds) that persist in the current room or for 30 to 90 seconds. If Marissa is dead, spawns Revek each 45 to 120 seconds, that persist in the current room or 20 to 90 seconds.

### Soul Extract Efficiency
- Hint: Allows you to drain soul more efficient from your foes and draw you reserve more quickly.
- Description: You gain 5 more soul per hit on enemies and your extra soul vessel soul will be added instantly to be used for spells after casting spell. This does NOT affect focus, unless you do the wind up again.

### Overwhelming Power
- Hint: Casting spells with full capacity will grant your spell much more power. Be careful though, these spells are  so powerful that you can easily loose control of them, searing your vessel and shatter your soul. It will take some time to recover your soul.
- Description: When you cast a spell while your soul vessel is full (not counting additional soul vessels), they deal twice as much damage and are twice as big.

### Pure Spirit
- Hint: Using the last resort of your soul will purify it's vessel and spawn an orb out of pure light. If you channeled three orbs, they will follow your powerful spell and detonate upon contact, returning their gathered soul to you.
- Description: If you have 0 soul after casting a spell you will gain an rotating orb around you that deals 5 contact damage. If you have 3 orbs active, the next fireball will increase their size, let them explode upon contact for 20 damage and restore 10 soul. Soul Snatcher and Soul Eater will increase the soul gained by the orb. Shaman Stone will increase the orb explosion damage.

### Eye Of The Watcher
- Hint: The eye of the watcher will protect you and share it's sight, allowing you to see in the dark. If the eye can fully see you, it gaze may once prevent a fatal blow on you. To call the eye again, look through the tool of the watcher.
- Description: Grants the lantern effect. If you already have the lantern effect and would take lethal damage, you will be healed to full hp instead (with Joni's you gaining 5 lifeblood instead). Once triggered, has to be restored by looking through the telescope by lurien.

### Happy Fate
- Hint: Your "happiness" increases all your abilities slightly. Getting hit makes you sad. :c A good rest may restore your hapiness.
- Description: After sitting on a bench, your nail damage, nail range, running speed, dash speed and cdash charge up speed is increased. You also gain 1 soul per second.

### Blessing of the Butterfly
- Hint: Your Wings have received the blessing of the butterfly, which allows them to crush enemies beneath them and repel even higher. While the wings rest on you, your enemies shall be shattered by your feet. From your best girl (besides Myla) c: You can 100% be sure, that I'd end your game immediatly if you kill her, if it wasn't for the audience power. Though for another power in the future I'll not be that kind, just a fair warning. Don't mess with the innocent and happy bugs in this game. >:c
- Description: Wings now deal 12 damage upon contact with enemy, apply massive knockback downwards and give you slightly more height. While your double jump is on cooldown, instead of taking damage from enemies below you, you deal 4 damage to them, regain soul and jump automatically again. (This does not restore wings) You can 100% be sure, that I'd end your game immediatly if you kill her, if it wasn't for the audience power. Though for another power in the future I'll not be that kind, just a fair warning. Don't mess with the innocent and happy bugs in this game. >:c

### Delicious Meal
- Hint: You can now consume the "delicious" eggs to provide you a temporarily saturation effect to make your nail stronger and regenerate your wounds. Press jump on the egg in the inventory to consume the egg.
- Description: You can consume a rancid egg to heal you for 1 mask every 8 seconds and gain a +20% damage buff for 3 minutes. Press jump to consume the egg.

### Crossroads

### Reluctant Pilgrim
- Hint: While you stay on the path, your nail shall receive the gift of the grubfather.
- Description: While standing of the ground, the grubberfly elegy effect is active regardless of your hp (Joni conditions are still normal). If you have grubberfly equipped, the damage on ground is doubled instead. Also mutes the beams after a few casts.

### Greater Mind
- Hint: You can now sense, which knowledge of the world you're missing.
- Description: Activates the tracker, to show you how many powers you are missing. If the counter is green, you have global access to the power in the area.

### Diamond Dash
- Hint: The shell of crystal heart is powered with the might of diamonds, which causes a quicker energie cast and allows you to stop you mid air. Hold up to remain at your position. Drains souls rapidly.
- Description: Crystal Heart is cast 0.3 seconds faster and can be hold midair while pressing up. Doubled, if you have Diamond Core unlocked. Drains 20 Soul per second (or 10 if you have Diamond Core).

### Crystal Peaks

### Diamond Core
- Hint: The crystal heart's core absorbed the power of diamond and got even stronger. If you hit a wall, all foes may be stunned shortly. The power of the diamond increases over time, makes you unstoppable once you got enough power.
- Description: Crystal Heart snares all enemies in the room if you hit a wall. The duration of the stun and cdash damage increases with the c dash duration. (Stun duration is capped at 10 seconds, gain 5 damage and 10% speed per second, gain invincibility after 3 seconds.)

### Deepnest

### Mask Overcharge
- Hint: Let one your mask occasionly overcharge. If it is the one that protects you, it emits a searing circle, that also absorbs the loose soul around you.
- Description: Overcharge one of your mask (it glows in different colors), while you have exactly that much health, a circle gathers around you that deal damage and restore 8 soul each second. The overcharged mask changes every 30 seconds and may never select the full hp mask. Inactive while you have Joni's Blessing equipped.

### Infested!
- Hint: Plant spider eggs in the wounds of your victims, that burst open on death.
- Description: Hitting an enemy with the nail plants an egg to the enemy (Capped at 5), which spawns a weaver once the enemy died. Weavers also apply eggs (Capped at 25). Weavers despawn after 20 seconds or upon leaving the room.

### King's Pass/Dirtmouth

### Well Focused
- Hint: You gain pure focus faster.
- Description: Focus is cast 30% faster.

### Screw the Rules
- Hint: Your rage is weaker but grows quicker.
- Description: Fury of the Fallen is now also active with 2 hp, but the damage buff is decreased to 50%. Grubberfly Beams are only nerfed by 40%.

### True Form
- Hint: While the true form is revealed, its vessels nail gets more powerful. Especially near your true self.
- Description: While your shade is active, you deal 30% more damage and increase your nail length by 25%. The effects are doubled, if you are in the same room as your shade.

### Caring shell
- Hint: No hazard shall cast harm onto you... UwU
- Description: Enviroment hazards (like spikes) don't deal damage to you anymore.

### Requiem
- Hint: When your shell breaks, holding a pure focus may arise you at the home of death. Holding the light's artifact will instead catch you in the protecting wings of the last one, if she opened her secret. Holding the shiny stone vessel, will drop your soul onto the now abandoned place, where the light started its revenge.
- Description: Holding focus, while you dying will spawn you in dirtmouth instead of your bench. Holding dream nail will spawn you at spirit's glade instead (when the glade is open). Holding Crystal Dash will spawn you at Hallownests Crown (?), requires Crystal Heart to work.

### Fog Canyon

### Friend of the Jellyfishes
- Hint: Jellyfishs and explosions may no longer harm you.
- Description: You're immune to jellyfishs enemies and explosions. Note: Non Jellyfish explosion enemies, "could" still deal 2 damage on contact. You're just immune to the explosion itself.

### Belly of the Jelly(fish)
- Hint: You are feeling light, like a feather.
- Description: Decrease your falling speed by about 20% and triples the time needed in air, for a hard fall.

### Jellyfish Flow
- Hint: You've gained the swimming agility of the jellyfishs... And you are now a part of the jellyspotters!
- Description: You swim 4 times as fast.

### Fungal Wastes

### One of Us
- Hint: Occasionally you emit a spore cloud. (Hold the super dash button to prevent the cloud.)
- Description: Every twelve seconds you cast the deep focus spore cloud. Hold the crystal dash button to prevent that (in case you want to do pogos for example).

### Pale Luck
- Hint: When someone casts harm on you, sometimes you are blessed by the higher being instead. Especially if you have some artefacts related to him.
- Description: When you would take damage, you have a 2% chance to be healed instead. Increased by 4% for each King's Brand and Kingssoul. Also increased by 1 % for each king's idol you have.

### Imposter
- Hint: While being part of the shrooms, sometime you focus treacherous energys... AMOGUS
- Description: While wearing spore shrooms, focus has a 20% chance to add a lifeblood (doesn't work if you have 3 or more lifeblood).

### United we Stand
- Hint: Your companions inspire each other.
- Description: Weavers are bigger, Grimmchild shoots faster and Hatchling deal more damage for each companion (of those three) you have.

### Mantis Style
- Hint: Your weapon may reach further away foes.
- Description: Increase your nail range by 50% (from base).

### Eternal Valor
- Hint: The heat of the battle shall allow you to endure more pain.
- Description: Each 12 hits on enemies, heal you for 1 mask. Not hitting an enemy for 3 seconds will take away a stack each half of a second.

### Glory of the Wealth
- Hint: Your enemies may "share" more of their wealth with you. Your wealth may protect you from the hits of those peasants... for a fee of course. Press Quick Map + Up to activate the effect and Quick Map + Down to cancel the effect. (In case you forget this input combination, the effect also deactivates itself once the leave the room). The cost increase fast over time while the effect is active and slowly decrease once inactive. If you are really greedy, the cost decrease faster. The great civilians of Hallownest call this power "Pay 2 Win".
- Description: Enemies drop double geo. Press Quick Map + Up to become invincible. For each second this is active it drains 1 more geo. Press Quick Map + Down to cancel the effect (or leave the room). While the effect is inactive the cost decrease by 1 each 5 second (or each 2.5 seconds if you have Fragile/Unbreakable Greed equipped).

### Bag of Mushrooms
- Hint: Allows you to consume a yummy mushroom snack occasionly. The saturation may power you up. Caution: Can cause throw up if you eat too much of the same ones. Press quick map to select another and cdash + dash to consume the mushroom. WARNING: The yellow one causes a nausea effect! If you don't want to use that, you can turn off the effect in the mod settings. Eating the yellow mushroom then, will only give a small effect.
- Description: Allows you to pick a mushroom to consume each 180 seconds. White shroom: Increases the speed of the game by 40%. Yellow shroom: Generates 20 soul each second, but causes nausea. Red shroom: Gives you 4 extra health, heals you fully and increases your nail damage by 20%, but you can't dash. Green shroom: Makes you small, decrease the gravity by 50% and doubles all damage taken. Taking the same mushroom twice in a row nerfs it's positive effect by 50%. Taking the same mushroom three times in a row, deals 2 damage to you instead. Press quick map to select another and cdash + dash to consume the mushroom. WARNING: The yellow one causes a nausea effect! If you don't want to use that, you can turn off the effect in the mod settings. Eating the yellow mushroom then, will only give a small effect.

### Greenpath

### Touch Grass
- Hint: The flora may nourish your shell, if you stand long enough near it.
- Description: Every 8 seconds standing on grass, you heal 1 mask. Decreased to 4 seconds if wearing Shape of Unn.

### Gift of Unn
- Hint: Grants you the power of Unn.
- Description: Gain the shape of Unn effect for focusing. If you're wearing Shape of Unn, focus restores 15 soul on a successful cast.

### Mindblast of Unn
- Hint: Your dream nail emits the power of Unn to the target's mind, which causes their bodies to be more vulnerable.
- Description: Hitting an enemy with the dream nail permanently increases the taken damage by 2. Bonus for wearing charm: +1 Dreamwielder, +2 Dreamshield and +3 Shape of Unn. The effect is doubled with awoken dream nail.

### Camouflage
- Hint: While doing nothing, your mind slowly ascent to Unn's dream, while your shell is shielded by Unn's power.
- Description: After standing still for 5 seconds, you gain invincibility until you do something. If you are wearing Shape of Unn, you keep the invincibility while focusing and moving as a slug.

### Return to Unn
- Hint: Reject Bugness, return to Slug. You move faster to Unn.
- Description: Your Movement speed is increased by 3 and your dash cooldown is reduces by 0.5 seconds, while you facing left.

### Grasp of Life
- Hint: While running on the ground, the breath of life shall grow grass below your feet. Sometimes, a pale blessed one may appear instead. Killing it, shall let your hear the cheers of the normal one.
- Description: While running on the ground spawn a grass patch each second. Every 10th one is "pale grass" which, upon destroying, causes all other spawned grass by you to cast wraith that deal 10 damage (20 if shape of unn is equipped). Grass despawns after 10 seconds.

### Howling Cliffs

### Lifeblood Omen
- Hint: Sometimes you will be haunted by a ghost from a distant land. Killing it will grant you the essence of it's soul. A more powerful weapon may attract more powerful foes.
- Description: Spawns a grimmkin every 180 seconds. Killing the ghost grants 3/6/9 lifeblood (based on ghost level). The ghost disappears if you leave the room or if 90 seconds passed. The chances for the ghost adjust based on your current nail level. I'll will not list the chances here. Look at the repository, if you want to know.

### Joni's Protection
- Hint: When going to a new area, you will receive the gift of Joni, which will quickly fade away.
- Description: When going to another area, you will be granted 5 life blood (10 if you have Joni's equipped). Each 3 seconds a lifeblood will fade away.

### Kingdom's Edge

### Wisdom of the Sage
- Hint: Guide the Sage through his journey to learn how to use your spells more efficient.
- Description: For each Mr. Mushroom stage that you completed, spells cost 1 soul less.

### Concussive Strikes
- Hint: Your huge nail swings cause Concussion on their target, which will cause the target to suffer more from your nail and extend the concussion.
- Description: Great Slash and Dash Slash cause Concussion on their target for 3 seconds. Concussed enemies take 10% more damage from nail attacks and increase their knockback by 50% (66% of Heavy Blow). Nail hits on the target extend the duration by 0.5 seconds each. Cyclone Slash is not counted as a nail slash in this case. Also cause enemies to glance 50% of their hits, decreasing their damage by 1.

### You like Jazz?
- Hint: Your hatchlings hatch way more aggressive and don't need that much soul to grow. How long did it took you for mashing throw the textbox? :)
- Description: You can now have 10 hatchlings at a time, they spawn twice at fast and cost only 25% of their normal soul. How long did it took you for mashing throw the textbox? :)

### Queens Garden

### Ring of Flowers
- Hint: The power of all existing flower are gathered if you channel powerful strikes.
- Description: Increase the damage of your nail arts by 10% for each recipient for the flower.

### Queen's Thorns
- Hint: The thorns of agony have received the blessing of the queen.
- Description: Thorns of Agony are now "Queen Thorns", which removes the freeze on hit. restore soul if it hits an enemy and has a 33% chance to restore 1 hp if it kills an enemy.

### Follow the Light
- Hint: Allows you to weave with the pure essence of light to form a portal which you can travel to. Moving to far away will destroy the unstable portal. Hold left while swinging the artifact of the light to weave the portal and hold right to travel through it. As long as the artifact has not its full potential, travelling will consume the energy of the dreams and destroy the portal once entered. If not, the portal will remain at your entry point. (Requires dream gate to work)
- Description: When you hold left while casting dreamnail, it will spawn an orange portal by your side, which you can travel to from anywhere in the room (Requires dream gate to work). Hold right while casting dreamnail to warp to the portal. Warping will consume 1 essence and the portal. Neither get's consumed if you have awoken Dreamnail (the portal will swap position with you instead).

### Grass Bombardement
- Hint: Forms the grass in Hallownest with the power of the soul to a "special delivery" which explodes shortly after creation, dealing huge damage. The disruption may break loose walls and floors. Requires soul to construct the bomb. Press (Quick)cast and left to drop the bomb. They say, that you also can channel the blue plague, to create an even stronger bomb which can even break the heaviest stones and emits a destruction shockwave through the room. Press (Quick)cast and right to consume the blue blood and spawn the powerful nuke.
- Description: Pressing left while casting. spawns a bomb which explodes after 3 seconds, that deals 40 damage (60 with shaman stone) and breaks damaged walls/ground. Pressing right, will consum a lifeblood mask to spawn a more powerful bomb, with a bigger radius, 50 % more damage and the ability to break ALL damaged floors/walls in the room, even heavy floors and one way walls.

### Resting Grounds

### Dream Blessing
- Hint: The dream artifact uses the power it absorbs from their powerful victims to use it's hidden power.<br>Monomon: Through her knowledge she exposes the foes biggest weakness.<br>Lurien: His gaze may freeze the enemy in place.<br>Herrah: Invoking her children from the victim.
- Description: Defeated Dreamers grant the dream nail an additional effect (doubled with awoken dreamnail).<br>Lurien: Roots the target for 3 seconds (15 seconds cooldown)<br>Herrah: Spawn 2 weavers. <br>Monomon: Per 100 Essence you have a 1% chance to instant kill the enemy (capped at 125 damage). Capped at 2400 Essence for 24%.

### Waterways

### Eternal Sentinel
- Hint: Increases your scent while wearing the sign of the protector. The shield of the ancient ones is more resistent and gather soul with the blessing of the protector.
- Description: Defender's Crest clouds are 150% bigger and tick twice as fast. Baldur shell now takes seven hits instead of four (ten with defender's Crest. When getting hit, while baldur shell is up, you gain 15 soul if you are also wearing Defender's Crest.

### Relentless Swarm
- Hint: Flukes rip the soul out of there victims.
- Description: Hits with flukes restore 2 soul, or 5 if the enemy died.

### White Palace

### Shadow Forged
- Hint: The void energy return quicker to you.
- Description: Decrease the cooldown of shade cloak by 0.2 seconds and increases sharp shadow damage by 100%.

### Shining Bound
- Hint: Every magic relic you currently wearing  shall gather more soul for you.
- Description: For each charm you're wearing, you gain 1 soul per 2 seconds.

### Diminishing Curse
- Hint: If you suffer from the curse of greed, it will vanish once you experienced enough pain after resting.
- Description: If you take 10 hits, you will no longer count as overcharmed, resets if you sit on a bench. The UI only updates if you open the charm screen.

### Sacred Shell
- Hint: Infuse your shell with the pale power of the monarch which grants you tenacity against strong strikes.
- Description: You can longer take more than one damage per hit (excluding overcharmed).
