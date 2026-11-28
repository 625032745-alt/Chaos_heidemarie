using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ReforgeCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    private static readonly LocString SelectFromHand =
        new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_HAND_TO_LINK");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("ChooseLink", 2)];

    public ReforgeCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var pile = PileType.Hand.GetPile(Owner);
        var cards = pile.Cards.Where(c => c.Keywords.Contains(LinkKeywords.Link)).ToList();
        foreach (var card in cards)
        {
            card.RemoveKeyword(LinkKeywords.Link);
        }

        await SelectFromHandPileToLink(choiceContext, Owner);
    }

    private async Task SelectFromHandPileToLink(PlayerChoiceContext ctx, Player player)
    {
        var baseValue = DynamicVars["ChooseLink"].IntValue;
        var selected = await CardSelectionHelper.SelectFromPile(
            ctx, player, PileType.Hand, SelectFromHand, baseValue);
        foreach (var card in selected)
        {
            card.AddKeyword(LinkKeywords.Link);
        }
        if (selected.Count > 0)
            await PowerCmd.Apply<InherentMemoryPower>(ctx, Owner.Creature, selected.Count, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ChooseLink"].UpgradeValueBy(1);
    }
}