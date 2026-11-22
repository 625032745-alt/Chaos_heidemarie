using ChaosHeidemarie.Keywords;
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
public class RadiantResonancePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/RadiantResonancePower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/RadiantResonancePower_Big.png"
    );

    // 记录本回合是否已触发
    private int _lastTriggerTurn = -1;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        // 只增加攻击伤害
        if (!props.IsPoweredAttack())
            return 0m;

        // 必须是自己造成的伤害
        if (dealer != Owner)
            return 0m;

        if (cardSource == null || cardPlay == null)
            return 0m;

        // 必须是攻击牌
        if (cardSource.Type != CardType.Attack)
            return 0m;

        // 必须带有连接关键词
        if (!cardSource.Keywords.Contains(LinkKeywords.Link))
            return 0m;

        // 必须是当前打出的这张牌
        if (!ReferenceEquals(cardPlay.Card, cardSource))
            return 0m;

        var combatState = Owner.Player?.PlayerCombatState;
        if (combatState == null)
            return 0m;

        // 本回合已经触发过
        if (combatState.TurnNumber == _lastTriggerTurn)
            return 0m;

        // 增加伤害，不在这里修改触发状态
        return Amount;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;

        if (Owner.Player == null || card.Owner != Owner.Player)
            return Task.CompletedTask;

        // 只记录连接攻击牌
        if (card.Type != CardType.Attack || !card.Keywords.Contains(LinkKeywords.Link))
            return Task.CompletedTask;

        var combatState = Owner.Player.PlayerCombatState;
        if (combatState == null)
            return Task.CompletedTask;

        if (_lastTriggerTurn == combatState.TurnNumber)
            return Task.CompletedTask;

        // 当前连接攻击牌已经结算
        _lastTriggerTurn = combatState.TurnNumber;

        Flash();
        return Task.CompletedTask;
    }
}