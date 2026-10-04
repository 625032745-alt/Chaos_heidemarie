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
            AssertMutable();
            _pendingUpgrade = value;
        }
    }

    protected abstract IReadOnlyList<Func<ICombatState, Player, CardModel>> CandidateCardFactories { get; }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        PendingUpgrade = true;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var combatState = CombatState;
        if (!PendingUpgrade || player != Owner || combatState == null)
            return;
        PendingUpgrade = false;
        var rng = Owner.RunState.Rng.CombatCardSelection;
        var candidateCards = CandidateCardFactories
            .OrderBy(_ => rng.NextFloat())
            .Take(4)
            .Select(factory => factory(combatState, Owner))
            .ToList();

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 0, 1)
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };
        var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, candidateCards, Owner, prefs);
        var picked = selected.FirstOrDefault();
        if (picked != null)
        {
            await CardCmd.Transform(this, picked);
        }
    }
}