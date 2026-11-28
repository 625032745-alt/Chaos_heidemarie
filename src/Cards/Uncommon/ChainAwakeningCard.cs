using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ChainAwakeningCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new("ChainAwakening", 6), new EnergyVar(3)];

    public ChainAwakeningCard() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = Owner.Creature.GetPower<InherentMemoryPower>();
        var baseValue = DynamicVars["ChainAwakening"].BaseValue;
        var energyValue = DynamicVars.Energy.BaseValue;
        if (power != null && power.Amount >= baseValue)
        {
            await PlayerCmd.GainEnergy(energyValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ChainAwakening"].UpgradeValueBy(-1);
    }
}