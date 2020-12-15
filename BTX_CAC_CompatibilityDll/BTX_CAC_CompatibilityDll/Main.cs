using Harmony;
using HBS.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BTX_CAC_CompatibilityDll
{
    class Main
    {
        public static Settings Sett;
        public static ILog Log;

        public static void Init(string directory, string settingsJSON)
        {
            Log = Logger.GetLogger("BTX_CAC_Compatibility");
            try
            {
                Sett = JsonConvert.DeserializeObject<Settings>(settingsJSON);
            }
            catch (Exception e)
            {
                Sett = new Settings();
                Log.LogException(e);
            }
            if (Sett.LogLevelLog)
                Logger.SetLoggerLevel("BTX_CAC_Compatibility", LogLevel.Log);
            HarmonyInstance harmony = HarmonyInstance.Create("com.github.mcb5637.BTX_CAC_Compatibility");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
