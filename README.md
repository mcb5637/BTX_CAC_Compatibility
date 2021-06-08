# BTX_CAC_Compatibility

Modifies BTX (and BTXMinusWeapons, if present) for CAC.

Installation: (requires working BTX installation)
- Download CustomBundle https://github.com/BattletechModders/CustomBundle		(i use 0.3.12)
- Download IRTweaks https://github.com/BattletechModders/IRTweaks				(i use REL_0.9.2.6)
- Download IRBTModUtils https://github.com/BattletechModders/IRBTModUtils		(i use REL_1.10.0)
- Remove BTMLColorLOSMod and MechResizer from your BTX installation (as CAC and CU basically do the same)
- Remove FewerHeadInjuries from your BTX install (IRTweaks is configured to to the same, just better)
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
        - Tag: attacking a tagged unit has an +3 accuracy boost, tag gets removed when unit moves, upgraded tags have a bonus to its own hit chance
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
        - NARC: attacking a narced unit has an +3 accuracy boost, narc pod gets removed after 2 to 4 rounds (depends on launcher), ecm blocks narc acc bonus
        - ATM: added 3 ammo types, trading damage for range, added clustering
        - Infernos: Broken, use SRM inferno ammo instead
    - Artillery
        - Thumper: light artillery (replaces HM mortar / + version for Bull Shark)
        - Sniper: medium artillery (found in mining shops)
        - Long Tom: heavy artillery (special, only for Bull Shark with special event)
        - Arrow IV: medium artillery (lostech / FP reward / Liao shops after 3049)
- Electronics
    - ECM
        - Guardian ECM: -20% detectability, 180m aura (+4 defense, indirect immune, sensorlock immune) (friendly only) (blue)
        - Liao Prototype ECM: -10% detectability, 90m aura (+4 defense, indirect immune, sensorlock immune) (friendly only) (blue)
        - Packrat ECM (only in Packrat vehicle): 90m aura (+4 defense, indirect immune) (friendly + enemy) (blue)
    - AP
        - Beagle Active Probe: +150m sensor range, free action sensor lock, 120m active probe ping (free action) (brown)
        - Liao Prototype AP: +100m sensor range, free action sensor lock, 90m active probe ping (brown)
    - Mech Quiks
        - Improved Sensors Quirk: +50m sensor range (stacks with AP)
        - Improved Comms Quirk: 200m aura (removes indirect immune) (hostile only) (green)
	- Stealth:
		- Null Signature System: Activatable (+2 defense, -50% detectability, +1 stealth, sensorlock immune, +10 heat)
		- Chameleon Light Polarization Shield: Activatable (+2 defense, -50% visibility, +1 stealth, +6 heat)
- Upgrades
	- AMS: Broken, use Ballistic Weapon AMS instead
	- TSM: Auto activates at >27 heat (\*2 melee damage, + 60m movement)
	- Prototype TSM: Auto activates at >27 heat (\*1.5 melee damage, +30m movement)
	- MASC: Activatable (\*2 speed) (fail chance 15%, add up per turn in use)
- Argo Upgrades
	- Storage: added 3 additional storage upgrades, each giving a new mechbay to use
	- Engine Repairs: The storage upgrades do fix BiggerDrops tonnage increase upgrades, by giving you a way to fullfill their requirements
- Indirect Fire changes:
	- Shooting at something you cannot see (but your ally can) is considered Indirect Fire, and in turn will be blocked if the target is covered by ECM
	- You only get an accuracy penalty for Indirect Fire if you have to shoot over obstacles to hit your target (if it was considered Indirect Fire by the old rules)
- Mechs:
	- Goliath GOL-1H (quad, SLDF/SW) (low chance everywhere, factory Stewart)
	- Goliath GOL-3M (quad, HC) (marik after 3047, factory Stewart after 3048)
	- Helepolis HEP-3H (sniper arty, SLDF) (comstar/snords) (TODO heat?)
	- Phoenix Hawk LAM PXH-HK2 (SLDF LAM) (comstar) (HK1(R)?)
	- Stinger LAM STG-A5 (SLDF LAM) (comstar/kurita, factory Irece) (A1?)
	- Wasp LAM WSP-105 (SLDF LAM) (comstar) (100b?)

TODO List:
- edit texts
- vehicle updates (inferno?)
- NARC reveals target?
- Flamers & Inferno ammo balance
- ECM tohit balance
- ac projectiles per shot, if possible
- new mechs: LAMs

Known bugs:
- CustomLoc patchig strings

Optional:
- If you want Urban vehicles to leave blood on destruction, look at CACs settings and change "DrawBloodChance" to 0.3
- manual deployment can be enabled via CU, set "DeployManual": to true to do so (warning: laggy)
 
Credits:
- CMiSSioN for CustomBundle
- everyone involved in RougeTech (also Crackfox for Vanilla CAC) for examples and inspiration on how to use CB
- Haree for BTX
- Hounfor for the Goliath, LAM and Helepolis icons
- lordruthermore for half working Goliath jsons
- Pode for half working LAM jsons
- Pode/bhtrail for looking up mech loadouts
