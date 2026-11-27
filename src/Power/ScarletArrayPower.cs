using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
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


    public async Task OnScarletSwordAttack(PlayerChoiceContext choiceContext, Creature target)
    {
        Flash();
        var effect = CombatState.RunState.Rng.CombatTargets.NextInt(5);
        switch (effect)
        {
            // 灾厄
            case 0:
                await PowerCmd.Apply<DoomPower>(choiceContext, target, Amount, Owner, null);
                break;

            // 中毒
            case 1:
                await PowerCmd.Apply<PoisonPower>(choiceContext, target, Amount, Owner, null);
                break;

            // 易伤
            case 2:
                await PowerCmd.Apply<VulnerablePower>(choiceContext, target, Amount, Owner, null);
                break;

            // 虚弱
            case 3:
                await PowerCmd.Apply<WeakPower>(choiceContext, target, Amount, Owner, null);
                break;

            // 永久降低力量
            case 4:
                await PowerCmd.Apply<StrengthPower>(choiceContext, target, -1m, Owner, null);
                break;
        }
    }
}