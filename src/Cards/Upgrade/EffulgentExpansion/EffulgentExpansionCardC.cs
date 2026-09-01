using ChaosHeidemarie.Cards.Token;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.EffulgentExpansion;

[RegisterCard(typeof(HeidemarieCardPool))]
public class EffulgentExpansionCardC : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/EffulgentExpansionCard.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link,RestKeyword.REST];
    
    public EffulgentExpansionCardC() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var player = card.Owner;
        var combatState = card.CombatState;
        if (combatState == null)
            return;
        for (var i = 0; i < 6; i++)
        {
            var newCard = combatState.CreateCard<EffulgentBladeCard>(player);
            await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Discard, player);
        }
    }
}