using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public sealed class AfterglowPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private int _lastTriggerTurn = -1;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner.Creature != Owner) return playCount;
        if (card.Type != CardType.Attack) return playCount;
        if (card.Keywords.Contains(LinkKeywords.Link)) return playCount;
        if (Owner.GetPower<ScarletSwordPower>() is not { Amount: > 0 }) return playCount;
        if (Owner.Player?.PlayerCombatState is not { } pcs) return playCount;
        if (pcs.TurnNumber == _lastTriggerTurn) return playCount;

        return playCount + Amount;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        _lastTriggerTurn = Owner.Player?.PlayerCombatState?.TurnNumber ?? -1;
        if (Owner.GetPower<ScarletSwordPower>() is not { } sword) return;
        Flash();
        await PowerCmd.Decrement(sword);
    }
}