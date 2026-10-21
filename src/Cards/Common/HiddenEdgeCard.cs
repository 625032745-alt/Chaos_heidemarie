using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class HiddenEdgeCard : ModCardTemplate
{
    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link];

    public HiddenEdgeCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        var selected = await CardSelectionHelper.SelectFromPile(choiceContext, Owner, PileType.Hand,
            CardSelectorPrefs.DiscardSelectionPrompt, 1, 1);
        var card = selected.FirstOrDefault();
        if (card != null && card.Type == CardType.Attack)
        {
            await CardCmd.Discard(choiceContext, card);
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}