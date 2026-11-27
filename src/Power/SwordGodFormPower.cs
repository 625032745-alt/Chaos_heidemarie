using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class SwordGodFormPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: "res://ArtWorks/images/power/EternalCrimsonDawnPower_Small.png",
            BigIconPath: "res://ArtWorks/images/power/EternalCrimsonDawnPower_Big.png"
        );

    // 两种资源分别记录上次触发回合
    private int _lastAuroraTriggerTurn = -1;
    private int _lastScarletTriggerTurn = -1;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount >= 0m)
            return;
        if (!ReferenceEquals(power.Owner, Owner))
            return;
        var player = Owner.Player;
        var combatState = player?.PlayerCombatState;
        if (player == null || combatState == null)
            return;
        var currentTurn = combatState.TurnNumber;
        var lostAmount = -amount;

        // 极光连锁：返还层数并获得2点能量
        if (power is InherentMemoryPower)
        {
            if (_lastAuroraTriggerTurn == currentTurn)
                return;
            _lastAuroraTriggerTurn = currentTurn;
            Flash();
            await PowerCmd.Apply<InherentMemoryPower>(choiceContext, Owner, lostAmount, Owner, cardSource);
            await PlayerCmd.GainEnergy(2m, player);
            return;
        }

        // 赤光剑：返还层数并获得格挡
        if (power is ScarletSwordPower)
        {
            if (_lastScarletTriggerTurn == currentTurn)
                return;
            _lastScarletTriggerTurn = currentTurn;
            Flash();
            await PowerCmd.Apply<ScarletSwordPower>(choiceContext, Owner, lostAmount, Owner, cardSource);
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
        }
    }
}