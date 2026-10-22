using ChaosHeidemarie.Cards.Token;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace ChaosHeidemarie.Cards.Base;

internal static class CardGenerationHelper
{
    public static async Task GenerateEffulgentBlades(Player player, int count,
        PileType pileType = PileType.Hand, CardKeyword? keyword = null)
    {
        if (count <= 0) return;
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        for (var i = 0; i < count; i++)
        {
            var card = combatState.CreateCard<EffulgentBladeCard>(player);
            if (keyword is { } extraKeyword)
                card.AddKeyword(extraKeyword);
            await CardPileCmd.AddGeneratedCardToCombat(card, pileType, player);
        }
    }
}
