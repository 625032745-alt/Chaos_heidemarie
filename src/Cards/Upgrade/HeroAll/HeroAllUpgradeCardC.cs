using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.HeroAll;

[RegisterCard(typeof(HeidemarieCardPool))]
public class HeroAllUpgradeCardC : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/HeroAllCard.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    private static readonly LocString SelectFromDrawToHand =
        new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_DRAW_TO_HAND");

    private static readonly LocString SelectFromDiscardToHand =
        new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_DISCARD_TO_HAND");

    public HeroAllUpgradeCardC() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SelectFromPileToHand(choiceContext, Owner, PileType.Draw, SelectFromDrawToHand);
        await SelectFromPileToHand(choiceContext, Owner, PileType.Discard, SelectFromDiscardToHand);
    }

    private async Task SelectFromPileToHand(PlayerChoiceContext ctx, Player player, PileType pileType, LocString prompt)
    {
        var selected = await CardSelectionHelper.SelectFromPile(
            ctx, player, pileType, prompt, 1, 1);
        foreach (var card in selected)
        {
            card.AddKeyword(LinkKeywords.Link);
        }
        await CardPileCmd.Add(selected, PileType.Hand);
    }
}