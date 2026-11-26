using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class InherentMemoryPower : ModPowerTemplate
{
    private const decimal BaseMaxAmount = 10m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/InherentMemoryPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/InherentMemoryPower_Big.png"
    );

    private int _lastLinkTriggerTurn = -1;

    // 上限 = 10 + 极昼层数 + 极大遗物 2 层。首次施加时 Owner 还是 null，所以按参数里的生物去算。
    private static decimal GetMaxAmountFor(Creature creature)
        => BaseMaxAmount
           + (creature.GetPower<PolarDayPower>()?.Amount ?? 0m)
           + (creature.Player?.GetRelic<InherentMemoryRelic>() != null ? 2m : 0m);

    private decimal GetMaxAmount() => Owner == null ? BaseMaxAmount : GetMaxAmountFor(Owner);

    private decimal GetRemainingAmount() => Math.Max(0m, GetMaxAmount() - Amount);

    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount,
        Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;

        // 只处理本能力、只处理加层；负增量（掉层、移除前扣减）直接放行。
        if (canonicalPower is not InherentMemoryPower || amount <= 0m)
            return false;

        // 只限制挂在自己身上的这一份；首次施加时 Owner 为 null。
        if (Owner != null && !ReferenceEquals(Owner, target))
            return false;

        var current = ReferenceEquals(Owner, target) ? Amount : 0m;
        var remaining = Math.Max(0m, GetMaxAmountFor(target) - current);
        if (amount <= remaining)
            return false;

        modifiedAmount = remaining;
        return true;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner != null)
        {
            var max = GetMaxAmount();
            if (Amount > max)
                SetAmount((int)max, true);
        }

        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not PolarDayPower || !ReferenceEquals(power.Owner, Owner) || amount >= 0m)
            return Task.CompletedTask;

        var max = GetMaxAmount();
        if (Amount > max)
            SetAmount((int)max, true);

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplyPower(choiceContext, cardPlay.Card);
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        await ApplyPower(choiceContext, card);
    }

    private async Task ApplyPower(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (Owner.Player == null || card.Owner != Owner.Player)
            return;
        if (!card.Keywords.Contains(LinkKeywords.Link))
            return;
        if (GetRemainingAmount() <= 0m)
            return;

        var player = Owner.Player;
        var turnNumber = player.PlayerCombatState?.TurnNumber ?? -1;
        if (turnNumber < 0 || _lastLinkTriggerTurn == turnNumber)
            return;

        _lastLinkTriggerTurn = turnNumber;
        Flash();
        await PowerCmd.ModifyAmount(choiceContext, this, 1m, Owner, card);
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != Owner)
            return 0m;
        if (!props.IsPoweredAttack())
            return 0m;

        return Amount;
    }

    public async Task ConsumeAsync(PlayerChoiceContext ctx, decimal count,
        Creature? applier, CardModel? cardSource)
    {
        var used = Math.Min(Amount, Math.Max(0m, count));
        if (used <= 0m)
            return;
        // 扣到 0 时框架会自己 Remove（ShouldRemoveDueToAmount），别手动 Remove
        await PowerCmd.ModifyAmount(ctx, this, -used, applier ?? Owner, cardSource);
    }
}