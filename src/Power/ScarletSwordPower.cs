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
public class ScarletSwordPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: "res://ArtWorks/images/power/ScarletSwordPower_Small.png",
            BigIconPath: "res://ArtWorks/images/power/ScarletSwordPower_Big.png"
        );

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
        if (card.Owner.Creature != Owner)
            return;

        if (!card.Keywords.Contains(LinkKeywords.Link))
            return;

        await Damage(choiceContext, card);
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power != this || amount <= 0m)
            return Task.CompletedTask;

        if (Amount > 8m)
            SetAmount(8);

        return Task.CompletedTask;
    }

    public async Task ConsumeAsync(PlayerChoiceContext ctx, decimal count, Creature? applier, CardModel? cardSource)
    {
        var used = Math.Min(Amount, Math.Max(0m, count));
        if (used <= 0m)
            return;
        await PowerCmd.ModifyAmount(ctx, this, -used, applier ?? Owner, cardSource);
    }


    public async Task Damage(PlayerChoiceContext choiceContext, CardModel? card)
    {
        var enemy = CombatState.HittableEnemies.MaxBy(t => t.CurrentHp);
        if (enemy == null)
            return;

        var damage = 6m;
        if (Owner.GetPower<ScarletTemperingPower>() is { } temperingPower)
        {
            damage += temperingPower.GetBonusDamage();
        }

        // 赤剑共鸣：获得格挡
        if (Owner.GetPower<ScarletResonancePower>() is { } resonancePower)
        {
            await resonancePower.OnScarletSwordAttack();
        }

        // 绯红连锁：对目标施加易伤，并获得增伤效果
        if (Owner.GetPower<CrimsonChainPower>() is { } crimsonChainPower)
        {
            await crimsonChainPower.OnScarletSwordAttack(choiceContext, enemy);
        }

        // 赤剑阵：随机施加debuff
        if (Owner.GetPower<ScarletArrayPower>() is { } arrayPower)
        {
            await arrayPower.OnScarletSwordAttack(choiceContext, enemy);
        }

        Flash();
        await CreatureCmd.Damage(choiceContext, enemy, damage, ValueProp.Unpowered, Owner, card, null);
        await PowerCmd.Decrement(this);
    }
}