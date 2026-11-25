using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class SwordGodFormPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/SwordGodFormPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/SwordGodFormPower_Big.png"
    );

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner))
            return;
        Flash();
        await PowerCmd.Apply<ScarletSwordPower>(new ThrowingPlayerChoiceContext(), Owner, 1m, Owner, null);
    }
}