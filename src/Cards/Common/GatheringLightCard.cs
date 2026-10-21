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
public class GatheringLightCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    private static readonly LocString SelectFromDiscardToHand =
        new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_DISCARD_TO_HAND");

    public override bool GainsBlock => true;
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6, ValueProp.Move)];

    public GatheringLightCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        var selected =
            await CardSelectionHelper.SelectFromPile(choiceContext, Owner, PileType.Discard, SelectFromDiscardToHand, 1,
                1);
        foreach (var card in selected)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
    }
}