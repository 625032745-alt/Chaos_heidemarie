using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class ChainResonancePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Type != CardType.Attack)
            return 0;
        var power = Owner.GetPower<InherentMemoryPower>();
        var baseValue = DynamicVars["ChainResonance"].BaseValue;
        if (power != null && power.Amount >= baseValue)
        {
            return 4;
        }

        return 0;
    }
}