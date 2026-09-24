using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ChainAwakeningCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new("ChainAwakening", 5), new("ChainAwakeningEnergy", 2)];

    public ChainAwakeningCard() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = Owner.Creature.GetPower<InherentMemoryPower>();
        var baseValue = DynamicVars["ChainAwakening"].BaseValue;
        var baseValue2 = DynamicVars["ChainAwakeningEnergy"].BaseValue;
        if (power != null && power.Amount >= baseValue)
        {
            await PlayerCmd.GainEnergy(baseValue2, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ChainAwakening"].UpgradeValueBy(-1);
        DynamicVars["ChainAwakeningEnergy"].UpgradeValueBy(1);
    }
}