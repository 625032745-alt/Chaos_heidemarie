using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Base;

public abstract class TemporaryLinkCardBase : ModCardTemplate
{
    private readonly HashSet<CardModel> _temporarilyLinkedCards = new();

    protected TemporaryLinkCardBase(int baseCost, CardType type, CardRarity rarity, TargetType target,
        bool showInCardLibrary = true) : base(baseCost, type, rarity, target, showInCardLibrary)
    {
    }

    protected void LinkForTurn(IEnumerable<CardModel> cards)
    {
        foreach (var card in cards)
        {
            if (card.Keywords.Contains(LinkKeywords.Link))
                continue;

            card.AddKeyword(LinkKeywords.Link);
            _temporarilyLinkedCards.Add(card);
        }
    }

    private void ClearTemporaryLinks()
    {
        foreach (var card in _temporarilyLinkedCards)
        {
            if (card.Keywords.Contains(LinkKeywords.Link))
                card.RemoveKeyword(LinkKeywords.Link);
        }

        _temporarilyLinkedCards.Clear();
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
            ClearTemporaryLinks();

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        ClearTemporaryLinks();
        return Task.CompletedTask;
    }
}
