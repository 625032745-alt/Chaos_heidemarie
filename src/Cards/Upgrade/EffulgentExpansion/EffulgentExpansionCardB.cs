using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.EffulgentExpansion;

[RegisterCard(typeof(HeidemarieCardPool))]
public class EffulgentExpansionCardB : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/EffulgentExpansionCard.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link];
    private int _handCardCount;

    public EffulgentExpansionCardB() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card != this)
            return Task.CompletedTask;
        var playerCombatState = card.Owner.PlayerCombatState;
        if (null != playerCombatState)
        {
            _handCardCount = playerCombatState.Hand.Cards.Count(c => c.Keywords.Contains(LinkKeywords.Link));
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card != this)
            return;
        var combatState = card.CombatState;
        if (combatState == null)
            return;
        await CardGenerationHelper.GenerateEffulgentBlades(this, _handCardCount);
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return Task.CompletedTask;
        _handCardCount = 0;
        return Task.CompletedTask;
    }
}