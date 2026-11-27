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


    public async Task Damage(PlayerChoiceContext choiceContext, CardModel? card, bool isExtraAttack = false)
    {
        if (!isExtraAttack && Amount <= 0m)
            return;

        var combatState = Owner.CombatState;
        if (combatState == null)
            return;

        var hittableEnemies = combatState.HittableEnemies;
        if (hittableEnemies.Count == 0)
            return;

        var enemy = combatState.RunState.Rng.CombatTargets.NextItem(hittableEnemies);
        var godForm = Owner.HasPower<SwordGodFormPower>();
        if (!godForm && Owner.GetPower<ScarletBladeDancePower>() is { Amount: > 0 } dancePower)
        {
            enemy = await dancePower.TrySelectTargetAsync(hittableEnemies);
        }

        var damage = 6m;
        if (Owner.GetPower<ScarletTemperingPower>() is { } temperingPower)
        {
            damage += temperingPower.GetBonusDamage();
        }

        Flash();
        if (godForm)
        {
            await CreatureCmd.Damage(choiceContext, hittableEnemies, damage, ValueProp.Unpowered, Owner, card, null);
        }
        else
        {
            if (enemy == null)
                return;

            await CreatureCmd.Damage(choiceContext, enemy, damage, ValueProp.Unpowered, Owner, card, null);
        }

        // 赤剑共鸣：获得格挡
        if (Owner.GetPower<ScarletResonancePower>() is { } resonancePower)
        {
            await resonancePower.OnScarletSwordAttack();
        }

        // 万辉：累计赤光剑攻击次数
        if (Owner.GetPower<BladesAfterglowPower>() is { } afterglowPower)
        {
            await afterglowPower.OnScarletSwordAttack(choiceContext);
        }

        // 绯红连锁：对目标施加易伤，并获得增伤效果
        if (enemy != null && Owner.GetPower<CrimsonChainPower>() is { } crimsonChainPower)
        {
            await crimsonChainPower.OnScarletSwordAttack(choiceContext, enemy);
        }

        // 赤剑阵追加攻击不消耗赤光剑，
        // 也不能再次触发赤剑阵。
        if (isExtraAttack)
            return;

        await PowerCmd.Decrement(this);

        if (Owner.GetPower<ScarletArrayPower>() is { } arrayPower)
        {
            await arrayPower.OnScarletSwordAttack(choiceContext, this, card);
        }
    }
}