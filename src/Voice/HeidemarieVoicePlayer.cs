using System;
using System.Collections.Generic;
using ChaosHeidemarie.Settings;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ChaosHeidemarie.Voice;

internal static class HeidemarieVoicePlayer
{
    private static readonly StringName SfxBus = new("SFX");
    private static readonly Dictionary<string, AudioStream> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, long> MissingVoiceNextLogAt = new(StringComparer.OrdinalIgnoreCase);

    private const float VolumeMultiplier = 4f;
    private const int MissingVoiceLogIntervalMs = 60000;

    private static AudioStreamPlayer? _player;
    private static Node? _attachedTo;
    private static Tween? _fadeTween;

    public static void Prewarm(IReadOnlyList<string> resPaths)
    {
        if (resPaths.Count == 0)
            return;

        EnsurePlayer();
        for (int i = 0; i < resPaths.Count; i++)
            _ = GetStream(resPaths[i]);
    }

    public static void Play(string resPath, float volume = 1f)
    {
        AudioStream? stream = GetStream(resPath);
        if (stream == null)
            return;

        AudioStreamPlayer? player = EnsurePlayer();
        if (player == null)
            return;

        CancelFadeTween();
        player.Stop();
        player.Stream = stream;
        player.Bus = SfxBus;
        player.VolumeLinear = volume * VolumeMultiplier * ChaosModSharedSettings.VoiceVolume;
        player.Play();
    }

    public static void PlayRandom(IReadOnlyList<string> resPaths, int seed = -1, float volume = 1f)
    {
        if (resPaths.Count == 0)
            return;

        int index = seed < 0 ? Random.Shared.Next(resPaths.Count) : Math.Abs(seed) % resPaths.Count;
        Play(resPaths[index], volume);
    }

    public static void FadeOutForCharacterSwitch(float durationSeconds = 0.3f)
    {
        AudioStreamPlayer? player = EnsurePlayer();
        if (player == null || !player.Playing)
            return;

        CancelFadeTween();
        if (durationSeconds <= 0.0001f)
        {
            player.Stop();
            return;
        }

        Tween tween = player.CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(player, "volume_db", -80.0, durationSeconds);
        tween.TweenCallback(Callable.From(player.Stop));
        _fadeTween = tween;
    }

    private static AudioStreamPlayer? EnsurePlayer()
    {
        Node? parent = null;
        NCombatRoom? room = NCombatRoom.Instance;
        if (room?.Ui != null)
            parent = room.Ui;
        if (parent == null && room != null)
            parent = room;
        if (parent == null)
            parent = NGame.Instance;
        if (parent == null)
            parent = ((SceneTree)Engine.GetMainLoop()).Root;
        if (parent == null)
            return null;

        if (_player == null || !GodotObject.IsInstanceValid(_player))
            _player = new AudioStreamPlayer();

        Node? currentParent = _player.GetParent();
        if (currentParent != parent || _attachedTo == null || !GodotObject.IsInstanceValid(_attachedTo) || _attachedTo != parent)
        {
            if (currentParent != null)
            {
                try
                {
                    currentParent.RemoveChild(_player);
                }
                catch
                {
                }
            }

            parent.AddChild(_player);
            _attachedTo = parent;
        }

        return _player;
    }

    private static void CancelFadeTween()
    {
        if (_fadeTween == null || !GodotObject.IsInstanceValid(_fadeTween))
            return;

        try
        {
            _fadeTween.Kill();
        }
        catch
        {
        }
        finally
        {
            _fadeTween = null;
        }
    }

    private static AudioStream? GetStream(string resPath)
    {
        if (Cache.TryGetValue(resPath, out AudioStream? cached))
            return cached;

        try
        {
            if (!ResourceLoader.Exists(resPath))
            {
                LogMissingVoiceAsset(resPath, null);
                return null;
            }

            AudioStream? loaded = ResourceLoader.Load<AudioStream>(resPath);
            if (loaded == null)
            {
                LogMissingVoiceAsset(resPath, null);
                return null;
            }

            Cache[resPath] = loaded;
            return loaded;
        }
        catch (Exception ex)
        {
            LogMissingVoiceAsset(resPath, ex.Message);
            return null;
        }
    }

    private static void LogMissingVoiceAsset(string resPath, string? error)
    {
        long now = System.Environment.TickCount64;
        if (MissingVoiceNextLogAt.TryGetValue(resPath, out long nextAt) && now < nextAt)
            return;

        MissingVoiceNextLogAt[resPath] = now + MissingVoiceLogIntervalMs;
        if (string.IsNullOrWhiteSpace(error))
        {
            ModEntry.Logger.Warn("[Voice] Missing voice asset: " + resPath);
            return;
        }

        ModEntry.Logger.Warn("[Voice] Voice asset load failed: " + resPath + " err=" + error);
    }
}
