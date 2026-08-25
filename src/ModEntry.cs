using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using GodotFileAccess = Godot.FileAccess;

namespace ChaosHeidemarie;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
    public static Logger Logger { get; private set; } = null!;
    private static Harmony? _harmony;

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        Logger = RitsuLibFramework.CreateLogger(ModInfo.Id);
        ModTypeDiscoveryHub.RegisterModAssembly(ModInfo.Id, assembly);

        var patcher = RitsuLibFramework.CreatePatcher(ModInfo.Id, "main");
        patcher.RegisterPatch<Patches.LocManagerSetLanguagePatch>();
        patcher.RegisterPatch<Patches.HeidemarieCharacterSelectAnimationLoopPatch>();
        patcher.RegisterPatch<Patches.HeidemarieCombatInitialIdleBootstrapPatch>();
        _harmony ??= new Harmony($"{ModInfo.Id}.runtime");
        _harmony.PatchAll(assembly);

        RitsuLibFramework.ApplyRequiredPatcher(patcher, DisableMod);
    }

    public static void RegisterLocalizationFallback(LocManager locManager)
    {
        if (locManager is null)
            return;

        foreach (var table in new[] { "characters", "cards", "relics", "card_keywords", "card_selection", "powers" })
        {
            MergeLocalizationTable(locManager, table, "eng");
            if (!string.Equals(locManager.Language, "eng", StringComparison.OrdinalIgnoreCase))
                MergeLocalizationTable(locManager, table, locManager.Language);
        }
    }

    private static void MergeLocalizationTable(LocManager locManager, string table, string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return;

        var path = $"res://ChaosHeidemarie/Localization/{table}/{language}.json";
        if (!GodotFileAccess.FileExists(path))
            return;

        try
        {
            using var file = GodotFileAccess.Open(path, GodotFileAccess.ModeFlags.Read);
            if (file is null)
                return;

            var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(file.GetAsText());
            if (entries is null || entries.Count == 0)
                return;

            locManager.GetTable(table).MergeWith(entries);
            Logger.Info($"[Localization] Merged {entries.Count} fallback entries from {language}/{table}.json.");
        }
        catch (Exception ex)
        {
            Logger.Warn($"[Localization] Failed to merge fallback table {language}/{table}.json: {ex.Message}");
        }
    }

    private static void DisableMod()
    {
        Logger.Error("Chaos Heidemarie disabled because a required RitsuLib patch could not be applied.");
    }
}
