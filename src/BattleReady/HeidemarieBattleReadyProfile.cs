using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ChaosHeidemarie.BattleReady;

internal static class HeidemarieBattleReadyProfile
{
    public const string ModKey = "chaos_heidemarie";
    public const string CharacterKey = "heidemarie";
    public const string BattleReadyScenePath = "res://ArtWorks/modspine/tscn_point/heidemarie_ready_point.tscn";
    public const string BattleDeadScenePath = "res://ArtWorks/modspine/tscn_point/heidemarie_dead_point.tscn";
    public const string BattleReadySkeletonDataPath =
        "res://ArtWorks/modspine/battle_ready/heidemarie_battle_ready/heidemarie_battle_ready_skel_data.tres";
    public const string BattleReadySkeletonDataFallbackPath =
        "res://ArtWorks/modspine/battle_ready/heidemarie_battle_ready/heidemarie_battle_ready_skel_data.tres";
    public const string BattleDeadSkeletonDataPath =
        "res://ArtWorks/modspine/deadcg/deadcg_heidemarie/deadcg_heidemarie_skel_data.tres";
    public const string BattleDeadSkeletonDataFallbackPath =
        "res://ArtWorks/modspine/deadcg/deadcg_heidemarie/deadcg_heidemarie_skel_data.tres";
}

internal static class HeidemarieBattleReadyTarget
{
    public static bool IsTarget(CharacterModel? character)
    {
        return character != null && character.Id == ModelDb.GetId<Characters.Heidemarie>();
    }

    public static bool IsTarget(Player? player)
    {
        return IsTarget(player?.Character);
    }

    public static bool IsMineTargetCard(CardModel? card)
    {
        return card != null && LocalContext.IsMine(card) && IsTarget(card.Owner?.Character);
    }

    public static bool IsLocalTargetCreature(NCreature? creature)
    {
        return creature != null &&
               creature.Entity.IsPlayer &&
               LocalContext.IsMe(creature.Entity) &&
               IsTarget(creature.Entity.Player);
    }
}
