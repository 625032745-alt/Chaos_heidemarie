using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using ChaosHeidemarie.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ResidualDawnCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("ResidualDawn", 1)];
    private static readonly LocString SelectFromHand = new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_HAND");


    public ResidualDawnCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await SelectFromHandPile(choiceContext, cardPlay.Card.Owner);

    protected override void OnUpgrade()
    {
        DynamicVars["ResidualDawn"].UpgradeValueBy(1m);
    }

    private async Task SelectFromHandPile(PlayerChoiceContext ctx, Player player)
    {
        var pile = PileType.Hand.GetPile(player);
        var selectFrom = (from c in pile.Cards orderby c.Rarity, c.Id select c).ToList();
        var selectCount = DynamicVars["ResidualDawn"].IntValue;
        var selected =
            await CardSelectCmd.FromSimpleGrid(ctx, selectFrom, player, new CardSelectorPrefs(SelectFromHand, selectCount));
        foreach (var card in selected)
        {
            if (card.Keywords.Contains(LinkKeywords.Link))
            {
                await CommonUtils.AddOrModifyPower<InherentMemoryPower>(ctx, Owner.Creature, 1m, this);
                break;
            }
            card.AddKeyword(LinkKeywords.Link);
        }
    }
}