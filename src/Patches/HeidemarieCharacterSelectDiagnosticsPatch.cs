using System;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace ChaosHeidemarie.Patches;

[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))]
internal static class HeidemarieCharacterSelectDiagnosticsPatch
{
    [HarmonyFinalizer]
    private static Exception? Finalizer(
        Exception? __exception,
        NCharacterSelectScreen __instance,
        NCharacterSelectButton charSelectButton,
        CharacterModel characterModel)
    {
        if (__exception == null || !HeidemarieAnimationPatchHelper.IsTarget(characterModel))
            return __exception;

        try
        {
            var method = AccessTools.DeclaredMethod(
                typeof(NCharacterSelectScreen),
                nameof(NCharacterSelectScreen.SelectCharacter),
                [typeof(NCharacterSelectButton), typeof(CharacterModel)]);
            var patchInfo = method == null ? null : Harmony.GetPatchInfo(method);
            var owners = patchInfo == null
                ? "<none>"
                : string.Join(",",
                    patchInfo.Owners
                        .Where(static owner => !string.IsNullOrWhiteSpace(owner))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(static owner => owner, StringComparer.Ordinal));

            var buttonIndex = charSelectButton?.GetIndex() ?? -1;
            var buttonName = charSelectButton?.Name ?? "<null>";

            string bgPath = "<n/a>";
            string bgExists = "<n/a>";
            string iconPath = "<n/a>";
            string iconExists = "<n/a>";
            string lockedIconPath = "<n/a>";
            string lockedIconExists = "<n/a>";
            string energyPath = "<n/a>";
            string energyExists = "<n/a>";

            if (characterModel is Characters.Heidemarie heidemarie)
            {
                bgPath = heidemarie.AssetProfile.Ui?.CharacterSelectBgPath ?? "<null>";
                bgExists = ResourceLoader.Exists(bgPath).ToString();
                iconPath = heidemarie.AssetProfile.Ui?.CharacterSelectIconPath ?? "<null>";
                iconExists = ResourceLoader.Exists(iconPath).ToString();
                lockedIconPath = heidemarie.AssetProfile.Ui?.CharacterSelectLockedIconPath ?? "<null>";
                lockedIconExists = ResourceLoader.Exists(lockedIconPath).ToString();
                energyPath = heidemarie.AssetProfile.Scenes?.EnergyCounterPath ?? "<null>";
                energyExists = ResourceLoader.Exists(energyPath).ToString();
            }

            ModEntry.Logger.Error(
                $"[CharacterSelect] SelectCharacter failed for Heidemarie: ex={__exception.GetType().Name}: {__exception.Message}; " +
                $"button={buttonName}; buttonIndex={buttonIndex}; owners={owners}; " +
                $"bg={bgPath}; bgExists={bgExists}; icon={iconPath}; iconExists={iconExists}; " +
                $"lockedIcon={lockedIconPath}; lockedIconExists={lockedIconExists}; energy={energyPath}; energyExists={energyExists}");
        }
        catch (Exception logEx)
        {
            ModEntry.Logger.Error($"[CharacterSelect] Diagnostics finalizer failed: {logEx.GetType().Name}: {logEx.Message}");
        }

        return __exception;
    }
}
