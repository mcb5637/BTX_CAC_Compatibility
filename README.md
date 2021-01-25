# BTX_CAC_Compatibility

Modifies BTX (and BTXMinusWeapons, if present) for CAC.

Installation: (requires working BTX installation)
 - Download CustomBundle https://github.com/BattletechModders/CustomBundle
 - Download IRTweaks https://github.com/BattletechModders/IRTweaks
 - Download IRBTModUtils https://github.com/BattletechModders/IRBTModUtils
 - Remove BTMLColorLOSMod and MechResizer from your BTX installation (as CAC and CU basically do the same)
 - Add IRBTModUtils and IRTweaks to your mods folder
 - Add CustomAmmoCategories, CustomComponents, CustomLocalization, CustomActivatableEquipment and CustomUnits to your mods folder (only add these 5 from CustomBundle, the rest is not needed)
 - Remove the folder CustomAmmoCategories/StreamingAssets
 - Add BTX_CAC_Compatibility, overriding the settings of CustomAmmoCategories, CustomUnits, CustomActivatableEquipment, IRTweaks, MissionControl and BiggerDrops


Component list (Clan & SLDF ones included):
 - PPC: added FI OFF mode
 - ER PPC: -
 - Snub PPC: single projectile, damage falloff over medium range
 
 - AC: fixed firing speed, only one visual projectile
 - LBX: added cluster/slug ammo, cluster ammo uses CAC shells instead of multiple projectiles
 - UAC: fixed firing speed, only one visual projectile, added AC mode (1 shot, but +4 acc)
 - Gauss: -
 
 - Laser: -
 - ER Laser: -
 - Pulse Laser: fixed animation
 - Tag: -
 
 - LRM: added hotload mode
 - Artemis IV LRM: added hotload mode, changed acc to +4 in direct fire (+0 indirect), added clustering
 
 - SRM: added inferno ammo (inferno causes fires everywhere)
 - Artemis IV SRM: added inferno ammo, added clustering, +4 acc
 - Streak SRM: added inferno ammo, added streak effect+clustering
 - Infernos: Broken, use SRM inferno ammo instead (TODO check if something is still using them)
 - Narc: -
 
 - Flamer: added forestfires
 - MG: added double speed mode (double shots, -4 acc, +5 heat)
 
 - ATM: added 3 modes instead of 3 ammo types, trading damage for range (Clan magic lets them be loaded with exactly the correct ammo types)
 
 - Thumper: light artillery (replaces HM mortar / + version for Bull Shark)
 - Sniper: medium artillery (found in mining shops / TODO: add Helepolis HEP-3H mech, used by ComStar)
 - Long Tom: heavy artillery (special, only for Bull Shark with special event)
 - Arrow IV: medium artillery (lostech / FP reward / TODO: check how to add to itemcollections after 3044)
 
 - Guardian ECM: -20% detectability, 180m aura (+4 defense, indirect immune, sensorlock immune) (friendly only) (blue)
 - Liao Prototype ECM: -10% detectability, 90m aura (+4 defense, indirect immune, sensorlock immune) (friendly only) (blue)
 - Packrat ECM (only in Packrat vehicle): 90m aura (+4 defense, indirect immune) (friendly + enemy) (blue)
 
 - Beagle Active Probe: +150m sensor range, free action sensor lock, 120m active probe ping (free action) (brown)
 - Liao Prototype AP: +100m sensor range, 90m active probe ping (brown)
 
 - Improved Sensors Quirk: +50m sensor range (stacks with AP)
 - Improved Comms Quirk: 200m aura (removes sensorlock immune) (hostile only) (green)
 
 - TODO: edit texts
 - TODO: AMS
 - WIP: EWS
 - TODO: Tag & Narc to hit bonus instead of damage bonus?
 - TODO: MASC & TSM

Optional:
 - If you want Urban vehicles to leave blood on destruction, look at CACs settings and change "DrawBloodChance" to 0.3
 
 - TODO: Vehicles?
