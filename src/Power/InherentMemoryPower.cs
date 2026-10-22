using ChaosHeidemarie.Keywords;
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

    // 上限 = 10 + 极昼层数。首次施加时 Owner 还是 null，所以按参数里的生物去算。
    private static decimal GetMaxAmountFor(Creature creature)
        => BaseMaxAmount + (creature.GetPower<PolarDayPower>()?.Amount ?? 0m);

    private decimal GetMaxAmount()
        => Owner == null ? BaseMaxAmount : GetMaxAmountFor(Owner);

    private decimal GetRemainingAmount()
        => Math.Max(0m, GetMaxAmount() - Amount);

    /// <summary>
    /// 拦截所有外来加层（遗物、卡牌、药水、其它能力都走这里）。
    /// Apply 与 ModifyAmount 两条路径都会调用，所以不再依赖 AddAmountWithLimit。
    /// </summary>
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

    /// <summary>
    /// 首次施加时本能力还没进监听列表，上面那个钩子不生效，这里补一次钳制。
    /// </summary>
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

    /// <summary>
    /// 极昼掉层后上限变小，已有层数跟着回落。
    /// </summary>
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not PolarDayPower || !ReferenceEquals(power.Owner, Owner) || amount >= 0m)
            return;

        var max = GetMaxAmount();
        if (Amount > max)
            await PowerCmd.ModifyAmount(choiceContext, this, max - Amount, Owner, null);
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        await ApplyPower(choiceContext, card);
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        await ApplyPower(choiceContext, card);
    }

    private async Task ApplyPower(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card.Owner != Owner.Player) return;
        if (!card.Keywords.Contains(LinkKeywords.Link)) return;
        if (GetRemainingAmount() <= 0m) return;

        Flash();
        // 上限由 TryModifyPowerAmountReceived 统一钳制，这里直接加 1 层即可。
        await PowerCmd.ModifyAmount(choiceContext, this, 1m, Owner, card);
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!props.IsPoweredAttack())
            return 0m;
        if (dealer != Owner)
            return 0m;
        if (cardSource == null)
            return 0m;

        return Amount * 2m;
    }
}