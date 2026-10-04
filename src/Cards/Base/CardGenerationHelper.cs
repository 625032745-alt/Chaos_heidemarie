using ChaosHeidemarie.Cards.Token;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace ChaosHeidemarie.Cards.Base;

internal static class CardGenerationHelper
{
    public static async Task GenerateEffulgentBlades(CardModel source, int count,
        PileType pileType = PileType.Hand, CardKeyword? keyword = null)
    {
        var combatState = source.CombatState;
        if (combatState == null || count <= 0)
            return;

        var player = source.Owner;
        // Keep generation sequential: adding a card can trigger combat hooks.
        for (var i = 0; i < count; i++)
        {
            var card = combatState.CreateCard<EffulgentBladeCard>(player);
            if (keyword is { } extraKeyword)
                card.AddKeyword(extraKeyword);

            await CardPileCmd.AddGeneratedCardToCombat(card, pileType, player);
        }
    }
}
