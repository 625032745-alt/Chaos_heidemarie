using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using TestTheSpire;

namespace ChaosHeidemarie.Tests;

[ModInitializer(nameof(Init))]
public static class Entry
{
    public static void Init()
    {
        CombatTestBootstrap.Initialize(Assembly.GetExecutingAssembly(), new CombatTestOptions
        {
            LogPrefix = "ChaosHeidemarie.Tests"
        });

        Log.Info("[ChaosHeidemarie.Tests] Mod initialized");
    }
}
