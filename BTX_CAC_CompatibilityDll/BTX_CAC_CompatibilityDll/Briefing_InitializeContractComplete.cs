using BattleTech;
using BattleTech.UI;
using Harmony;

namespace BTX_CAC_CompatibilityDll
{
    [HarmonyPatch(typeof(Briefing), "InitializeContractComplete")]
    public class Briefing_InitializeContractComplete
    {
        public static void Prefix()
        {
            var o = UnityGameInstance.BattleTechGame.SaveManager.GameInstanceSaves;
            if (o.SaveFailedPopupVisible)
                Traverse.Create(o).Property("SaveFailedPopupVisible").SetValue(false);
        }
    }

}
