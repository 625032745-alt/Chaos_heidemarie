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
public class ChainSigilPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/ChainSigilPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/ChainSigilPower_Big.png"
    );

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        var inherentMemory = Owner.GetPower<InherentMemoryPower>();

        if (inherentMemory == null || inherentMemory.Amount < 5)
            return;

        // 随机选择一个敌人
        var enemy = combatState.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
        if(enemy == null) return;

        // 造成等于链印层数的伤害
        await CreatureCmd.Damage(choiceContext, enemy, Amount, ValueProp.Unpowered, Owner, null, null);
    }
}