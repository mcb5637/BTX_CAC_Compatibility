# BTX_CAC_Compatibility

Modifies BTX (and BTXMinusWeapons, if present) for CAC.

Installation: (requires working BTX installation)
 - Download CustomBundle https://github.com/BattletechModders/CustomBundle version >= 0.2.96
 - Download IRTweaks https://github.com/BattletechModders/IRTweaks
 - Download IRBTModUtils https://github.com/BattletechModders/IRBTModUtils
 - Remove BTMLColorLOSMod and MechResizer from your BTX installation (as CAC and CU basically do the same)
 - Add IRBTModUtils and IRTweaks to your mods folder
 - Add CustomAmmoCategories, CustomComponents, CustomLocalization, CustomActivatableEquipment and CustomUnits to your mods folder (only add these 5 from CustomBundle, the rest is not needed)
 - Remove the folder CustomAmmoCategories/StreamingAssets
 - Add BTX_CAC_Compatibility, overriding files from anything previously mentioned and MissionControl and BiggerDrops


Component list (Clan & SLDF ones included):
- Weapons (& Ammo)
    - Energy
        - PPC: added FI OFF mode
        - ER PPC: -
        - Snub PPC: single projectile, damage falloff over medium range
        - Laser: -
        - ER Laser: -
        - Pulse Laser: fixed animation
        - Tag: attacking a tagged unit has an accuracy boost, tag gets removed when unit moves, upgraded tags have a bonus to its own hit chance
        - Flamer: added forestfires
    - Ballistic
        - AC: fixed firing speed, only one visual projectile
        - LBX: added cluster/slug ammo, cluster ammo uses CAC shells instead of multiple projectiles, small range increase (to TT values)
        - UAC: fixed firing speed, only one visual projectile, added AC mode (1 shot, but +4 acc), minimal range increase (to TT values)
        - Gauss: -
        - MG: added double speed mode (double shots, -4 acc, +5 heat)
		- AMS: MG that shoots at incoming missles (20 shots at 0.5 acc) (can overload for 30 shots + jam chance / can be used as MG)
    - Missle
        - LRM: added hotload mode
        - Artemis IV LRM: added hotload mode, changed acc to +4 in direct fire (+0 indirect), added clustering
        - SRM: added inferno ammo (inferno causes fires everywhere)
        - Artemis IV SRM: added inferno ammo, added clustering, +4 acc
        - Streak SRM: added inferno ammo, added streak effect+clustering
        - Narc: -
        - ATM: added 3 modes instead of 3 ammo types, trading damage for range (Clan magic lets them be loaded with exactly the correct ammo types)
        - Infernos: Broken, use SRM inferno ammo instead (TODO check if something is still using them)
    - Artillery
        - Thumper: light artillery (replaces HM mortar / + version for Bull Shark)
        - Sniper: medium artillery (found in mining shops / TODO: add Helepolis HEP-3H mech, used by ComStar)
        - Long Tom: heavy artillery (special, only for Bull Shark with special event)
        - Arrow IV: medium artillery (lostech / FP reward / TODO: check how to add to itemcollections after 3044)
- Electronics
    - ECM
        - Guardian ECM: -20% detectability, 180m aura (+4 defense, indirect immune, sensorlock immune) (friendly only) (blue)
        - Liao Prototype ECM: -10% detectability, 90m aura (+4 defense, indirect immune, sensorlock immune) (friendly only) (blue)
        - Packrat ECM (only in Packrat vehicle): 90m aura (+4 defense, indirect immune) (friendly + enemy) (blue)
    - AP
        - Beagle Active Probe: +150m sensor range, free action sensor lock, 120m active probe ping (free action) (brown)
        - Liao Prototype AP: +100m sensor range, 90m active probe ping (brown)
    - Mech Quiks
        - Improved Sensors Quirk: +50m sensor range (stacks with AP)
        - Improved Comms Quirk: 200m aura (removes sensorlock immune) (hostile only) (green)
- Upgrades
	- AMS: Broken, use Ballistic Weapon AMS instead
	- TSM: Auto activates at >27 heat (\*2 melee damage, + 60m movement)
	- Prototype TSM: Auto activates at >27 heat (\*1.5 melee damage, +30m movement)
	- MASC: Activatable (\*2 speed) (fail chance 15%, add up per turn in use)
 
 - TODO: edit texts
 - TODO: Tag & Narc to hit bonus instead of damage bonus?
 - TODO: NullSignature & Chamaeleon
 - TODO: targeting patch for indirect fire
 - TODO: Flamers & Inferno ammo balance
 - Known bugs: CustomLoc patchig strings, inferno/flamer floaties

Optional:
 - If you want Urban vehicles to leave blood on destruction, look at CACs settings and change "DrawBloodChance" to 0.3
 - manual deployment can be enabled via CU, set "DeployManual": to true to do so (warning: laggy)
 
 - TODO: Vehicles?

Credits:
- CMiSSioN for CustomBundle
- everyone involved in RougeTech (also Crackfox for Vanilla CAC) for examples and inspiration on how to use CB
- Haree for BTX
