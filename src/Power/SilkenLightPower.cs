using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class SilkenLightPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/SilkenLightPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/SilkenLightPower_Big.png"
    );

    private bool _usedThisTurn;

    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target,
        decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (_usedThisTurn || canonicalPower is not InherentMemoryPower) return false;
        if (target != Owner || amount <= 0m) return false;
        modifiedAmount = amount + Amount;
        return true;
    }

    public override Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        _usedThisTurn = true;
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner) && side == CombatSide.Player)
        {
            _usedThisTurn = false;
            await PowerCmd.Remove(this);
        }
    }
}