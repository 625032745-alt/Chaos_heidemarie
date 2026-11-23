using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Rare;

[RegisterCard(typeof(HeidemarieCardPool))]
public class DawnSanctuaryCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2), new BlockVar(10, ValueProp.Move)];

    public DawnSanctuaryCard() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        var select = await CardSelectionHelper.SelectFromPile(choiceContext, Owner, PileType.Hand,
            CardSelectorPrefs.DiscardSelectionPrompt, 2);
        if (select.Count > 0)
        {
            await CardCmd.Discard(choiceContext, select);
        }

        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}