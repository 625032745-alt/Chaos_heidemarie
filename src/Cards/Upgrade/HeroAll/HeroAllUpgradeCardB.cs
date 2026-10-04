using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.HeroAll;

[RegisterCard(typeof(HeidemarieCardPool))]
public class HeroAllUpgradeCardB : TemporaryLinkCardBase
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/HeroAllCard.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];

    public HeroAllUpgradeCardB() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 3, Owner);
        var combatState = cardPlay.Card.Owner.PlayerCombatState;
        if (combatState == null) return;
        var handCards = combatState.Hand.Cards.Where(c => c.EnergyCost.GetWithModifiers(CostModifiers.All) <= 1);
        LinkForTurn(handCards);
    }
}
