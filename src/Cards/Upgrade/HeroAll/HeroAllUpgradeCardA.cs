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
public class HeroAllUpgradeCardA : ModCardTemplate
{
    private readonly HashSet<CardModel> _linkedCards = new();
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
