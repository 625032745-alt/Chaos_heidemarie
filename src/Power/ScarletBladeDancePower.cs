using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class ScarletBladeDancePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/ScarletBladeDancePower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/ScarletBladeDancePower_Big.png"
    );

    public async Task<Creature?> TrySelectTargetAsync(IReadOnlyList<Creature> candidates)
    {
        if (Amount <= 0m || candidates.Count == 0) return null;

        var hittableEnemies = CombatState.HittableEnemies;
        var target = hittableEnemies.MinBy(c => c.CurrentHp);

        if (target == null) return null;
        Flash();
        await PowerCmd.Decrement(this);
        return target;
    }


    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;
        if (!participants.Contains(Owner))
            return;
        await PowerCmd.Remove(this);
    }
}