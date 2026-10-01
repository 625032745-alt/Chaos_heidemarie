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
        new(PortraitPath: "res://ArtWorks/images/cards/ThreadLightCard.png");

    private List<CardModel> _cards = new();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link, CardKeyword.Exhaust];

    public ThreadLightCardC() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var thisCard = cardPlay.Card;
        var player = thisCard.Owner;
        var combatState = player.PlayerCombatState;
        var cardsToPlay = combatState.Hand.Cards
            .Where(c => c != thisCard && c.Keywords.Contains(LinkKeywords.Link)).ToList();
        foreach (var card in cardsToPlay)
        {
            _cards.Add(card);
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }


    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cardModel = cardPlay.Card;
        if (cardModel != this) return;
        if (_cards.Count > 0)
        {
            foreach (var card in _cards)
            {
                await CardPileCmd.Add(card, PileType.Hand);
            }
            _cards.Clear();
        }
    }
}