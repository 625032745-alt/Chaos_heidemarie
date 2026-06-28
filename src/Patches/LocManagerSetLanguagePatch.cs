using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Patching.Models;

namespace ChaosHeidemarie.Patches;

internal sealed class LocManagerSetLanguagePatch : IPatchMethod
{
    public static string PatchId => "chaos_heidemarie_merge_localization_after_set_language";

    public static string Description => "Merge Chaos Heidemarie localization after the base game tables are loaded";

    public static bool IsCritical => true;

    public static ModPatchTarget[] GetTargets()
    {
        return
        [
            new(typeof(LocManager), nameof(LocManager.SetLanguage), [typeof(string)]),
        ];
    }

    public static void Postfix(LocManager __instance)
    {
        ModEntry.RegisterLocalizationFallback(__instance);
    }
}
