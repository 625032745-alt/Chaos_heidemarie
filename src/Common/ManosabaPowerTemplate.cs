using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Common;

public abstract class ManosabaPowerTemplate : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => PowerAssetProfile.Empty;

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power.Amount < 0 && !power.AllowNegative)
            power.RemoveInternal();

        return Task.CompletedTask;
    }
}
