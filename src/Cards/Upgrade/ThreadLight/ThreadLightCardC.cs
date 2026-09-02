using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.ThreadLight;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ThreadLightCardC : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/ThreadLightCard.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link, CardKeyword.Exhaust];
    private List<CardModel> _cards;

    public ThreadLightCardC() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var player = card.Owner;
        var combatState = player.PlayerCombatState;
        _cards = combatState.Hand.Cards.Where(c => c.Keywords.Contains(LinkKeywords.Link)).ToList();
        foreach (var c in _cards)
        {
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (_cards.Contains(card))
        {
           await CardPileCmd.Add(card,PileType.Hand);
        }
    }
}