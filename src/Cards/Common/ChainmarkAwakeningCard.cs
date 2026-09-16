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
public class ChainmarkAwakeningCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("ChainmarkAwakening", 1)];

    public ChainmarkAwakeningCard() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = DynamicVars["ChainmarkAwakening"].BaseValue;
        var power = Owner.Creature.GetPower<InherentMemoryPower>();
        if (power != null)
        {
            await PowerCmd.ModifyAmount(choiceContext, power, amount, null, this);
        }
        else
        {
            await PowerCmd.Apply<InherentMemoryPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ChainmarkAwakening"].UpgradeValueBy(1m);
    }
}