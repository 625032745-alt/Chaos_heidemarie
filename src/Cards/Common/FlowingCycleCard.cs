using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class FlowingCycleCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("FlowingCycle", 1)];

    public FlowingCycleCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selected = await CardSelectionHelper.SelectFromPile(choiceContext, Owner, PileType.Hand,
            CardSelectorPrefs.DiscardSelectionPrompt, 1, 1);
        var card = selected.FirstOrDefault();
        if (card != null)
        {
            if (card.Keywords.Contains(LinkKeywords.Link))
            {
                await CardPileCmd.Add(card, PileType.Discard);
                var baseValue = DynamicVars["FlowingCycle"].BaseValue;
                await PowerCmd.Apply<ScarletSwordPower>(choiceContext, Owner.Creature, baseValue, Owner.Creature, this);
            }
            else
            {
                await CardCmd.Discard(choiceContext, card);
            }
            await CardPileCmd.Draw(choiceContext, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FlowingCycle"].UpgradeValueBy(1);
    }
}