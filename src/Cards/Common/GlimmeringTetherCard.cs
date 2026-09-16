using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class GlimmeringTetherCard : ModCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("GlimmeringTether", 1)];
    private static readonly LocString SelectFromHand = new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_HAND");
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public GlimmeringTetherCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, Owner);
        await SelectFromHandPile(choiceContext, Owner);
        
    }
    
    private async Task SelectFromHandPile(PlayerChoiceContext ctx, Player player)
    {
        var pile = PileType.Hand.GetPile(player);
        var selectFrom = (from c in pile.Cards orderby c.Rarity, c.Id select c).ToList();
        var selectCount = DynamicVars["GlimmeringTether"].IntValue;
        var selected =
            await CardSelectCmd.FromSimpleGrid(ctx, selectFrom, player, new CardSelectorPrefs(SelectFromHand, selectCount));
        foreach (var card in selected)
        {
            card.AddKeyword(LinkKeywords.Link);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["GlimmeringTether"].UpgradeValueBy(1);
    }
}