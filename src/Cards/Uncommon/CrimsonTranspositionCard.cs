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
public class CrimsonTranspositionCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CrimsonTransposition", 2), new EnergyVar(1)];

    public CrimsonTranspositionCard() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = Owner.Creature.GetPower<ScarletSwordPower>();
        var baseValue = DynamicVars["CrimsonTransposition"].BaseValue;
        if (power != null && power.Amount >= baseValue)
        {
            await power.ConsumeAsync(choiceContext, baseValue, Owner.Creature,
                this);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
            await CardPileCmd.Draw(choiceContext, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["CrimsonTransposition"].UpgradeValueBy(-1);
    }
}