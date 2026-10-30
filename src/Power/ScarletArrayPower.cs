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
public class ScarletArrayPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: "res://ArtWorks/images/power/ScarletArrayPower_Small.png",
            BigIconPath: "res://ArtWorks/images/power/ScarletArrayPower_Big.png"
        );

    private bool _firstAttackCheckedThisTurn;
    private int _extraAttackCount;


    public async Task OnScarletSwordAttack(PlayerChoiceContext choiceContext)
    {
        if (!_firstAttackCheckedThisTurn)
        {
            _firstAttackCheckedThisTurn = true;
            var scarletSwordPower = Owner.GetPower<ScarletSwordPower>();
            if (scarletSwordPower == null || scarletSwordPower.Amount < 2m)
                return;

            Flash();
            _extraAttackCount = Amount;
            await PowerCmd.ModifyAmount(choiceContext, scarletSwordPower, -2M, Owner, null);
            return;
        }

        if (_extraAttackCount <= 0)
            return;
        // 先消耗额外攻击次数，避免递归触发
        _extraAttackCount--;
        await ExecuteExtraAttack(choiceContext);
    }
    
    private async Task ExecuteExtraAttack(PlayerChoiceContext choiceContext)
    {
        var combatState = Owner.CombatState;
        if (combatState == null)
            return;
        var enemies = combatState.HittableEnemies.ToList();
        if (enemies.Count == 0)
            return;
        var enemy = combatState.RunState.Rng.CombatTargets.NextItem(enemies);
        if (enemy == null)
            return;
        Flash();
        await CreatureCmd.Damage(choiceContext, enemy, 6, ValueProp.Unpowered, Owner, null, null);
    }


    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner))
            return Task.CompletedTask;

        _firstAttackCheckedThisTurn = false;
        _extraAttackCount = 0;

        return Task.CompletedTask;
    }
}