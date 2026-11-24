using ChaosHeidemarie.Cards.Token;
using ChaosHeidemarie.Content;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Relics;

[RegisterRelic(typeof(HeidemarieRelicPool))]
public sealed class BrokenRadianceHiltRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath:
        "res://ArtWorks/images/relic/BrokenRadianceHiltRelic_Small.png",
        IconOutlinePath:
        "res://ArtWorks/images/relic/BrokenRadianceHiltRelic_Outline.png",
        BigIconPath:
        "res://ArtWorks/images/relic/BrokenRadianceHiltRelic_Big.png"
    );

    // 本回合是否已经触发
    private bool _triggeredThisTurn;

    public override Task BeforeCombatStart()
    {
        _triggeredThisTurn = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        // 每回合只能触发一次
        if (_triggeredThisTurn)
            return;

        // 只处理极光剑
        if (card is not EffulgentBladeCard blade)
            return;

        // 只处理遗物持有者的极光剑
        if (card.Owner != Owner)
            return;

        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        // 先标记，避免重复触发
        _triggeredThisTurn = true;

        Flash();
        var enemy = combatState.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
        if (enemy == null) return;
        await CreatureCmd.Damage(choiceContext, enemy, blade.DynamicVars.Damage, blade, null);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner.Creature))
            return Task.CompletedTask;

        _triggeredThisTurn = false;

        return Task.CompletedTask;
    }
}