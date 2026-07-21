using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ChaosHeidemarie.BattleReady;

internal static class HeidemarieBattleReadyBodyHoldController
{
    private const double CardPlayFocusSuppressSeconds = 2.5;
    private const float OutDelaySeconds = 0.3f;
    private const float CancelOutDelaySeconds = 0.3f;
    private const string AnimEnter = "idle_to_b_idle";
    private const string AnimSpecialIdle = "b_idle";
    private const string AnimExit = "b_idle_to_idle";
    private const string AnimNormalIdle = "idle_loop";
    private const string AnimCollapseIdle = "collapse_idle";
    private const decimal CollapseIdleHpThreshold = 0.10m;

    private static bool _isHovered;
    private static bool _isUiFocused;
    private static ulong _focusToken;
    private static bool _outScheduled;
    private static bool _specialIdleActive;
    private static DateTime _suppressFocusUntilUtc = DateTime.MinValue;
    private static bool _cardUsePlaying;
    private static Player? _cardUseOwner;
    private static MegaSprite? _hookedBodySprite;
    private static Player? _hookedBodyOwner;

    private static bool IsFocused => _isHovered || _isUiFocused;
    private static bool IsFocusedEffective => IsFocused || _outScheduled;

    public static void NotifyCombatEnded()
    {
        _isHovered = false;
        _isUiFocused = false;
        _outScheduled = false;
        _specialIdleActive = false;
        _suppressFocusUntilUtc = DateTime.MinValue;
        _cardUsePlaying = false;
        _cardUseOwner = null;
        _hookedBodySprite = null;
        _hookedBodyOwner = null;
        _focusToken++;
    }

    public static void NotifyCombatStarted(Player? player)
    {
        if (!HeidemarieBattleReadyTarget.IsTarget(player))
            return;

        if (TryGetBodySprite(player, out MegaSprite? bodySprite))
            EnsureBodyHook(player, bodySprite!);

        TaskHelper.RunSafely(RefreshNormalIdleAfterCombatStart(player));
    }

    public static void NotifyCurrentHpChanged(Creature creature)
    {
        if (creature == null || !creature.IsPlayer)
            return;
        if (!HeidemarieBattleReadyTarget.IsTarget(creature.Player))
            return;

        if (TryGetBodySprite(creature.Player, out MegaSprite? bodySprite))
            EnsureBodyHook(creature.Player, bodySprite!);

        if (_cardUsePlaying || IsFocusedEffective)
            return;

        TryRefreshNormalIdleForHp(creature.Player);
    }

    public static void NotifyHovered(CardModel card, bool hovered)
    {
        if (!HeidemarieBattleReadyTarget.IsTarget(card.Owner?.Character))
            return;
        if (_isHovered == hovered)
            return;

        bool wasFocused = IsFocusedEffective;
        _isHovered = hovered;
        _focusToken++;

        if (hovered)
        {
            _outScheduled = false;
            if (IsFocusSuppressed())
                return;
            if (wasFocused)
                return;
            if (!_specialIdleActive)
                EnterSpecialIdle(card.Owner);
            return;
        }

        if (IsFocused)
            return;

        _outScheduled = true;
        ulong token = _focusToken;
        TaskHelper.RunSafely(DelayedReturnIfStillUnfocused(card.Owner, token, OutDelaySeconds));
    }

    public static void NotifyUiFocused(CardModel card, bool focused)
    {
        if (!HeidemarieBattleReadyTarget.IsTarget(card.Owner?.Character))
            return;
        if (_isUiFocused == focused)
            return;

        bool wasFocused = IsFocusedEffective;
        _isUiFocused = focused;
        _focusToken++;

        if (focused)
        {
            _outScheduled = false;
            if (IsFocusSuppressed())
                return;
            if (wasFocused)
                return;
            if (!_specialIdleActive)
                EnterSpecialIdle(card.Owner);
            return;
        }

        if (IsFocused)
            return;

        _outScheduled = true;
        ulong token = _focusToken;
        TaskHelper.RunSafely(DelayedReturnIfStillUnfocused(card.Owner, token, OutDelaySeconds));
    }

    public static void NotifyBeforeCardPlayed(CardPlay cardPlay)
    {
        CardModel? card = cardPlay.Card;
        if (card == null || !HeidemarieBattleReadyTarget.IsTarget(card.Owner?.Character))
            return;

        if (TryGetBodySprite(card.Owner, out MegaSprite? bodySprite))
            EnsureBodyHook(card.Owner, bodySprite!);

        _isHovered = false;
        _isUiFocused = false;
        _outScheduled = false;
        _specialIdleActive = false;
        _cardUsePlaying = true;
        _cardUseOwner = card.Owner;
        _suppressFocusUntilUtc = DateTime.UtcNow.AddSeconds(CardPlayFocusSuppressSeconds);
        _focusToken++;
    }

    public static void NotifyCanceled(CardModel card)
    {
        if (!HeidemarieBattleReadyTarget.IsTarget(card.Owner?.Character))
            return;

        _isHovered = false;
        _isUiFocused = false;
        _outScheduled = true;
        ulong token = ++_focusToken;
        TaskHelper.RunSafely(DelayedReturnIfStillUnfocused(card.Owner, token, CancelOutDelaySeconds));
    }

    private static async Task DelayedReturnIfStillUnfocused(Player? player, ulong token, float delaySeconds)
    {
        await WaitSeconds(delaySeconds);
        if (token != _focusToken)
            return;
        if (IsFocused)
            return;

        _outScheduled = false;
        ReturnToNormalIdle(player);
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

    private static async Task RefreshNormalIdleAfterCombatStart(Player? player)
    {
        await WaitSeconds(0.1f);
        if (!HeidemarieBattleReadyTarget.IsTarget(player))
            return;
        if (_cardUsePlaying || IsFocusedEffective)
            return;

        TryRefreshNormalIdleForHp(player);
    }

    private static void EnterSpecialIdle(Player? player)
    {
        if (!TryGetBodySprite(player, out MegaSprite? bodySprite))
            return;

        string currentAnim = GetCurrentAnimationName(bodySprite!) ?? string.Empty;
        if (IsActionAnimation(currentAnim))
            return;
        if (string.Equals(currentAnim, AnimSpecialIdle, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(currentAnim, AnimEnter, StringComparison.OrdinalIgnoreCase))
        {
            _specialIdleActive = true;
            return;
        }

        if (string.Equals(currentAnim, AnimExit, StringComparison.OrdinalIgnoreCase))
        {
            if (TryPlayLoop(bodySprite!, AnimSpecialIdle))
                _specialIdleActive = true;
            return;
        }

        if (TryPlaySequence(bodySprite!, AnimEnter, AnimSpecialIdle))
            _specialIdleActive = true;
    }

    private static void ReturnToNormalIdle(Player? player)
    {
        if (!_specialIdleActive)
            return;
        if (!TryGetBodySprite(player, out MegaSprite? bodySprite))
        {
            _specialIdleActive = false;
            return;
        }

        string currentAnim = GetCurrentAnimationName(bodySprite!) ?? string.Empty;
        if (string.Equals(currentAnim, AnimNormalIdle, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(currentAnim, AnimCollapseIdle, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(currentAnim, AnimExit, StringComparison.OrdinalIgnoreCase))
        {
            _specialIdleActive = false;
            return;
        }

        TryPlaySequence(bodySprite!, AnimExit, GetNormalIdleAnimation(bodySprite!, player));
        _specialIdleActive = false;
    }

    private static bool TryGetBodySprite(Player? player, out MegaSprite? bodySprite)
    {
        bodySprite = null;
        if (player == null)
            return false;

        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null)
            return false;

        NCreature? creatureNode = room.GetCreatureNode(player.Creature);
        if (creatureNode == null || !GodotObject.IsInstanceValid(creatureNode))
            return false;

        bodySprite = creatureNode.Visuals?.SpineBody;
        return bodySprite != null;
    }

    private static void EnsureBodyHook(Player? owner, MegaSprite bodySprite)
    {
        if (ReferenceEquals(_hookedBodySprite, bodySprite))
        {
            _hookedBodyOwner = owner;
            return;
        }

        _hookedBodySprite = bodySprite;
        _hookedBodyOwner = owner;
        bodySprite.ConnectAnimationCompleted(Callable.From<GodotObject, GodotObject, GodotObject>((_, __, ___) =>
        {
            if (_cardUsePlaying)
            {
                Player? o = _cardUseOwner;
                _cardUsePlaying = false;
                _cardUseOwner = null;
                ResolvePostCardUseIdle(o);
                return;
            }

            ResolvePostActionIdle(_hookedBodyOwner);
        }));
    }

    private static bool TryPlaySequence(MegaSprite bodySprite, string transitionAnim, string loopAnim)
    {
        bool hasLoop = HasAnim(bodySprite, loopAnim);
        bool hasTransition = HasAnim(bodySprite, transitionAnim);
        if (!hasLoop)
            return false;

        MegaAnimationState state = bodySprite.GetAnimationState();
        try
        {
            if (hasTransition)
            {
                state.SetAnimation(transitionAnim, loop: false);
                state.AddAnimation(loopAnim, 0f, loop: true);
                return true;
            }

            state.SetAnimation(loopAnim, loop: true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryPlayLoop(MegaSprite bodySprite, string loopAnim)
    {
        if (!HasAnim(bodySprite, loopAnim))
            return false;

        try
        {
            bodySprite.GetAnimationState().SetAnimation(loopAnim, loop: true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool HasAnim(MegaSprite bodySprite, string anim)
    {
        try
        {
            return bodySprite.HasAnimation(anim);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsFocusSuppressed()
    {
        return DateTime.UtcNow < _suppressFocusUntilUtc;
    }

    private static void ResolvePostCardUseIdle(Player? player)
    {
        if (!TryGetBodySprite(player, out MegaSprite? bodySprite))
        {
            _specialIdleActive = false;
            return;
        }

        if (IsFocused)
        {
            if (TryPlayLoop(bodySprite!, AnimSpecialIdle))
                _specialIdleActive = true;
            return;
        }

        TryPlaySequence(bodySprite!, AnimExit, GetNormalIdleAnimation(bodySprite!, player));
        _specialIdleActive = false;
    }

    private static void ResolvePostActionIdle(Player? player)
    {
        if (!TryGetBodySprite(player, out MegaSprite? bodySprite))
            return;

        string currentAnim = GetCurrentAnimationName(bodySprite!) ?? string.Empty;
        if (IsFocused)
        {
            if (string.Equals(currentAnim, AnimSpecialIdle, StringComparison.OrdinalIgnoreCase))
            {
                _specialIdleActive = true;
                return;
            }

            if (TryPlayLoop(bodySprite!, AnimSpecialIdle))
                _specialIdleActive = true;
            return;
        }

        if (_outScheduled)
            return;

        string targetIdle = GetNormalIdleAnimation(bodySprite!, player);
        if (string.Equals(currentAnim, targetIdle, StringComparison.OrdinalIgnoreCase))
        {
            _specialIdleActive = false;
            return;
        }

        _specialIdleActive = false;
        TryPlayLoop(bodySprite!, targetIdle);
    }

    private static void TryRefreshNormalIdleForHp(Player? player)
    {
        if (!TryGetBodySprite(player, out MegaSprite? bodySprite))
            return;
        if (_specialIdleActive || _outScheduled || IsFocused)
            return;

        string currentAnim = GetCurrentAnimationName(bodySprite!) ?? string.Empty;
        if (IsActionAnimation(currentAnim) ||
            string.Equals(currentAnim, AnimEnter, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(currentAnim, AnimSpecialIdle, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(currentAnim, AnimExit, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string targetIdle = GetNormalIdleAnimation(bodySprite!, player);
        if (string.Equals(currentAnim, targetIdle, StringComparison.OrdinalIgnoreCase))
            return;
        if (!string.Equals(currentAnim, AnimNormalIdle, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(currentAnim, AnimCollapseIdle, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        TryPlayLoop(bodySprite!, targetIdle);
    }

    private static string GetNormalIdleAnimation(MegaSprite bodySprite, Player? player)
    {
        if (!ShouldUseCollapseIdle(player))
            return AnimNormalIdle;
        return HasAnim(bodySprite, AnimCollapseIdle) ? AnimCollapseIdle : AnimNormalIdle;
    }

    private static bool ShouldUseCollapseIdle(Player? player)
    {
        Creature? creature = player?.Creature;
        if (creature == null)
            return false;
        if (creature.MaxHp <= 0m)
            return false;

        decimal hpRatio = (decimal)creature.CurrentHp / creature.MaxHp;
        return hpRatio <= CollapseIdleHpThreshold;
    }

    private static bool IsActionAnimation(string? animationName)
    {
        if (string.IsNullOrWhiteSpace(animationName))
            return false;

        return animationName.StartsWith("attack_", StringComparison.OrdinalIgnoreCase) ||
               animationName.StartsWith("u", StringComparison.OrdinalIgnoreCase) ||
               animationName.StartsWith("buff_", StringComparison.OrdinalIgnoreCase) ||
               animationName.StartsWith("hit", StringComparison.OrdinalIgnoreCase) ||
               animationName.StartsWith("death", StringComparison.OrdinalIgnoreCase) ||
               animationName.StartsWith("victory", StringComparison.OrdinalIgnoreCase) ||
               animationName.StartsWith("enter_", StringComparison.OrdinalIgnoreCase) ||
               animationName.StartsWith("defend", StringComparison.OrdinalIgnoreCase) ||
               animationName.StartsWith("cast", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetCurrentAnimationName(MegaSprite bodySprite)
    {
        try
        {
            return bodySprite.GetAnimationState()?.GetCurrent(0)?.GetAnimationName();
        }
        catch
        {
            return null;
        }
    }
}
