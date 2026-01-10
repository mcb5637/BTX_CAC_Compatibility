#!/usr/bin/bash

rm "./BTX_CAC_Compatibility.zip"
7z a "./BTX_CAC_Compatibility.zip" "./additionalDependencies/*"
7z a "./BTX_CAC_Compatibility.zip" "./BTX_CAC_Compatibility/" "./BiggerDrops/" "./BT_Extended_Timeline/" "./BT_Extended/" "./FullXotlTables/" "./IRTweaks/" "./MechAffinity/" "./MissionControl/" "./BEX_Affinities/" "./LICENSE" "./README.md"
cd ./CBD/Core
7z a "../../BTX_CAC_Compatibility.zip" "./CustomActivatableEquipment/" "./CustomAmmoCategories/" "./CustomComponents/" "./CustomLocalization/" "./CustomUnits/" "./CustomVoices/" "./CustomLocalSettings/" "./CustomPrewarm/"
cd ../../
cd ./CBD
7z a "../BTX_CAC_Compatibility.zip" "ModTek/Injectors/"
cd ../
7z d "./BTX_CAC_Compatibility.zip" "CustomAmmoCategories/StreamingAssets"
7z d "./BTX_CAC_Compatibility.zip" "ModTek/Injectors/RogueTechPerfFixesInjector.dll"
7z a "./BTX_CAC_Compatibility.zip" "./CustomActivatableEquipment/" "./CustomAmmoCategories/" "./CustomComponents/" "./CustomLocalization/" "./CustomUnits/" "./RaidFix/" "./Abilifier/"
