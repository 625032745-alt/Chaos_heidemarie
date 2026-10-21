using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class DawnshatterPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => DynamicVars["CardsLeft"].IntValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CardsLeft", Amount)];

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/DawnshatterPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/DawnshatterPower_Big.png"
    );

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not InherentMemoryPower || power.Owner != Owner)
            return;
        if (amount <= 0)
            return;
        if (power.Amount < 3)
            return;
        if (DynamicVars["CardsLeft"].IntValue <= 0)
            return;
        DynamicVars["CardsLeft"].BaseValue--;
        InvokeDisplayAmountChanged();
        await CardPileCmd.Draw(choiceContext, Owner.Player);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
            return Task.CompletedTask;
        DynamicVars["CardsLeft"].BaseValue = Amount;
        InvokeDisplayAmountChanged();

        return Task.CompletedTask;
    }
}