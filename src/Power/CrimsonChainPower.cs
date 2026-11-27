using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class CrimsonChainPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: "res://ArtWorks/images/power/CrimsonChainPower_Small.png",
            BigIconPath: "res://ArtWorks/images/power/CrimsonChainPower_Big.png");

    public async Task OnScarletSwordAttack(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target == null)
            return;
        Flash();
        await PowerCmd.Apply<VulnerablePower>(choiceContext, target, 2m, Owner, null);
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, Amount, Owner, null);
    }
}