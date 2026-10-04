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
public class HeroAllUpgradeCardD : TemporaryLinkCardBase
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/HeroAllCard.png");

    public HeroAllUpgradeCardD() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var drawPile = PileType.Draw.GetPile(player);
        var attackCards = drawPile.Cards.Where(c => c.Type == CardType.Attack).Take(2).ToList();
        foreach (var attackCard in attackCards)
        {
            await CardPileCmd.Add(attackCard, PileType.Hand);
        }
        var combatState = player.PlayerCombatState;
        if (combatState == null) return;
        var handCards = combatState.Hand.Cards.Where(c => c.Type == CardType.Attack).ToList();
        LinkForTurn(handCards);
    }
}
