using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ChaosHeidemarie.Cards.Base;

public abstract class TransformAtTurnStartCardBase : TemporaryLinkCardBase
{
    private bool _pendingUpgrade;

    protected TransformAtTurnStartCardBase(int baseCost, CardType type, CardRarity rarity, TargetType target,
        bool showInCardLibrary = true) : base(baseCost, type, rarity, target, showInCardLibrary)
    {
    }

    [SavedProperty]
    public bool PendingUpgrade
    {
        get => _pendingUpgrade;
        set
        {
            if (!IsMutable)
            {
                return;
            }

            _pendingUpgrade = value;
        }
    }

    protected abstract IReadOnlyList<Func<ICombatState, Player, CardModel>> CandidateCardFactories { get; }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        // 先给父类清掉临时连接，再写标记（父类实现不可漏调）
        await base.AfterCombatEnd(room);

        if (IsMutable)
        {
            PendingUpgrade = true;
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        ICombatState? combatState = CombatState;
        if (combatState == null || player != Owner || !PendingUpgrade)
        {
            return;
        }

        List<CardModel> candidates = BuildCandidates(combatState, player);
        if (candidates.Count == 0)
        {
            // 没有候选就别留着标记，否则每回合空转
            PendingUpgrade = false;
            return;
        }


        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 0, 1);
        IEnumerable<CardModel> selected =
            await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, player, prefs);
        CardModel? picked = selected.FirstOrDefault();

        // 取消 / 点确认没选：清标记，避免下回合再弹
        PendingUpgrade = false;
        if (picked == null || picked == this)
        {
            return;
        }

        if (IsTransformable && Pile != null)
        {
            await CardCmd.Transform(this, picked);
        }
    }

    private List<CardModel> BuildCandidates(ICombatState combatState, Player player)
    {
        List<CardModel> result = new List<CardModel>(4);
        HashSet<string> seen = new HashSet<string>();

        foreach (Func<ICombatState, Player, CardModel> factory in CandidateCardFactories)
        {
            CardModel? card;
            try
            {
                card = factory(combatState, player);
            }
            catch (Exception)
            {
                continue;
            }

            if (card.Owner != player)
            {
                continue;
            }

            if (!seen.Add(card.Id.Entry))
            {
                continue;
            }

            result.Add(card);
            if (result.Count >= 4)
            {
                break;
            }
        }

        return result;
    }
}