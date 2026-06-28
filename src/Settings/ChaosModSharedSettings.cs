using System;
using System.IO;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace ChaosHeidemarie.Settings;

internal static class ChaosModSharedSettings
{
    private const string SharedSettingsDirName = "chaosmod";
    private const string SharedSettingsFileName = "xcskin_settings.json";
    private const string SharedDomainKeyPrefix = "CHAOSMOD_XCSKIN_";

    private static readonly string SharedVoiceVolumeKey = SharedDomainKeyPrefix + "VOICE_VOLUME";
    private static readonly string SharedBattleReadyScaleKey = SharedDomainKeyPrefix + "BATTLE_READY_SCALE";
    private static readonly string SharedBattleReadyOffsetXKey = SharedDomainKeyPrefix + "BATTLE_READY_OFFSET_X";
    private static readonly string SharedBattleReadyOffsetYKey = SharedDomainKeyPrefix + "BATTLE_READY_OFFSET_Y";
    private static readonly string SharedPortraitsEnabledKey = SharedDomainKeyPrefix + "PORTRAITS_ENABLED";

    private static int _settingsLoaded;
    private static float _voiceVolume = 0.8f;
    private static float _battleReadyScale = 1f;
    private static float _battleReadyOffsetX;
    private static float _battleReadyOffsetY;
    private static bool _portraitsEnabled = true;

    public static float VoiceVolume
    {
        get
        {
            EnsureSettingsLoaded();
            return GetSharedFloat(SharedVoiceVolumeKey, _voiceVolume);
        }
    }

    public static float BattleReadyScale
    {
        get
        {
            EnsureSettingsLoaded();
            return GetSharedFloat(SharedBattleReadyScaleKey, _battleReadyScale);
        }
    }

    public static float BattleReadyOffsetX
    {
        get
        {
            EnsureSettingsLoaded();
            return GetSharedFloat(SharedBattleReadyOffsetXKey, _battleReadyOffsetX);
        }
    }

    public static float BattleReadyOffsetY
    {
        get
        {
            EnsureSettingsLoaded();
            return GetSharedFloat(SharedBattleReadyOffsetYKey, _battleReadyOffsetY);
        }
    }

    public static bool PortraitsEnabled
    {
        get
        {
            EnsureSettingsLoaded();
            return GetSharedBool(SharedPortraitsEnabledKey, _portraitsEnabled);
        }
    }

    public static void SetBattleReadyScale(float value, bool persist)
    {
        EnsureSettingsLoaded();
        _battleReadyScale = Mathf.Clamp(value, 0.5f, 2.0f);
        SetSharedFloat(SharedBattleReadyScaleKey, _battleReadyScale);
        if (persist)
            TrySaveSettings();
    }

    public static void SetVoiceVolume(float value, bool persist)
    {
        EnsureSettingsLoaded();
        _voiceVolume = Mathf.Clamp(value, 0f, 1f);
        SetSharedFloat(SharedVoiceVolumeKey, _voiceVolume);
        if (persist)
            TrySaveSettings();
    }

    public static void SetBattleReadyOffsetX(float value, bool persist)
    {
        EnsureSettingsLoaded();
        _battleReadyOffsetX = Mathf.Clamp(value, -400f, 400f);
        SetSharedFloat(SharedBattleReadyOffsetXKey, _battleReadyOffsetX);
        if (persist)
            TrySaveSettings();
    }

    public static void SetBattleReadyOffsetY(float value, bool persist)
    {
        EnsureSettingsLoaded();
        _battleReadyOffsetY = Mathf.Clamp(value, -400f, 400f);
        SetSharedFloat(SharedBattleReadyOffsetYKey, _battleReadyOffsetY);
        if (persist)
            TrySaveSettings();
    }

    public static void SetPortraitsEnabled(bool value, bool persist)
    {
        EnsureSettingsLoaded();
        _portraitsEnabled = value;
        SetSharedBool(SharedPortraitsEnabledKey, _portraitsEnabled);
        if (persist)
            TrySaveSettings();
    }

    private static float GetSharedFloat(string key, float fallback)
    {
        object? value = AppDomain.CurrentDomain.GetData(key);
        return value switch
        {
            float f => f,
            double d => (float)d,
            int i => i,
            _ => fallback
        };
    }

    private static void SetSharedFloat(string key, float value)
    {
        try
        {
            AppDomain.CurrentDomain.SetData(key, value);
        }
        catch
        {
        }
    }

    private static bool GetSharedBool(string key, bool fallback)
    {
        object? value = AppDomain.CurrentDomain.GetData(key);
        return value switch
        {
            bool b => b,
            int i => i != 0,
            string s when bool.TryParse(s, out bool parsed) => parsed,
            _ => fallback
        };
    }

    private static void SetSharedBool(string key, bool value)
    {
        try
        {
            AppDomain.CurrentDomain.SetData(key, value);
        }
        catch
        {
        }
    }

    private static void EnsureSettingsLoaded()
    {
        if (System.Threading.Interlocked.Exchange(ref _settingsLoaded, 1) != 0)
            return;

        try
        {
            string path = GetSettingsPath();
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                SharedSettingsModel? settings = JsonSerializer.Deserialize<SharedSettingsModel>(json);
                if (settings != null)
                {
                    _voiceVolume = Mathf.Clamp(settings.Volume, 0f, 1f);
                    _battleReadyScale = Mathf.Clamp(settings.BattleReadyScale, 0.5f, 2.0f);
                    _battleReadyOffsetX = Mathf.Clamp(settings.BattleReadyOffsetX, -400f, 400f);
                    _battleReadyOffsetY = Mathf.Clamp(settings.BattleReadyOffsetY, -400f, 400f);
                    _portraitsEnabled = settings.PortraitsEnabled;
                }
            }

            SetSharedFloat(SharedVoiceVolumeKey, _voiceVolume);
            SetSharedFloat(SharedBattleReadyScaleKey, _battleReadyScale);
            SetSharedFloat(SharedBattleReadyOffsetXKey, _battleReadyOffsetX);
            SetSharedFloat(SharedBattleReadyOffsetYKey, _battleReadyOffsetY);
            SetSharedBool(SharedPortraitsEnabledKey, _portraitsEnabled);
        }
        catch (Exception ex)
        {
            Log.Warn("[ChaosHeidemarie] Shared settings load failed: " + ex.Message);
            _voiceVolume = 0.8f;
            _battleReadyScale = 1f;
            _battleReadyOffsetX = 0f;
            _battleReadyOffsetY = 0f;
            _portraitsEnabled = true;

            SetSharedFloat(SharedVoiceVolumeKey, _voiceVolume);
            SetSharedFloat(SharedBattleReadyScaleKey, _battleReadyScale);
            SetSharedFloat(SharedBattleReadyOffsetXKey, _battleReadyOffsetX);
            SetSharedFloat(SharedBattleReadyOffsetYKey, _battleReadyOffsetY);
            SetSharedBool(SharedPortraitsEnabledKey, _portraitsEnabled);
        }
    }

    private static void TrySaveSettings()
    {
        try
        {
            string path = GetSettingsPath();
            string? dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            SharedSettingsModel settings = new()
            {
                Volume = GetSharedFloat(SharedVoiceVolumeKey, _voiceVolume),
                BattleReadyScale = GetSharedFloat(SharedBattleReadyScaleKey, _battleReadyScale),
                BattleReadyOffsetX = GetSharedFloat(SharedBattleReadyOffsetXKey, _battleReadyOffsetX),
                BattleReadyOffsetY = GetSharedFloat(SharedBattleReadyOffsetYKey, _battleReadyOffsetY),
                PortraitsEnabled = GetSharedBool(SharedPortraitsEnabledKey, _portraitsEnabled)
            };

            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Log.Warn("[ChaosHeidemarie] Shared settings save failed: " + ex.Message);
        }
    }

    private static string GetSettingsPath()
    {
        string baseDir = string.Empty;
        try
        {
            baseDir = OS.GetUserDataDir();
        }
        catch
        {
        }

        if (string.IsNullOrWhiteSpace(baseDir))
        {
            try
            {
                baseDir = ProjectSettings.GlobalizePath("user://");
            }
            catch
            {
            }
        }

        if (string.IsNullOrWhiteSpace(baseDir))
        {
            try
            {
                baseDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            }
            catch
            {
            }
        }

        if (!string.IsNullOrWhiteSpace(baseDir) &&
            baseDir.Contains(".app/Contents/", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                string fallback = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrWhiteSpace(fallback))
                    baseDir = fallback;
            }
            catch
            {
            }
        }

        if (string.IsNullOrWhiteSpace(baseDir))
            baseDir = AppContext.BaseDirectory;

        return Path.Combine(baseDir, SharedSettingsDirName, SharedSettingsFileName);
    }

    private sealed class SharedSettingsModel
    {
        public float Volume { get; set; } = 0.8f;
        public float BattleReadyScale { get; set; } = 1f;
        public float BattleReadyOffsetX { get; set; }
        public float BattleReadyOffsetY { get; set; }
        public bool PortraitsEnabled { get; set; } = true;
    }
}
