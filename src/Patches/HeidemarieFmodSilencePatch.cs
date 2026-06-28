using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace ChaosHeidemarie.Patches;

internal static class HeidemarieFmodSilenceHelper
{
    private const string CharacterAudioPrefix =
        "event:/sfx/characters/chaos_heidemarie_character_heidemarie/chaos_heidemarie_character_heidemarie_";
    private const string CharacterTransitionSfx = "event:/sfx/ui/wipe_chaos_heidemarie_character_heidemarie";

    public static bool ShouldSuppress(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (string.Equals(path, CharacterTransitionSfx, StringComparison.OrdinalIgnoreCase))
            return true;

        return string.Equals(path, CharacterAudioPrefix + "select", StringComparison.OrdinalIgnoreCase)
               || string.Equals(path, CharacterAudioPrefix + "attack", StringComparison.OrdinalIgnoreCase)
               || string.Equals(path, CharacterAudioPrefix + "cast", StringComparison.OrdinalIgnoreCase)
               || string.Equals(path, CharacterAudioPrefix + "die", StringComparison.OrdinalIgnoreCase);
    }
}

[HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.PlayOneShot), [typeof(string), typeof(float)])]
internal static class HeidemarieFmodSilenceSinglePatch
{
    [HarmonyPrefix]
    public static bool Prefix(string path)
    {
        return !HeidemarieFmodSilenceHelper.ShouldSuppress(path);
    }
}

[HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.PlayOneShot),
    [typeof(string), typeof(Dictionary<string, float>), typeof(float)])]
internal static class HeidemarieFmodSilenceParameterizedPatch
{
    [HarmonyPrefix]
    public static bool Prefix(string path)
    {
        return !HeidemarieFmodSilenceHelper.ShouldSuppress(path);
    }
}
