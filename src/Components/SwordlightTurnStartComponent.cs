using ChaosHeidemarie.Cards;
using ChaosHeidemarie.Powers;
using MinionLib.Component;

namespace ChaosHeidemarie.Components;

public sealed partial class SwordlightTurnStartComponent : CardComponent
{
    public override Task AfterCardEnteredCombatPostfix(
        CardModel card,
        ComponentContext componentContext)
    {
        if (!ReferenceEquals(card, Card) || card is not Swordlight)
            return Task.CompletedTask;

        return SwordlightPower.EnsureInstalled(card.Owner, card);
    }

    public override Task AfterCardChangedPilesPrefix(
        CardModel card,
        PileType oldPileType,
        AbstractModel? source,
        ComponentContext componentContext)
    {
        if (!ReferenceEquals(card, Card) || card is not Swordlight)
            return Task.CompletedTask;

        return SwordlightPower.EnsureInstalled(card.Owner, card);
    }

    public override async Task BeforeSideTurnStartPrefix(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState,
        ComponentContext componentContext)
    {
        var card = Card;
        if (card is not Swordlight || side != CombatSide.Player || !participants.Contains(card.Owner.Creature))
            return;
        if (card.Pile?.Type != PileType.Discard)
            return;

        await CardPileCmd.Add(card, PileType.Draw);
    }
}
