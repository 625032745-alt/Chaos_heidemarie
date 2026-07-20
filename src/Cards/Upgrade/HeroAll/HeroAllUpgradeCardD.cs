using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
public class HeroAllUpgradeCardD : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/HeroAllCard.png");
    private readonly HashSet<CardModel> _linkedCards = new();

    public HeroAllUpgradeCardD() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var drawPile = PileType.Draw.GetPile(player);
        var attackCards = drawPile.Cards.Where(c => c.Type == CardType.Attack).ToList();
        for (int i = 0; i < 2; i++)
        {
            var attackCard = attackCards[i];
            attackCard.RemoveFromCurrentPile();
            await CardPileCmd.Add(attackCard, PileType.Hand);
        }
        var combatState = player.PlayerCombatState;
        if (combatState == null) return;
        var handCards = combatState.Hand.Cards.Where(c => c.Type == CardType.Attack).ToList();
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
