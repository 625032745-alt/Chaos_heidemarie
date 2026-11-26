using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Rare;

[RegisterCard(typeof(HeidemarieCardPool))]
public class HeroAllCard : TemporaryLinkCardBase
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];
    public override CardVisualStyle CustomVisualStyle => CardVisualStyle.Ancient;

    public HeroAllCard() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this)
            return;
        var drawnCards = await CardPileCmd.Draw(choiceContext, 3, Owner);
        LinkForTurn(drawnCards);
    }

    protected override void OnUpgrade()
    {
        CardCmd.ApplyKeyword(this, LinkKeywords.Link);
    }
}