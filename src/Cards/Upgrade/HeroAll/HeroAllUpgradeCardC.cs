using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.HeroAll;

[RegisterCard(typeof(HeidemarieCardPool))]
public class HeroAllUpgradeCardC : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/HeroAllCard.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public static readonly LocString SelectFromDrawToHand = new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_DRAW_TO_HAND");
    public static readonly LocString SelectFromDiscardToHand = new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_DISCARD_TO_HAND");

    public HeroAllUpgradeCardC() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SelectFromDrawPileToHand(choiceContext,cardPlay.Card.Owner);
        await SelectFromDiscardPileToHand(choiceContext,cardPlay.Card.Owner);
    }

    /// <summary>
    /// 从抽牌堆选择卡牌放入手牌
    /// </summary>
    /// <param name="ctx">玩家选择上下文</param>
    /// <param name="player">玩家</param>
    /// <param name="count">选择数量</param>
    /// <returns></returns>
    public async Task SelectFromDrawPileToHand(PlayerChoiceContext ctx, Player player)
    {
        var pile = PileType.Draw.GetPile(player);
        var selectFrom = (from c in pile.Cards orderby c.Rarity, c.Id select c).ToList();
        IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(ctx, selectFrom, player, new CardSelectorPrefs(SelectFromDrawToHand, 1));
        foreach (var card in selected)
        {
            card.AddKeyword(LinkKeywords.Link);
        }
        await CardPileCmd.Add(selected, PileType.Hand);
    }

    /// <summary>
    /// 从弃牌堆选择卡牌放入手牌
    /// </summary>
    /// <param name="ctx">玩家选择上下文</param>
    /// <param name="player">玩家</param>
    /// <param name="count">选择数量</param>
    /// <returns></returns>
    public async Task SelectFromDiscardPileToHand(PlayerChoiceContext ctx, Player player)
    {
        var pile = PileType.Discard.GetPile(player);
        var selectFrom = (from c in pile.Cards orderby c.Rarity, c.Id select c).ToList();
        IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(ctx, selectFrom, player, new CardSelectorPrefs(SelectFromDiscardToHand, 1));
        foreach (var card in selected)
        {
            card.AddKeyword(LinkKeywords.Link);
        }
        await CardPileCmd.Add(selected, PileType.Hand);
    }
}
