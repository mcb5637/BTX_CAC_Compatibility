# BTX_CAC_Compatibility

Modifies BEX (and BTXMinusWeapons, if present) for CAC.

Installation/Update: (requires working BEX installation)
- Add BEX BiggerDrops addon (if not already present)
- install a new ModTek (4.1+) (if not already present)
	- remove the old ModTek and .modtek folders
	- verify files (Steam/GOG) / reinstall Battletech (anything else) (to remove ModTek <3.0, if present)
	- download and unzip ModTek to the correct folder: https://github.com/BattletechModders/ModTek/blob/master/INSTALL.md
	- direct download link: https://github.com/BattletechModders/ModTek/releases/download/v4.1.0/ModTek.zip
- Remove BTMLColorLOSMod, MechResizer and StablePiloting from your BTX installation (as CAC, CU and MechAffinity basically do the same)
- update CAB
- remove IRBModUtils (CAC-C for now contains their own versions of them)
- remove any previous version of CAC-C, CAE, CAC, CC, CLoc, CPrewarm, CU, CVoices, if they exist
- Add BTX_CAC_Compatibility, overriding files


Component list (Clan & SLDF ones included):
- Weapons (& Ammo)
    - Energy
        - PPC: added FI OFF mode
        - ER PPC: -
        - Snub PPC: 5 projectiles, damage falloff over medium range, upgraded version have extra mode with 1 projectile
        - Laser: -
        - ER Laser: -
		- Heavy Laser: -
		- Binary Laser: fixed animation
        - Pulse Laser: fixed animation
		- X-Pulse Laser: changed to single projectile, changed effect to pulse laser
        - Tag: attacking a tagged unit has an +3 accuracy boost, tag gets removed when unit moves, upgraded tags have a bonus to its own hit chance
        - Flamer: added forestfires
    - Ballistic
        - AC: fixed firing speed, only one visual projectile
        - LBX: AC mode to fire AC ammo (slugs), cluster ammo uses CAC shells instead of multiple projectiles, small range increase (to TT values)
        - UAC: fixed firing speed, only one visual projectile, added AC mode (1 shot, but +4 acc, lowered recoil, quartered (halved for 20s)) heat, minimal range increase (to TT values)
        - Gauss: -
		- Silver Bullet Gauss: fixed multiple projectiles
		- Heavy Gauss: -
		- Light Gauss: -
        - MG: added double speed mode (double shots, -4 acc, +5 heat)
		- AMS: MG that shoots at incoming missles (20 shots at 0.5 acc) (can overload for 30 shots + jam chance / can be used as MG)
    - Missle
        - LRM: added hotload mode, added Deadfire ammo
        - Artemis IV LRM: turned into a Weapon Addon for LRMs (deprecated, but still working)
        - SRM: added inferno ammo (inferno causes fires everywhere), added Deadfire ammo
        - Artemis IV SRM: turned into a Weapon Addon for SRMs (deprecated, but still working)
        - Streak SRM: added inferno ammo, added streak effect, added Deadfire ammo
        - NARC: attacking a narced unit has an +3 accuracy boost, narc pod gets removed after 2 to 4 rounds (depends on launcher), ecm blocks narc acc bonus
        - ATM: added 3 ammo types, trading damage for range, added clustering
        - Infernos: Broken, use SRM inferno ammo instead
		- MRM: individual hit generator, unguided flag
		- RL: individual hit generator, unguided flag
		- iNarc: attacking a narced unit has an +3 accuracy boost, narc pod gets removed after 2 to 4 rounds (depends on launcher), ecm blocks narc acc bonus
    - Artillery
        - Thumper: light artillery (replaces HM mortar / + version for Bull Shark)
        - Sniper: medium artillery (found in mining shops)
        - Long Tom: heavy artillery (special, only for Bull Shark with special event)
        - Arrow IV: medium artillery (lostech / FP reward / Liao shops after 3049)
- Electronics
    - ECM
        - Guardian ECM: -20% detectability, 180m aura (ECM: +4 defense, indirect immune, sensorlock immune, friendly only, blue or ECCM: negates 1 ECM, cyan)
        - Liao Prototype ECM: -10% detectability, 90m aura (ECM: +4 defense, indirect immune, sensorlock immune, friendly only, blue or ECCM: negates 1 ECM, cyan)
        - Packrat ECM (only in Packrat vehicle): 90m aura (+4 defense, indirect immune, friendly + enemy, blue)
    - AP
        - Beagle Active Probe: +150m sensor range, free action sensor lock, 120m active probe ping (free action) (brown)
        - Liao Prototype AP: +100m sensor range, free action sensor lock, 90m active probe ping (free action) (brown)
    - Mech Quiks
        - Improved Sensors Quirk: +50m sensor range (stacks with AP)
        - Improved Comms Quirk: 200m aura (removes indirect immune) (hostile only) (green)
	- Stealth:
		- Null Signature System: Activatable (+2 defense, -50% detectability, +1 stealth, sensorlock immune (counts as 10 Guardians), +10 heat)
		- Chameleon Light Polarization Shield: Activatable (+2 defense, -50% visibility, +1 stealth, +6 heat)
- Upgrades
	- AMS: Broken, use Ballistic Weapon AMS instead
	- TSM: Auto activates at >27 heat (\*2 melee damage, + 60m movement)
	- Prototype TSM: Auto activates at >27 heat (\*1.5 melee damage, +30m movement)
	- MASC: Activatable (\*2 speed) (fail chance 15%, add up per turn in use)
	- Coolant Pod: doubles heatsinking for 1 turn, 1 activation per pod (does not stack)
	- Artemis IV FCS: attaches to one SRM or LRM launcher and gives it: +4 direct fire acc, better clustering
	- PPC Capacitor: attaches to one PPC or ERPPC and gives it: CAP Mode (+25 DMG, +15 Heat, 1 turn Weapon Cooldown)
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
	- UrbanMech LAM UM-LAM-X (Experimental LAM) (Kurita/Liao Flying High Again mission)

What to do when adding CAC-C into an existing savegame:
- Store and ready every mech that has any of the following fixed equipment:
	- any ECM (you are fine if the component has - X% Detectability as bonus)
	- any ActiveProbe (you are fine if the component has +Xm Sensor Range as bonus)
	- any TSM (you are fine if the in mission GUI shows only one active component for it)
	- MASC (you are fine if the in mission GUI shows only one active component for it)
	- Bull Shark BSK-MAZ (you are fine if it has a fixed Thumper ballistic weapon)
	- Exterminator EXT-4D (you are fine if it has Null Signature and Chameleon)
- Remove any equipment and use its new replacements (use the savegame editor to change the parts in your inventory if you want):
	- Liao ECM (Gear_Sensor_Prototype_ECM -> Gear_SensorCAC_LiaoProtoECM)
	- Liao AP (Gear_Sensor_Prototype_ActiveProbe -> Gear_SensorCAC_LiaoProtoAP)
	- AMS upgrade components (Gear_AMS_XXX -> Weapon_AMSCAC_XXX) to AMS weapons & MG ammo
	- Infernos Launcher (Weapon_Inferno_Inferno2_XXX -> Ammo_AmmunitionBox_Generic_SRM_Inferno) to standard SRMs with Inferno ammo
	- Deadfire LRMs/SRMs (Weapon_SRM_DFSRMX_0-STOCK -> Ammo_AmmunitionBox_Generic_SRM_DF)(Weapon_LRM_DFLRMX_0-STOCK -> Ammo_AmmunitionBox_Generic_LRM_DF) to standard LRMs/SRMs with deadfire ammo
	- Mech Mortar (Gear_Mortar_MechMortar -> Weapon_MortarCAC_ThumperFree) to Thumper with matching ammo (requires ballistics slot)
	- Artemis SRM/LRM (Weapon_SRM_ASRMXXX/Weapon_LRM_ALRMXXX ->  Gear_Addon_Artemis4) to a matching standard SRM/LRM launcher with Artemis IV FCS attachment

TODO List:
- quad critable arm actuators to leg actuators
- narc/inarc ammo types
- imp sensor unlocks sensorlock without skill (add to irtweaks)
- multitarget quirk unlocks without skill? (irtweaks stat)
- ac projectiles per shot, if possible
- a-pods CAE?
- artillery mounting rework
- edit texts
- vehicle updates (inferno?)
- NARC reveals target?
- Flamers & Inferno ammo balance
- ECM tohit balance
- new mechs: LAMs

Known bugs:
- CustomLoc patchig strings

Optional:
- i recommend having a look at BT_Extended_CE/mod.json settings and enable some of these in all modes
- If you want Urban vehicles to leave blood on destruction, look at CACs settings and change "DrawBloodChance" to 0.3
- manual deployment can be enabled via CU, set "DeployManual": to true to do so (warning: laggy)
- if you do not want to drop more than 4 mechs, i recommend turning off MissionControls AdditionalLances

Manual setup:
- clone git repo, including submodules
- correct paths in dll
- compile
Package for release:
- update version info in mod.json and dll
- setup additionalDependencies
	- ModTek 4.1
	- SimpleMechAssembly
	- IRBModUtils
	- BiggerDrops
	- MechAffinity 1.4 (dll only)
	- MissionControl
- compile as release
- use pack.bat
 
Credits:
- CMiSSioN for CustomBundle
- everyone involved in RougeTech (also Crackfox for Vanilla CAC) for examples and inspiration on how to use CB
- Haree for BTX, Tarantula and Scorpion jsons
- Hounfor for the Quad, LAM and Helepolis icons
- lordruthermore for half working Goliath jsons
- Pode for half working LAM jsons
- Pode/bhtrail for looking up mech loadouts
- Hounfor + Graywolfe for helping updating to BTX 1.9.3
- Hounfor for Flying High Again contracts
- Kierk for Scorpion/Tarantula pathing
