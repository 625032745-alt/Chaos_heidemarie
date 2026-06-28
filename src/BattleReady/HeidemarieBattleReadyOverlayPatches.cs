using System;
using ChaosHeidemarie.Cards;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace ChaosHeidemarie.BattleReady;

[HarmonyPatch]
internal static class HeidemarieBattleReadyOverlayPatches
{
    private static int _combatAnimToken;

    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
    [HarmonyPostfix]
    public static void AfterBeforeCombatStart(IRunState runState, object? combatState)
    {
        try
        {
            Player? me = LocalContext.GetMe(runState);
            if (!HeidemarieBattleReadyTarget.IsTarget(me))
                return;

            HeidemarieBattleReadyOverlay.Preload();
            HeidemarieBattleDeadOverlay.Preload();
            TryPlayCombatStartAnimation(me);
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatVictory))]
    [HarmonyPostfix]
    public static void AfterCombatVictory(IRunState runState, object? combatState, CombatRoom room)
    {
        try
        {
            Player? me = LocalContext.GetMe(runState);
            if (!HeidemarieBattleReadyTarget.IsTarget(me))
                return;

            HeidemarieBattleReadyOverlay.NotifyCombatEnded();
            TryPlayVictoryAnimation(me);
        }
        catch
        {
        }
    }

    private static void TryPlayCombatStartAnimation(Player? player)
    {
        if (player == null)
            return;

        int token = ++_combatAnimToken;
        TryApplyPlayerAnimation(player, "battle_start", "idle_loop", token, retries: 8);
    }

    private static void TryPlayVictoryAnimation(Player? player)
    {
        if (player == null)
            return;

        int token = ++_combatAnimToken;
        TryApplyPlayerAnimation(player, "victory_ready", "victory_loop", token, retries: 8);
    }

    private static void TryApplyPlayerAnimation(Player player, string firstAnim, string? loopAnim, int token, int retries)
    {
        try
        {
            if (token != _combatAnimToken)
                return;

            NCombatRoom? room = NCombatRoom.Instance;
            if (room == null)
                return;

            NCreature? creatureNode = room.GetCreatureNode(player.Creature);
            if (creatureNode == null || !GodotObject.IsInstanceValid(creatureNode) || !creatureNode.HasSpineAnimation)
            {
                if (retries > 0)
                    Callable.From(() => TryApplyPlayerAnimation(player, firstAnim, loopAnim, token, retries - 1)).CallDeferred();
                return;
            }

            var state = creatureNode.SpineAnimation.GetAnimationState();
            if (state == null)
            {
                if (retries > 0)
                    Callable.From(() => TryApplyPlayerAnimation(player, firstAnim, loopAnim, token, retries - 1)).CallDeferred();
                return;
            }

            creatureNode.SpineAnimation.SetAnimation(firstAnim, loop: false);
            if (loopAnim != null)
                creatureNode.SpineAnimation.AddAnimation(loopAnim, 0f, loop: true);
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterDeath))]
    [HarmonyPostfix]
    public static void AfterDeathPostfix(
        IRunState runState,
        object? combatState,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        try
        {
            if (creature == null ||
                !creature.IsPlayer ||
                !LocalContext.IsMe(creature) ||
                !HeidemarieBattleReadyTarget.IsTarget(creature.Player) ||
                wasRemovalPrevented)
            {
                return;
            }

            HeidemarieBattleReadyOverlay.NotifyCombatEnded();
            HeidemarieBattleDeadOverlay.Play();
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(NMouseCardPlay), nameof(NMouseCardPlay.Start))]
    [HarmonyPostfix]
    public static void AfterMouseCardPlayStart(NMouseCardPlay __instance)
    {
        try
        {
            CardModel? card = __instance.Holder?.CardModel;
            if (!HeidemarieBattleReadyTarget.IsMineTargetCard(card))
                return;

            HeidemarieBattleReadyOverlay.NotifyHovered(card!, hovered: true);
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(NHandCardHolder), "OnFocus")]
    [HarmonyPostfix]
    public static void AfterHandFocus(NHandCardHolder __instance)
    {
        try
        {
            CardModel? card = __instance.CardModel;
            if (!HeidemarieBattleReadyTarget.IsMineTargetCard(card))
                return;

            HeidemarieBattleReadyOverlay.NotifyUiFocused(card!, focused: true);
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(NHandCardHolder), "OnUnfocus")]
    [HarmonyPostfix]
    public static void AfterHandUnfocus(NHandCardHolder __instance)
    {
        try
        {
            CardModel? card = __instance.CardModel;
            if (!HeidemarieBattleReadyTarget.IsMineTargetCard(card))
                return;

            HeidemarieBattleReadyOverlay.NotifyUiFocused(card!, focused: false);
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(NHandCardHolder), "OnMousePressed")]
    [HarmonyPostfix]
    public static void AfterHandMousePressed(NHandCardHolder __instance, InputEvent inputEvent)
    {
        try
        {
            if (inputEvent is not InputEventMouseButton { ButtonIndex: MouseButton.Left })
                return;

            CardModel? card = __instance.CardModel;
            if (!HeidemarieBattleReadyTarget.IsMineTargetCard(card))
                return;

            HeidemarieBattleReadyOverlay.NotifyHovered(card!, hovered: true);
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(NHandCardHolder), "DoCardHoverEffects")]
    [HarmonyPostfix]
    public static void AfterHandHoverEffects(NHandCardHolder __instance, bool isHovered)
    {
        try
        {
            CardModel? card = __instance.CardModel;
            if (!HeidemarieBattleReadyTarget.IsMineTargetCard(card))
                return;

            if (isHovered)
            {
                HeidemarieBattleReadyOverlay.NotifyHovered(card!, hovered: true);
                return;
            }

            if (__instance.HasFocus() || Input.IsMouseButtonPressed(MouseButton.Left))
                return;

            HeidemarieBattleReadyOverlay.NotifyHovered(card!, hovered: false);
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
    [HarmonyPrefix]
    public static void BeforeCardPlayedPrefix(object combatState, CardPlay cardPlay)
    {
        try
        {
            CardModel? card = cardPlay.Card;
            if (!HeidemarieBattleReadyTarget.IsMineTargetCard(card))
                return;

            HeidemarieBattleReadyOverlay.NotifyBeforeCardPlayed(cardPlay);
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
    [HarmonyPostfix]
    public static void AfterBeforeCardPlayedPostfix(object combatState, CardPlay cardPlay)
    {
        try
        {
            CardModel? card = cardPlay.Card;
            if (!HeidemarieBattleReadyTarget.IsMineTargetCard(card))
                return;
            if (card is not HeidemarieDefend && card?.Id != ModelDb.GetId<HeidemarieDefend>())
                return;

            TryPlayDefendAnimation(card.Owner);
        }
        catch
        {
        }
    }

    private static void TryPlayDefendAnimation(Player? player)
    {
        if (player == null)
            return;

        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null)
            return;

        NCreature? creatureNode = room.GetCreatureNode(player.Creature);
        if (creatureNode == null || !GodotObject.IsInstanceValid(creatureNode) || !creatureNode.HasSpineAnimation)
            return;

        try
        {
            if (creatureNode.SpineAnimation.SetAnimation("defend", loop: false) == null)
                return;
        }
        catch
        {
            return;
        }

        try
        {
            _ = creatureNode.SpineAnimation.AddAnimation("idle_loop", 0f, loop: true);
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(NCardPlay), nameof(NCardPlay.CancelPlayCard))]
    [HarmonyPostfix]
    public static void AfterCancelPlayCard(NCardPlay __instance)
    {
        try
        {
            CardModel? card = __instance.Holder?.CardModel;
            if (!HeidemarieBattleReadyTarget.IsMineTargetCard(card))
                return;

            HeidemarieBattleReadyOverlay.NotifyCanceled(card!);
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(NControllerCardPlay), nameof(NControllerCardPlay.Start))]
    [HarmonyPostfix]
    public static void AfterControllerPlayStart(NControllerCardPlay __instance)
    {
        try
        {
            CardModel? card = __instance.Holder?.CardNode?.Model;
            if (!HeidemarieBattleReadyTarget.IsMineTargetCard(card))
                return;

            HeidemarieBattleReadyOverlay.NotifyUiFocused(card!, focused: true);
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(NControllerCardPlay), nameof(NControllerCardPlay._Input))]
    [HarmonyPostfix]
    public static void AfterControllerInput(NControllerCardPlay __instance, InputEvent inputEvent)
    {
        try
        {
            if (inputEvent is not InputEventAction { Pressed: true } action)
                return;
            if (action.Action != MegaInput.cancel)
                return;

            CardModel? card = __instance.Holder?.CardModel;
            if (!HeidemarieBattleReadyTarget.IsMineTargetCard(card))
                return;

            HeidemarieBattleReadyOverlay.NotifyCanceled(card!);
        }
        catch
        {
        }
    }
}
