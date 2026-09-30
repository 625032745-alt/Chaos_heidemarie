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
public class ResilientChainPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/ResilientChainPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/ResilientChainPower_Big.png"
    );

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (power is not InherentMemoryPower)
            return;

        // 不是自己的极光连锁
        if (power.Owner != Owner)
            return;

        // amount < 0 代表这次极光连锁减少
        if (amount >= 0)
            return;

        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);
    }
}