using ChaosHeidemarie.Content;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Relics;

[RegisterRelic(typeof(HeidemarieRelicPool))]
public sealed class CrackedSilverCoinRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath:
        "res://ArtWorks/images/relic/CrackedSilverCoinRelic_Small.png",
        IconOutlinePath:
        "res://ArtWorks/images/relic/CrackedSilverCoinRelic_Outline.png",
        BigIconPath:
        "res://ArtWorks/images/relic/CrackedSilverCoinRelic_Big.png"
    );

    private const int RequiredCardCount = 5;
    
    private int _cardsPlayedThisTurn;

    public override Task BeforeCombatStart()
    {
        _cardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只统计遗物持有者打出的牌
        if (cardPlay.Player != Owner)
            return;

        // 不统计自动打出的牌
        if (cardPlay.IsAutoPlay)
            return;

        // 同一组出牌事件只统计一次
        if (!cardPlay.IsLastInSeries)
            return;

        // 本回合已经达到5张，不再计数
        if (_cardsPlayedThisTurn >= RequiredCardCount)
            return;

        _cardsPlayedThisTurn++;

        // 只有第5张牌触发
        if (_cardsPlayedThisTurn != RequiredCardCount)
            return;

        Flash();
        await CardPileCmd.Draw(choiceContext, Owner);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner.Creature))
            return Task.CompletedTask;
        
        _cardsPlayedThisTurn = 0;

        return Task.CompletedTask;
    }
}