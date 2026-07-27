using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Cards.Upgrade.HeroAll;
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

namespace ChaosHeidemarie.Cards;

[RegisterCard(typeof(HeidemarieCardPool))]
public class HeroAllCard : TransformAtTurnStartCardBase
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    private readonly HashSet<CardModel> _linkedCards = new();
    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];

    public HeroAllCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this)
            return;
        var drawnCards = await CardPileCmd.Draw(choiceContext, 3, Owner);
        foreach (var drawnCard in drawnCards)
        {
            if (drawnCard.Keywords.Contains(LinkKeywords.Link))
            {
                continue;
            }

            _linkedCards.Add(drawnCard);
            drawnCard.AddKeyword(LinkKeywords.Link);
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

    protected override void OnUpgrade()
    {
        CardCmd.ApplyKeyword(this, LinkKeywords.Link);
    }

    protected override Type[] GetCandidateCardTypes()
    {
        return
        [
            typeof(HeroAllUpgradeCardA),
            typeof(HeroAllUpgradeCardB),
            typeof(HeroAllUpgradeCardC),
            typeof(HeroAllUpgradeCardD)
        ];
    }
}
