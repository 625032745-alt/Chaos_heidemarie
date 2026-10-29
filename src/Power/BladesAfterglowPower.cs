using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class BladesAfterglowPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/BladesAfterglowPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/BladesAfterglowPower_Big.png"
    );
    
    private const int RequiredAttackCount = 3;
    
    private int _attackCount;
    
    public async Task OnScarletSwordAttack(PlayerChoiceContext choiceContext)
    {
        _attackCount++;
        if (_attackCount < RequiredAttackCount)
            return;
        Flash();
        await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, Amount, ValueProp.Unpowered, Owner);
    }

    // 每回合结束时重置计数
    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext,
        CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner))
            return Task.CompletedTask;

        _attackCount = 0;
        return Task.CompletedTask;
    }
}