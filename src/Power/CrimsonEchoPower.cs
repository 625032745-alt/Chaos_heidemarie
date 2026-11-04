using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class CrimsonEchoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/CrimsonEchoPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/CrimsonEchoPower_Big.png"
    );
    
    private int _lastTriggerTurn = -1;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not ScarletSwordPower)
            return;
        if (amount >= 0m)
            return;
        var combatState = Owner.Player?.PlayerCombatState;
        if (combatState == null)
            return;
        // 本回合已经触发
        if (combatState.TurnNumber == _lastTriggerTurn)
            return;
        // 记录本回合已经触发
        _lastTriggerTurn = combatState.TurnNumber;
        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
    }
}