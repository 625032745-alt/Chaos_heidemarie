using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class CrimsonChainPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/CrimsonChainPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/CrimsonChainPower_Big.png"
    );

    private bool _triggeredThisTurn;

    public async Task OnScarletSwordAttack(PlayerChoiceContext choiceContext, Creature target)
    {
        if (_triggeredThisTurn)
            return;
        _triggeredThisTurn = true;
        Flash();
        await PowerCmd.Apply<VulnerablePower>(choiceContext, target, 2m, Owner, null);
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, Amount, Owner, null);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner))
            return Task.CompletedTask;

        _triggeredThisTurn = false;

        return Task.CompletedTask;
    }
}