using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.HeroAll;

[RegisterCard(typeof(HeidemarieCardPool))]
public class HeroAllUpgradeCardB : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/HeroAllCard.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];
    private readonly HashSet<CardModel> _linkedCards = new();

    public HeroAllUpgradeCardB() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 3, Owner);
        var combatState = cardPlay.Card.Owner.PlayerCombatState;
        if (combatState == null) return;
        var handCards = combatState.Hand.Cards.Where(c => c.EnergyCost.GetWithModifiers(CostModifiers.All) <= 1);
        foreach (var card in handCards)
        {
            card.AddKeyword(LinkKeywords.Link);
            _linkedCards.Add(card);
        }
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        foreach (var card in _linkedCards)
        {
            if (card != null && card.Keywords.Contains(LinkKeywords.Link))
            {
                card.RemoveKeyword(LinkKeywords.Link);
            }
        }

        _linkedCards.Clear();
        return Task.CompletedTask;
    }
}
