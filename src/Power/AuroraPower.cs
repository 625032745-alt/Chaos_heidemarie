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
public class AuroraPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    private int _lastGainTurn = -1;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/AuroraPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/AuroraPower_Big.png"
    );

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext,
        PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not InherentMemoryPower || amount <= 0m) return;

        var combatState = Owner.Player?.PlayerCombatState;
        if (combatState == null || combatState.TurnNumber == _lastGainTurn)
            return;

        _lastGainTurn = combatState.TurnNumber;
        Flash();
        await CardPileCmd.Draw(choiceContext, Owner.Player);
    }
}