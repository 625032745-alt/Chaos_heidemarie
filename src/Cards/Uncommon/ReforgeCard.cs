using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ReforgeCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link];
    public static readonly LocString SelectFromHand = new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_HAND_TO_LINK");

    public ReforgeCard() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cardModel = cardPlay.Card;
        if (cardModel != this) return;
        var player = cardModel.Owner;
        var pile = PileType.Hand.GetPile(player);
        var cards = pile.Cards.Where(c => c.Keywords.Contains(LinkKeywords.Link)).ToList();
        foreach (var card in cards)
        {
            card.RemoveKeyword(LinkKeywords.Link);
        }

        await SelectFromHandPileToLink(choiceContext, player);
    }

    private async Task SelectFromHandPileToLink(PlayerChoiceContext ctx, Player player)
    {
        var pile = PileType.Hand.GetPile(player);
        var selectFrom = (from c in pile.Cards orderby c.Rarity, c.Id select c).ToList();
        var selected =
            await CardSelectCmd.FromSimpleGrid(ctx, selectFrom, player, new CardSelectorPrefs(SelectFromHand, 2));
        foreach (var card in selected)
        {
            card.AddKeyword(LinkKeywords.Link);
        }
    }
}