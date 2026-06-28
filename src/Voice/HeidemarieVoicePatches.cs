using ChaosHeidemarie.BattleReady;
using ChaosHeidemarie.Cards;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace ChaosHeidemarie.Voice;

[HarmonyPatch(typeof(NCharacterSelectButton), nameof(NCharacterSelectButton.Select))]
internal static class HeidemarieCharacterSelectVoicePatch
{
    private static bool _prewarmed;

    [HarmonyPostfix]
    public static void Postfix(NCharacterSelectButton __instance)
    {
        if (HeidemarieBattleReadyTarget.IsTarget(__instance.Character))
        {
            if (!_prewarmed)
            {
                _prewarmed = true;
                HeidemarieVoicePlayer.Prewarm(HeidemarieVoiceProfile.VoicePrewarmCandidates);
            }

            HeidemarieVoicePlayer.Play(HeidemarieVoiceProfile.VoiceSelect);
            return;
        }

        HeidemarieVoicePlayer.FadeOutForCharacterSwitch(0.3f);
    }
}

[HarmonyPatch]
internal static class HeidemarieCombatVoicePatch
{
    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
    [HarmonyPostfix]
    public static void AfterBeforeCombatStart(IRunState runState, object? combatState)
    {
        Player? me = LocalContext.GetMe(runState);
        if (!HeidemarieBattleReadyTarget.IsTarget(me))
            return;

        HeidemarieVoicePlayer.PlayRandom(HeidemarieVoiceProfile.VoiceCombatStartCandidates);
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatVictory))]
    [HarmonyPostfix]
    public static void AfterCombatVictory(IRunState runState, object? combatState, CombatRoom room)
    {
        Player? me = LocalContext.GetMe(runState);
        if (!HeidemarieBattleReadyTarget.IsTarget(me))
            return;

        HeidemarieVoicePlayer.PlayRandom(HeidemarieVoiceProfile.VoiceVictoryCandidates);
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterRoomEntered))]
internal static class HeidemarieRoomVoicePatch
{
    [HarmonyPostfix]
    public static void Postfix(IRunState runState, AbstractRoom room)
    {
        if (room == null || room.RoomType != RoomType.RestSite)
            return;

        Player? me = LocalContext.GetMe(runState);
        if (!HeidemarieBattleReadyTarget.IsTarget(me))
            return;

        HeidemarieVoicePlayer.Play(HeidemarieVoiceProfile.VoiceRest);
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
internal static class HeidemarieCardVoicePatch
{
    [HarmonyPrefix]
    public static void Prefix(object combatState, CardPlay cardPlay)
    {
        CardModel? card = cardPlay.Card;
        if (!HeidemarieBattleReadyTarget.IsMineTargetCard(card))
            return;

        if (card is HeidemarieDefend || card?.Id == ModelDb.GetId<HeidemarieDefend>())
        {
            HeidemarieVoicePlayer.PlayRandom(HeidemarieVoiceProfile.VoiceDefendCandidates);
            return;
        }

        if (card is HeidemarieResolve || card?.Id == ModelDb.GetId<HeidemarieResolve>())
        {
            HeidemarieVoicePlayer.PlayRandom(HeidemarieVoiceProfile.VoicePowerCandidates);
            return;
        }

        if (card!.Type == CardType.Attack)
        {
            HeidemarieVoicePlayer.PlayRandom(HeidemarieVoiceProfile.VoiceAttackCandidates);
            return;
        }

        if (card.Type == CardType.Power)
        {
            HeidemarieVoicePlayer.PlayRandom(HeidemarieVoiceProfile.VoicePowerCandidates);
            return;
        }

        if (card.Type == CardType.Skill)
            HeidemarieVoicePlayer.PlayRandom(HeidemarieVoiceProfile.VoiceSkillCandidates);
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageReceived))]
internal static class HeidemarieDamageVoicePatch
{
    [HarmonyPostfix]
    public static void Postfix(
        PlayerChoiceContext choiceContext,
        IRunState runState,
        object? combatState,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == null || !target.IsPlayer)
            return;
        if (!LocalContext.IsMe(target))
            return;
        if (!HeidemarieBattleReadyTarget.IsTarget(target.Player))
            return;
        if (result.UnblockedDamage <= 0 && result.OverkillDamage <= 0)
            return;

        HeidemarieVoicePlayer.Play(HeidemarieVoiceProfile.VoiceHit);
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDeath))]
internal static class HeidemarieDeathVoicePatch
{
    [HarmonyPostfix]
    public static void Postfix(
        IRunState runState,
        object? combatState,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == null || !creature.IsPlayer)
            return;
        if (!LocalContext.IsMe(creature))
            return;
        if (!HeidemarieBattleReadyTarget.IsTarget(creature.Player))
            return;
        if (wasRemovalPrevented)
            return;

        HeidemarieVoicePlayer.Play(HeidemarieVoiceProfile.VoiceDefeat);
    }
}
