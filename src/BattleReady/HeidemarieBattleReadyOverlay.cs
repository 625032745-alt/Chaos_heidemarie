using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChaosHeidemarie.Settings;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ChaosHeidemarie.BattleReady;

internal static class HeidemarieBattleReadyOverlay
{
    private const float OutDelaySeconds = 0.3f;
    private const float CancelOutDelaySeconds = 0.8f;
    private const string AnimIn = "b_in";
    private const string AnimIdle = "b_idle";
    private const string AnimOut = "b_out";
    private const string AnimCardAttack = "card_attack";
    private static readonly string[] AnimCardNonAttackCandidates = ["card_casting"];

    private static PackedScene? _cachedScene;
    private static bool _sceneLoadAttempted;
    private static bool _sceneMissingWarned;
    private static Resource? _cachedSkeleton;
    private static bool _skeletonLoadAttempted;

    private static Node? _node;
    private static MegaSprite? _sprite;
    private static Vector2 _basePos;
    private static Vector2 _baseScale = Vector2.One;
    private static bool _baseCaptured;
    private static bool _busy;
    private static bool _isHovered;
    private static bool _isUiFocused;
    private static ulong _focusToken;
    private static bool _outScheduled;
    private static bool _outPlaying;
    private static bool _cardUsePlaying;

    private static bool _hasAnimIn;
    private static bool _hasAnimIdle;
    private static bool _hasAnimOut;
    private static bool _hasCardAttack;
    private static string? _cardNonAttackAnim;
    private static string? _lastFirst;
    private static string? _lastNextLoop;

    private static readonly HashSet<string> MissingAnimsWarned = new(StringComparer.Ordinal);
    private static ulong _watchToken;
    private static long _createDisabledUntil;
    private static int _createErrorLogged;
    private const int CreateDisableMs = 30000;

    private static bool IsFocused => _isHovered || _isUiFocused;
    private static bool IsFocusedEffective => IsFocused || _outScheduled;

    public static void Preload()
    {
        if (!ChaosModSharedSettings.PortraitsEnabled)
            return;

        _ = GetScene();
        _ = GetSkeleton();
    }

    public static void RefreshPortraitState()
    {
        if (!ChaosModSharedSettings.PortraitsEnabled)
            NotifyCombatEnded();
    }

    public static void NotifyCombatEnded()
    {
        _isHovered = false;
        _isUiFocused = false;
        _outScheduled = false;
        Cleanup();
    }

    public static void ApplyTransformFromSettings()
    {
        Node? node = _node;
        if (node == null || !GodotObject.IsInstanceValid(node))
            return;

        try
        {
            ApplyTransform(node);
        }
        catch
        {
        }
    }

    public static void NotifyHovered(CardModel card, bool hovered)
    {
        if (!ChaosModSharedSettings.PortraitsEnabled)
        {
            NotifyCombatEnded();
            return;
        }

        if (!HeidemarieBattleReadyTarget.IsTarget(card.Owner?.Character))
            return;

        bool wasFocused = IsFocusedEffective;
        _isHovered = hovered;
        _focusToken++;

        if (hovered)
        {
            _outScheduled = false;
            if (!_busy)
            {
                EnsureCreated(playIntro: true);
                return;
            }

            if (wasFocused || _outPlaying || _cardUsePlaying)
                return;

            PlaySequence(AnimIn, AnimIdle);
            return;
        }

        if (IsFocused)
            return;

        _outScheduled = true;
        ulong token = _focusToken;
        TaskHelper.RunSafely(DelayedOutIfStillUnfocused(token, OutDelaySeconds));
    }

    public static void NotifyUiFocused(CardModel card, bool focused)
    {
        if (!ChaosModSharedSettings.PortraitsEnabled)
        {
            NotifyCombatEnded();
            return;
        }

        if (!HeidemarieBattleReadyTarget.IsTarget(card.Owner?.Character))
            return;

        bool wasFocused = IsFocusedEffective;
        _isUiFocused = focused;
        _focusToken++;

        if (focused)
        {
            _outScheduled = false;
            if (!_busy)
            {
                EnsureCreated(playIntro: true);
                return;
            }

            if (wasFocused || _outPlaying || _cardUsePlaying)
                return;

            PlaySequence(AnimIn, AnimIdle);
            return;
        }

        if (IsFocused)
            return;

        _outScheduled = true;
        ulong token = _focusToken;
        TaskHelper.RunSafely(DelayedOutIfStillUnfocused(token, OutDelaySeconds));
    }

    public static void NotifyBeforeCardPlayed(CardPlay cardPlay)
    {
        CardModel? card = cardPlay.Card;
        if (card == null || !HeidemarieBattleReadyTarget.IsTarget(card.Owner?.Character))
            return;

        _focusToken++;
        _isHovered = false;
        _outScheduled = false;
        EnsureCreated(playIntro: false);

        string? anim = GetCardUseAnim(card);
        if (anim == null)
            return;

        _outPlaying = false;
        _cardUsePlaying = true;
        if (!PlaySingle(anim))
        {
            _cardUsePlaying = false;
            if (IsFocused)
                PlaySequence(AnimIdle, AnimIdle);
            else
                StartOut();
        }
    }

    public static void NotifyCanceled(CardModel card)
    {
        if (!HeidemarieBattleReadyTarget.IsTarget(card.Owner?.Character) || !_busy)
            return;

        _isHovered = false;
        _isUiFocused = false;
        _outScheduled = true;
        ulong token = ++_focusToken;
        if (_cardUsePlaying || _outPlaying)
            return;

        TaskHelper.RunSafely(DelayedOutIfStillUnfocused(token, CancelOutDelaySeconds));
    }

    private static PackedScene? GetScene()
    {
        if (_cachedScene != null)
            return _cachedScene;
        if (_sceneLoadAttempted)
            return null;

        _sceneLoadAttempted = true;
        _cachedScene = ResourceLoader.Load<PackedScene>(HeidemarieBattleReadyProfile.BattleReadyScenePath);
        return _cachedScene;
    }

    private static Resource? GetSkeleton()
    {
        if (_cachedSkeleton != null)
            return _cachedSkeleton;
        if (_skeletonLoadAttempted)
            return null;

        _skeletonLoadAttempted = true;
        string? selected = null;
        if (ResourceLoader.Exists(HeidemarieBattleReadyProfile.BattleReadySkeletonDataPath))
            selected = HeidemarieBattleReadyProfile.BattleReadySkeletonDataPath;
        else if (ResourceLoader.Exists(HeidemarieBattleReadyProfile.BattleReadySkeletonDataFallbackPath))
            selected = HeidemarieBattleReadyProfile.BattleReadySkeletonDataFallbackPath;

        if (selected == null)
            return null;

        _cachedSkeleton = ResourceLoader.Load<Resource>(selected);
        return _cachedSkeleton;
    }

    private static void EnsureCreated(bool playIntro)
    {
        if (!ChaosModSharedSettings.PortraitsEnabled)
        {
            Cleanup();
            return;
        }

        long now = System.Environment.TickCount64;
        if (now < _createDisabledUntil)
            return;

        if (_node != null && GodotObject.IsInstanceValid(_node) && _sprite != null)
            return;

        try
        {
            Cleanup();

            NCombatRoom? room = NCombatRoom.Instance;
            if (room == null)
                return;

            PackedScene? scene = GetScene();
            if (scene == null)
            {
                if (!_sceneMissingWarned)
                {
                    _sceneMissingWarned = true;
                    Log.Warn("[ChaosHeidemarie] Missing battle-ready scene: " +
                             HeidemarieBattleReadyProfile.BattleReadyScenePath);
                }
                return;
            }

            Node instance = scene.Instantiate();
            ApplyBattleReadySkeletonIfPresent(instance);
            CaptureBaseTransform(instance);
            ApplyTransform(instance);
            _node = instance;
            _sprite = new MegaSprite(instance);
            InitAnimCache(_sprite);
            _busy = true;
            _outPlaying = false;
            _cardUsePlaying = false;

            ulong watchToken = ++_watchToken;
            TaskHelper.RunSafely(IdleWatchLoop(instance, watchToken));

            _sprite.ConnectAnimationCompleted(Callable.From<GodotObject, GodotObject, GodotObject>((_, _, _) =>
            {
                if (_node != instance)
                    return;

                if (_cardUsePlaying)
                {
                    _cardUsePlaying = false;
                    if (IsFocused)
                        PlaySequence(AnimIdle, AnimIdle);
                    else
                        StartOut();
                    return;
                }

                if (_outPlaying)
                {
                    _outPlaying = false;
                    if (IsFocused)
                        PlaySequence(AnimIn, AnimIdle);
                    else
                        Cleanup();
                }

                if (string.Equals(_lastFirst, AnimIn, StringComparison.Ordinal) &&
                    string.Equals(_lastNextLoop, AnimIdle, StringComparison.Ordinal))
                {
                    _lastFirst = AnimIdle;
                    _lastNextLoop = null;
                }
            }));

            room.CombatVfxContainer.AddChildSafely(instance);
            if (instance is CanvasItem item)
                item.ZIndex = 0;

            if (playIntro)
                PlaySequence(AnimIn, AnimIdle);
            else
                PlaySequence(AnimIdle, AnimIdle);
        }
        catch (Exception ex)
        {
            _createDisabledUntil = System.Environment.TickCount64 + CreateDisableMs;
            if (System.Threading.Interlocked.Exchange(ref _createErrorLogged, 1) == 0)
                Log.Warn("[ChaosHeidemarie] Battle-ready overlay create failed: " + ex);
            Cleanup();
        }
    }

    private static void CaptureBaseTransform(Node instance)
    {
        if (_baseCaptured)
            return;

        switch (instance)
        {
            case Node2D node2D:
                _baseCaptured = true;
                _basePos = node2D.Position;
                _baseScale = node2D.Scale;
                break;
            case Control control:
                _baseCaptured = true;
                _basePos = control.Position;
                _baseScale = control.Scale;
                break;
        }
    }

    private static void ApplyTransform(Node instance)
    {
        float scale = ChaosModSharedSettings.BattleReadyScale;
        float offsetX = ChaosModSharedSettings.BattleReadyOffsetX;
        float offsetY = ChaosModSharedSettings.BattleReadyOffsetY;

        switch (instance)
        {
            case Node2D node2D:
                node2D.Scale = _baseScale * new Vector2(scale, scale);
                node2D.Position = _basePos + new Vector2(offsetX, -offsetY);
                break;
            case Control control:
                control.Scale = _baseScale * new Vector2(scale, scale);
                control.Position = _basePos + new Vector2(offsetX, -offsetY);
                break;
        }
    }

    private static void ApplyBattleReadySkeletonIfPresent(Node instance)
    {
        Resource? skeleton = GetSkeleton();
        if (skeleton != null)
            instance.Set("skeleton_data_res", skeleton);
    }

    private static async Task DelayedOutIfStillUnfocused(ulong token, float delaySeconds)
    {
        await WaitSeconds(delaySeconds);
        if (token != _focusToken || IsFocused || !_busy)
            return;

        _outScheduled = false;
        if (_cardUsePlaying)
            return;

        StartOut();
    }

    private static async Task IdleWatchLoop(Node instance, ulong watchToken)
    {
        while (watchToken == _watchToken)
        {
            await WaitSeconds(1f);
            if (watchToken != _watchToken)
                return;
            if (!_busy || _node != instance || !GodotObject.IsInstanceValid(instance) || _sprite == null)
                return;
            if (_cardUsePlaying || _outPlaying || _outScheduled || IsFocused)
                continue;
            if (!string.Equals(_lastFirst, AnimIdle, StringComparison.Ordinal) || _lastNextLoop != null)
                continue;

            StartOut();
        }
    }

    private static async Task WaitSeconds(float seconds)
    {
        if (seconds <= 0f)
            return;

        try
        {
            NCombatRoom? room = NCombatRoom.Instance;
            SceneTree? tree = room?.GetTree();
            if (room != null && tree != null)
            {
                SceneTreeTimer timer = tree.CreateTimer(seconds);
                await room.ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
                return;
            }
        }
        catch
        {
        }

        await Cmd.CustomScaledWait(seconds, seconds);
    }

    private static void StartOut()
    {
        if (_cardUsePlaying || _outPlaying)
            return;
        if (!_hasAnimOut)
        {
            Cleanup();
            return;
        }

        _outScheduled = false;
        _outPlaying = true;
        if (!PlaySingle(AnimOut))
            Cleanup();
    }

    private static void PlaySequence(string first, string nextLoop)
    {
        MegaSprite? sprite = _sprite;
        if (sprite == null || !HasAnim(sprite, first))
        {
            LogMissingAnimOnce(first);
            return;
        }

        MegaAnimationState state = sprite.GetAnimationState();
        if (string.Equals(first, nextLoop, StringComparison.Ordinal))
        {
            if (string.Equals(_lastFirst, first, StringComparison.Ordinal) && _lastNextLoop == null)
                return;

            state.SetAnimation(first, loop: true);
            _lastFirst = first;
            _lastNextLoop = null;
            return;
        }

        if (string.Equals(_lastFirst, first, StringComparison.Ordinal) &&
            string.Equals(_lastNextLoop, nextLoop, StringComparison.Ordinal))
        {
            return;
        }

        state.SetAnimation(first, loop: false);
        if (HasAnim(sprite, nextLoop))
        {
            state.AddAnimation(nextLoop, 0f, loop: true);
            _lastFirst = first;
            _lastNextLoop = nextLoop;
        }
        else
        {
            _lastFirst = first;
            _lastNextLoop = null;
        }
    }

    private static bool PlaySingle(string anim)
    {
        MegaSprite? sprite = _sprite;
        if (sprite == null)
            return false;
        if (!HasAnim(sprite, anim))
        {
            LogMissingAnimOnce(anim);
            return false;
        }
        if (string.Equals(_lastFirst, anim, StringComparison.Ordinal) && _lastNextLoop == null)
            return true;

        sprite.GetAnimationState().SetAnimation(anim, loop: false);
        _lastFirst = anim;
        _lastNextLoop = null;
        return true;
    }

    private static string? GetCardUseAnim(CardModel card)
    {
        if (card.Type == CardType.Attack)
            return _hasCardAttack ? AnimCardAttack : null;
        return _cardNonAttackAnim;
    }

    private static void InitAnimCache(MegaSprite sprite)
    {
        _hasAnimIn = sprite.HasAnimation(AnimIn);
        _hasAnimIdle = sprite.HasAnimation(AnimIdle);
        _hasAnimOut = sprite.HasAnimation(AnimOut);
        _hasCardAttack = sprite.HasAnimation(AnimCardAttack);

        _cardNonAttackAnim = null;
        foreach (string candidate in AnimCardNonAttackCandidates)
        {
            if (!sprite.HasAnimation(candidate))
                continue;
            _cardNonAttackAnim = candidate;
            break;
        }

        _lastFirst = null;
        _lastNextLoop = null;
    }

    private static bool HasAnim(MegaSprite sprite, string anim)
    {
        if (string.Equals(anim, AnimIn, StringComparison.Ordinal))
            return _hasAnimIn;
        if (string.Equals(anim, AnimIdle, StringComparison.Ordinal))
            return _hasAnimIdle;
        if (string.Equals(anim, AnimOut, StringComparison.Ordinal))
            return _hasAnimOut;
        if (string.Equals(anim, AnimCardAttack, StringComparison.Ordinal))
            return _hasCardAttack;
        if (_cardNonAttackAnim != null && string.Equals(anim, _cardNonAttackAnim, StringComparison.Ordinal))
            return true;

        return sprite.HasAnimation(anim);
    }

    private static void LogMissingAnimOnce(string anim)
    {
        if (MissingAnimsWarned.Add(anim))
            Log.Warn("[ChaosHeidemarie] Battle-ready overlay missing animation: " + anim);
    }

    private static void Cleanup()
    {
        _watchToken++;
        if (_node != null && GodotObject.IsInstanceValid(_node))
            _node.QueueFreeSafely();

        _node = null;
        _sprite = null;
        _baseCaptured = false;
        _basePos = default;
        _baseScale = Vector2.One;
        _busy = false;
        _outPlaying = false;
        _cardUsePlaying = false;
        _outScheduled = false;
        _lastFirst = null;
        _lastNextLoop = null;
    }
}

internal static class HeidemarieBattleDeadOverlay
{
    private const string Anim = "animation";

    private static PackedScene? _cachedScene;
    private static bool _sceneLoadAttempted;
    private static bool _sceneMissingWarned;
    private static bool _missingAnimWarned;
    private static bool _missingSkeletonWarned;
    private static Resource? _cachedSkeleton;
    private static bool _skeletonLoadAttempted;
    private static long _createDisabledUntil;
    private static int _createErrorLogged;
    private const int CreateDisableMs = 30000;

    public static void Preload()
    {
        _ = GetScene();
        _ = GetSkeleton();
    }

    public static void Play()
    {
        long now = System.Environment.TickCount64;
        if (now < _createDisabledUntil)
            return;

        try
        {
            NCombatRoom? room = NCombatRoom.Instance;
            if (room == null)
                return;

            PackedScene? scene = GetScene();
            if (scene == null)
            {
                if (!_sceneMissingWarned)
                {
                    _sceneMissingWarned = true;
                    Log.Warn("[ChaosHeidemarie] Missing battle-dead scene: " +
                             HeidemarieBattleReadyProfile.BattleDeadScenePath);
                }
                return;
            }

            Node instance = scene.Instantiate();
            ApplySkeletonIfPresent(instance);

            MegaSprite sprite;
            try
            {
                sprite = new MegaSprite(instance);
            }
            catch (Exception ex)
            {
                Log.Warn("[ChaosHeidemarie] Battle-dead overlay init failed: " + ex.Message);
                instance.QueueFreeSafely();
                return;
            }

            if (!sprite.HasAnimation(Anim))
            {
                if (!_missingAnimWarned)
                {
                    _missingAnimWarned = true;
                    Log.Warn("[ChaosHeidemarie] Battle-dead overlay missing animation: " + Anim);
                }
                instance.QueueFreeSafely();
                return;
            }

            CanvasLayer layer = new()
            {
                Layer = 1000
            };
            ((Node)layer).Name = "HeidemarieBattleDeadOverlayLayer";

            Node layerParent = room.Ui as Node ?? room;
            layerParent.AddChildSafely(layer);
            layer.AddChildSafely(instance);

            sprite.ConnectAnimationCompleted(Callable.From<GodotObject, GodotObject, GodotObject>((_, _, _) =>
            {
                if (GodotObject.IsInstanceValid(layer))
                    layer.QueueFreeSafely();
            }));

            if (instance is CanvasItem canvasItem)
            {
                canvasItem.TopLevel = true;
                canvasItem.ZAsRelative = false;
                canvasItem.ZIndex = 1000;
            }

            sprite.GetAnimationState().SetAnimation(Anim, loop: false);
        }
        catch (Exception ex)
        {
            _createDisabledUntil = System.Environment.TickCount64 + CreateDisableMs;
            if (System.Threading.Interlocked.Exchange(ref _createErrorLogged, 1) == 0)
                Log.Warn("[ChaosHeidemarie] Battle-dead overlay create failed: " + ex);
        }
    }

    private static PackedScene? GetScene()
    {
        if (_cachedScene != null)
            return _cachedScene;
        if (_sceneLoadAttempted)
            return null;

        _sceneLoadAttempted = true;
        try
        {
            _cachedScene = ResourceLoader.Load<PackedScene>(HeidemarieBattleReadyProfile.BattleDeadScenePath);
        }
        catch
        {
            _cachedScene = null;
        }

        return _cachedScene;
    }

    private static void ApplySkeletonIfPresent(Node instance)
    {
        Resource? skeleton = GetSkeleton();
        if (skeleton == null)
        {
            if (!_missingSkeletonWarned)
            {
                _missingSkeletonWarned = true;
                Log.Warn("[ChaosHeidemarie] Battle-dead overlay missing skeleton data.");
            }
            return;
        }

        instance.Set("skeleton_data_res", skeleton);
    }

    private static Resource? GetSkeleton()
    {
        if (_cachedSkeleton != null)
            return _cachedSkeleton;
        if (_skeletonLoadAttempted)
            return null;

        _skeletonLoadAttempted = true;
        string? selected = null;
        if (ResourceLoader.Exists(HeidemarieBattleReadyProfile.BattleDeadSkeletonDataPath))
            selected = HeidemarieBattleReadyProfile.BattleDeadSkeletonDataPath;
        else if (ResourceLoader.Exists(HeidemarieBattleReadyProfile.BattleDeadSkeletonDataFallbackPath))
            selected = HeidemarieBattleReadyProfile.BattleDeadSkeletonDataFallbackPath;

        if (selected == null)
            return null;

        try
        {
            _cachedSkeleton = ResourceLoader.Load<Resource>(selected);
        }
        catch
        {
            _cachedSkeleton = null;
        }

        return _cachedSkeleton;
    }
}
