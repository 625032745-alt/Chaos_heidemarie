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
public class HeroAllUpgradeCardA : TemporaryLinkCardBase
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/HeroAllCard.png");

    public HeroAllUpgradeCardA() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await CardPileCmd.Draw(choiceContext, 1, player);
        var combatState = player.PlayerCombatState;
        if (combatState == null) return;
        var handCards = combatState.Hand.Cards.Where(c => c.Keywords.Contains(RestKeyword.REST));
        LinkForTurn(handCards);
    }
}
