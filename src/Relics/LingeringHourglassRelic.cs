using ChaosHeidemarie.Content;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Relics;

[RegisterRelic(typeof(HeidemarieRelicPool))]
public sealed class LingeringHourglassRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath:
        "res://ArtWorks/images/relic/LingeringHourglassRelic_Small.png",
        IconOutlinePath:
        "res://ArtWorks/images/relic/LingeringHourglassRelic_Outline.png",
        BigIconPath:
        "res://ArtWorks/images/relic/LingeringHourglassRelic_Big.png"
    );

    private const int MaxTriggersPerTurn = 3;
    private const decimal BlockAmount = 2m;

    // 当前回合已触发次数
    private int _triggerCount;

    public override Task BeforeCombatStart()
    {
        _triggerCount = 0;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只处理遗物持有者打出的牌
        if (cardPlay.Player.Creature != Owner.Creature)
            return;

        // 自动打出的牌不计入
        if (cardPlay.IsAutoPlay)
            return;

        // 同一组出牌事件只触发一次
        if (!cardPlay.IsLastInSeries)
            return;

        // 每回合最多触发3次
        if (_triggerCount >= MaxTriggersPerTurn)
            return;

        // 只处理当前费用为0的牌
        if (cardPlay.Card.EnergyCost.Canonical == 0 && !cardPlay.Card.EnergyCost.CostsX)
        {
            _triggerCount++;
            Flash();
            // 获得2点格挡
            await CreatureCmd.GainBlock(Owner.Creature, BlockAmount, ValueProp.Unpowered, null);
        }
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner.Creature))
            return Task.CompletedTask;

        _triggerCount = 0;

        return Task.CompletedTask;
    }
}