using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ChaosHeidemarie.Utils;

public class CommonUtils
{
    public static async Task AddOrModifyPower<TPower>(
        PlayerChoiceContext ctx,
        Creature creature,
        decimal amount,
        CardModel source)
        where TPower : PowerModel
    {
        var power = creature.GetPower<TPower>();
        if (power != null)
        {
            await PowerCmd.ModifyAmount(ctx, power, amount, null, source);
        }
        else
        {
            await PowerCmd.Apply<TPower>(ctx, creature, amount, creature, source);
        }
    }
}