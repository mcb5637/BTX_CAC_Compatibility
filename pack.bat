del ".\BTX_CAC_Compatibility.zip"
"C:\Program Files\7-Zip\7z.exe" a ".\BTX_CAC_Compatibility.zip" ".\additionalDependencies\*"
"C:\Program Files\7-Zip\7z.exe" a ".\BTX_CAC_Compatibility.zip" ".\BTX_CAC_Compatibility\" ".\Abilifier\" ".\BEX_Affinities\" ".\BiggerDrops\" ".\BT_Extended_Timeline\" ".\FullXotlTables\" ".\MechAffinity\" ".\LICENSE" ".\README.md"
cd .\CBD\Core
"C:\Program Files\7-Zip\7z.exe" a "..\..\BTX_CAC_Compatibility.zip" ".\CustomActivatableEquipment\" ".\CustomAmmoCategories\" ".\CustomComponents\" ".\CustomLocalization\" ".\CustomUnits\" ".\CustomVoices\" ".\CustomLocalSettings\" ".\CustomPrewarm\"
cd ..\..\
cd .\CBD
"C:\Program Files\7-Zip\7z.exe" a "..\BTX_CAC_Compatibility.zip" "ModTek\Injectors\"
cd ..\
"C:\Program Files\7-Zip\7z.exe" d ".\BTX_CAC_Compatibility.zip" "CustomAmmoCategories\StreamingAssets"
"C:\Program Files\7-Zip\7z.exe" d ".\BTX_CAC_Compatibility.zip" "ModTek\Injectors\RogueTechPerfFixesInjector.dll"
"C:\Program Files\7-Zip\7z.exe" a ".\BTX_CAC_Compatibility.zip" ".\CustomActivatableEquipment\" ".\CustomAmmoCategories\" ".\CustomComponents\" ".\CustomLocalization\" ".\CustomUnits\" ".\RaidFix\"

pause