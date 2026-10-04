using ChaosHeidemarie.Content;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class FirstLightCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    private static readonly LocString SelectFromDrawToHand =
        new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_DRAW_TO_HAND");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("FirstLight", 3)];

    public FirstLightCard() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SelectFromDrawPileToHand(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FirstLight"].UpgradeValueBy(1);
    }

    private async Task SelectFromDrawPileToHand(PlayerChoiceContext ctx, Player player)
    {
        var pile = PileType.Draw.GetPile(player);
        var baseValue = DynamicVars["FirstLight"].IntValue;
        var selectFrom = pile.Cards.Take(baseValue).ToList();
        if (selectFrom.Count == 0)
            return;
        IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(ctx, selectFrom, player,
            new CardSelectorPrefs(SelectFromDrawToHand, 1));
        await CardPileCmd.Add(selected, PileType.Hand);
    }
}