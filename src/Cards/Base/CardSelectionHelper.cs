using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace ChaosHeidemarie.Cards.Base;

internal static class CardSelectionHelper
{
    public static async Task<IReadOnlyList<CardModel>> SelectFromPile(PlayerChoiceContext context,
        Player player, PileType pileType, LocString prompt, int count, int minCount,
        Func<CardModel, bool>? predicate = null)
    {
        var candidates = pileType.GetPile(player).Cards
            .Where(predicate ?? (_ => true))
            .OrderBy(card => card.Rarity)
            .ThenBy(card => card.Id)
            .ToList();
        var selectCount = Math.Min(count, candidates.Count);
        if (selectCount <= 0)
            return Array.Empty<CardModel>();

        var prefs = new CardSelectorPrefs(prompt, minCount, count)
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };
        var selected = await CardSelectCmd.FromSimpleGrid(context, candidates, player, prefs);
        return selected.ToList();
    }
}