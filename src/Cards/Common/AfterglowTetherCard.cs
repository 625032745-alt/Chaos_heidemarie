using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class AfterglowTetherCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(10, ValueProp.Move)];

    private static readonly LocString SelectFromDrawToHand =
        new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_DRAW_TO_HAND");

    public AfterglowTetherCard() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        var selected = await CardSelectionHelper.SelectFromPile(
            choiceContext, Owner, PileType.Draw, SelectFromDrawToHand, 2);
        foreach (var card in selected)
        {
            card.AddKeyword(CardKeyword.Retain);
            await CardPileCmd.Add(card, PileType.Hand);   
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}