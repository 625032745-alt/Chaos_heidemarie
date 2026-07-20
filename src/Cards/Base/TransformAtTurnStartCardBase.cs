using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Base;

public abstract class TransformAtTurnStartCardBase : ModCardTemplate
{
    
    private bool _pendingUpgrade;

    protected TransformAtTurnStartCardBase(int baseCost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true) : base(baseCost, type, rarity, target, showInCardLibrary)
    {
    }

    [SavedProperty]
    public bool PendingUpgrade
    {
        get => _pendingUpgrade;
        set
        {
            AssertMutable();
            _pendingUpgrade = value;
        }
    }
    
    protected abstract Type[] GetCandidateCardTypes();
    
    public override Task AfterCombatEnd(CombatRoom room)
    {
        PendingUpgrade = true;
        return Task.CompletedTask;
    }
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!PendingUpgrade) return;
        PendingUpgrade = false;
        // 2. 获取随机数生成器
        var rng = Owner.RunState.Rng.CombatCardSelection;
        var selectedTypes = GetCandidateCardTypes().OrderBy(_ => rng.NextFloat()).Take(4).ToList();
        // 4. 创建实际的卡牌实例
        var createCardMethod = typeof(ICombatState).GetMethod("CreateCard", [typeof(Player)]);
        var candidateCards = new List<CardModel>();

        foreach (var type in selectedTypes)
        {
            var genericMethod = createCardMethod.MakeGenericMethod(type);
            var card = (CardModel)genericMethod.Invoke(CombatState, [Owner]);
            candidateCards.Add(card);
        }

        // 5. 让玩家选择一张卡牌
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 0, 1)
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };
        var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, candidateCards, Owner, prefs);
        var picked = selected.FirstOrDefault();
        // 6. 如果玩家选择了卡牌，进行变换
        if (picked != null)
        {
            await CardCmd.Transform(this, picked);
        }
    }
}