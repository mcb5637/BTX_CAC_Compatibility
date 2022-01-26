del ".\BTX_CAC_Compatibility.zip"
"C:\Program Files\7-Zip\7z.exe" a ".\BTX_CAC_Compatibility.zip" ".\BTX_CAC_Compatibility\" ".\BiggerDrops\" ".\BT_Extended_Timeline\" ".\FullXotlTables\" ".\IRTweaks\" ".\MechAffinity\" ".\MissionControl\" ".\LICENSE" ".\README.md"
cd .\CB
"C:\Program Files\7-Zip\7z.exe" a "..\BTX_CAC_Compatibility.zip" ".\CustomActivatableEquipment\" ".\CustomAmmoCategories\" ".\CustomComponents\" ".\CustomLocalization\" ".\CustomUnits\"
cd ..\
"C:\Program Files\7-Zip\7z.exe" d ".\BTX_CAC_Compatibility.zip" "CustomAmmoCategories\StreamingAssets"
"C:\Program Files\7-Zip\7z.exe" a ".\BTX_CAC_Compatibility.zip" ".\CustomActivatableEquipment\" ".\CustomAmmoCategories\" ".\CustomComponents\" ".\CustomLocalization\" ".\CustomUnits\"

pause