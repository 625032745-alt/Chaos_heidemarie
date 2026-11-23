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

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/ScarletSwordPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/ScarletSwordPower_Big.png"
    );

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
        if (card.Owner.Creature != Owner) return;
        if (card.Keywords.Contains(LinkKeywords.Link))
        {
            var hittableEnemies = CombatState.HittableEnemies;
            if (hittableEnemies.Count <= 0) return;
            var enemy = CombatState.RunState.Rng.CombatTargets.NextItem(hittableEnemies);
            if (enemy == null) return;
            Flash();
            var damage = 6m;
            if (Owner.GetPower<ScarletTemperingPower>() is { } temperingPower)
            {
                damage += temperingPower.GetBonusDamage();
            }
            await CreatureCmd.Damage(choiceContext, enemy, damage, ValueProp.Unpowered, Owner, card, null);
            if (Owner.GetPower<ScarletResonancePower>() is { } resonancePower)
            {
                await resonancePower.OnScarletSwordAttack();
            }

            if (Owner.GetPower<BladesAfterglowPower>() is { } bladesAfterglowPower)
            {
                await bladesAfterglowPower.OnScarletSwordAttack(choiceContext);
            }

            if (Owner.GetPower<CrimsonChainPower>() is { } crimsonChainPower)
            {
                await crimsonChainPower.OnScarletSwordAttack(choiceContext, enemy);
            }

            if (Owner.GetPower<ScarletArrayPower>() is { } scarletArrayPower)
            {
                await scarletArrayPower.OnScarletSwordAttack(choiceContext);
            }

            await PowerCmd.Decrement(this);
        }
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power != this) return Task.CompletedTask;
        if (power is not ScarletSwordPower || amount <= 0)
            return Task.CompletedTask;
        if (power.Amount > 8)
        {
            SetAmount(8);
        }

        return Task.CompletedTask;
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