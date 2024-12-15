# BTX_CAC_Compatibility

Modifies BEX (and BTXMinusWeapons, if present) for CAC.

Installation/Update: (requires working BEX installation)
- Remove BTMLColorLOSMod, MechResizer and StablePiloting from your BTX installation (as CAC, CU and MechAffinity basically do the same)
- update CAB
- remove IRBModUtils (CAC-C for now contains their own versions of them)
- remove any previous version of CAC-C, CAE, CAC, CC, CLoc, CPrewarm, CU, CVoices, if they exist
- Add BTX_CAC_Compatibility, overriding files

Versioning:
The first 2 parts of the version number match the intended BEX version number.
The other 2 follow semantic versioning.
A "b" as last char in the github release/git tag indicates beta status, this is not included in assembly/mod.json.
There will not be different releases that differ only in this beta marker.

Component list (Clan & SLDF ones included):
- Weapons (& Ammo)
    - Energy
        - PPC: added FI OFF mode
        - ER PPC: -
        - Snub PPC: 5 projectiles, damage falloff over long range, extra mode with 1 projectile
        - Laser: -
        - ER Laser: -
		- Heavy Laser: -
		- Binary Laser: fixed animation
        - Pulse Laser: fixed animation
		- X-Pulse Laser: changed to single projectile
        - Tag: attacking a tagged unit ignores evasion+indirect penalities, tag gets removed when unit moves, upgraded tags have a bonus to its own hit chance
        - Flamer: added forestfires
    - Ballistic
        - AC: fixed firing speed, special ammo types
        - LBX: can switch ammo between cluster and ac (including special ac ammo types)
        - UAC/RAC: fixed firing speed, no access to special ammo types
        - Gauss: -
		- Silver Bullet Gauss: fixed multiple projectiles
		- Heavy Gauss: -
		- Light Gauss: -
        - MG: added double speed mode (double shots, -4 acc, +5 heat)
		- AMS: MG that shoots at incoming missles (20 shots at 0.5 acc) (can overload for 30 shots + jam chance (fires at all nearby missles) / can be used as MG)
    - Missle
        - LRM: added hotload mode, added Deadfire ammo (narc, tag or artemis compatible)
        - Artemis IV LRM: turned into a Weapon Addon for LRMs (deprecated)
        - SRM: added inferno ammo (inferno causes fires everywhere), added Deadfire ammo (narc or artemis compatible)
        - Artemis IV SRM: turned into a Weapon Addon for SRMs (deprecated)
        - Streak SRM: added inferno ammo, added streak effect, added Deadfire ammo
        - NARC: multiple pods
			- Homing Pod: attacking a narced unit has an +4 accuracy boost, better clustering, narc pod gets removed after 3 rounds, ecm blocks narc acc bonus
			- Explosive Pod: damge (kurita shops after 3059)
        - ATM: 3 ammo types, trading damage for range, added clustering
        - Infernos: Broken, use SRM inferno ammo instead
		- MRM: individual hit generator, unguided flag
		- RL: individual hit generator, unguided flag
		- iNarc: multiple pods
			- Homing Pod: attacking a narced unit has an +4 accuracy boost, better clustering, narc pod gets removed after 3 rounds, ecm blocks narc acc bonus
			- Explosive Pod: damge
			- Haywire Pod: -3 accuracy for 3 rounds
    - Artillery
        - Thumper: light artillery (replaces HM mortar / Bull Shark)
        - Sniper: medium artillery (found in mining shops) (tag compatible)
        - Long Tom: heavy artillery (found in mining shops, only mountable in Bull Shark)
        - Arrow IV: medium artillery (lostech / FP reward / Liao shops after 3049)
		- Artillery Loader: every artillery needs a Loader attached in the same or adjacent location (found in mining shops)
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
	- Artemis IV FCS: attaches to one SRM or LRM launcher and gives it: +4 direct fire acc, better clustering (can be turned off to fire special ammo or use tag/narc)
	- PPC Capacitor: attaches to one PPC (including ER, Heavy and Snub) and gives it: CAP Mode (+25 DMG, +15 Heat, 1 turn Weapon Cooldown)
- Argo Upgrades
	- Storage: added 3 additional storage upgrades, each giving a new mechbay to use
	- Engine Repairs: The storage upgrades do fix BiggerDrops tonnage increase upgrades, by giving you a way to fullfill their requirements
- Indirect Fire changes:
	- Shooting at something you cannot see (but your ally can) is considered Indirect Fire, and in turn will be blocked if the target is covered by ECM
	- You only get an accuracy penalty for Indirect Fire if you have to shoot over obstacles to hit your target (if it was considered Indirect Fire by the old rules)
- Mechs
	- Exterminator:
		- EXT-4C (stealth, SLDF) (ComStar)
	- Goliath:
		- GOL-1H (quad, SLDF/SW) (low chance everywhere, factory Oliver/Stewart)
		- GOL-3M (quad, HC) (Liao/Marik after 3047, factory Stewart)
		- GOL-3M2 (quad, HC) (Liao/Marik after 3052, factory Stewart)
	- Helepolis:
		- HEP-1H (sniper arty, SLDF) (very low chance Liao)
		- HEP-3H (sniper arty, SLDF) (ComStar/Snords)
	- Phoenix Hawk LAM:
		- PHX-HK1 (SLDF Royal LAM) (ComStar)
		- PHX-HK1R (SLDF Royal LAM) (WoB)
		- PHX-HK2 (SLDF LAM) (ComStar/Great Houses)
		- PHX-HK2M (SW LAM) (ComStar/Great Houses)
	- Scorpion:
		- SCP-1N (quad, SLDF/SW) (low chance everywhere, factory Oliver)
		- SCP-1O (quad, HC) (Kurita/Liao/Marik after 3049, factory Oliver)
	- Screamer LAM:
		- SCR-1X-LAM (Experimental LAM) (N/A)
	- Sirocco:
		- SRC-3C (quad, HC) (Liao/Marik/WoB after 3060, factory Stewart)
		- SRC-5C (quad, HC) (Marik/WoB after 3060, factory Stewart)
	- Stinger LAM:
		- STG-A1 (SLDF Royal LAM) (ComStar)
		- STG-A5 (SLDF LAM) (ComStar/Great Houses, factory Irece)
		- STG-A10 (SW LAM) (ComStar/Kurita, factory Irece)
	- Tarantula:
		- ZPH-1 (quad, HC) (low chance everywhere after 3054, factory Stewart)
		- ZPH-2A (quad, HC) (ComStar/Great Houses after 3060, factory Stewart)
	- UrbanMech LAM:
		- UM-LAM-X (Experimental LAM) (Kurita/Liao, Flying High Again mission)
	- Wasp LAM:
		- WSP-100 (SLDF LAM) (ComStar)
		- WSP-100b (SLDF Royal LAM) (ComStar)
		- WSP-105 (SLDF LAM) (ComStar/Great Houses)
		- WSP-105M (SW LAM) (Marik/WoB)

What to do when adding CAC-C into an existing savegame:
- Store and ready every mech that has any of the following fixed equipment:
	- any ECM (you are fine if the component has - X% Detectability as bonus)
	- any ActiveProbe (you are fine if the component has +Xm Sensor Range as bonus)
	- any TSM (you are fine if the in mission GUI shows only one active component for it)
	- MASC (you are fine if the in mission GUI shows only one active component for it)
	- Bull Shark BSK-MAZ (you are fine if it has a fixed Integrated Artillery Mount)
- Remove any equipment and use its new replacements (use the savegame editor to change the parts in your inventory if you want):
	- Liao ECM (Gear_Sensor_Prototype_ECM -> Gear_SensorCAC_LiaoProtoECM)
	- Liao AP (Gear_Sensor_Prototype_ActiveProbe -> Gear_SensorCAC_LiaoProtoAP)
	- AMS upgrade components (Gear_AMS_XXX -> Weapon_AMSCAC_XXX) to AMS weapons & MG ammo
	- Infernos Launcher (Weapon_Inferno_Inferno2_XXX -> Ammo_AmmunitionBox_Generic_SRM_Inferno) to standard SRMs with Inferno ammo
	- Deadfire LRMs/SRMs (Weapon_SRM_DFSRMX_0-STOCK -> Ammo_AmmunitionBox_Generic_SRM_DF)(Weapon_LRM_DFLRMX_0-STOCK -> Ammo_AmmunitionBox_Generic_LRM_DF) to standard LRMs/SRMs with deadfire ammo
	- Mech Mortar (Gear_Mortar_MechMortar -> Weapon_MortarCAC_ThumperFree) to Thumper with matching ammo (requires ballistics slot)
	- Artemis SRM/LRM (Weapon_SRM_ASRMXXX/Weapon_LRM_ALRMXXX ->  Gear_Addon_Artemis4) to a matching standard SRM/LRM launcher with Artemis IV FCS attachment

TODO BEX 2.0:
- weather acc modifiers?
- movable blockers
- CAC PRs
- component upgrader

TODO List:
- imp sensor unlocks sensorlock without skill (add to irtweaks)
- multitarget quirk unlocks without skill? (irtweaks stat)
- a-pods CAE?
- edit texts
- NARC reveals target?
- Flamers & Inferno ammo balance
- ECM tohit balance

Known bugs:
- CustomLoc patchig strings

Optional:
- i recommend having a look at BT_Extended_CE/mod.json settings and enable some of these in all modes
- If you want Urban vehicles to leave blood on destruction, look at CACs settings and change "DrawBloodChance" to 0.3
- manual deployment can be enabled via CU, set "DeployManual": to true to do so (warning: laggy)

Manual setup:
- clone git repo, including submodules
- correct paths in dll
- compile
Package for release:
- update version info in mod.json and dll
- setup additionalDependencies
	- SimpleMechAssembly
	- IRBModUtils
	- BiggerDrops
	- MechAffinity 1.4 (dll only)
	- CustomComponents (latest, Dec 28, 2023)
	- CAC & CAE repos
- compile CAC & CAE
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
- Kierk for adapting mechs, affinities, shops and xotl tables to 2.0
