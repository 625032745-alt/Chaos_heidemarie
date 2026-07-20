using System.Linq;
using System.Threading.Tasks;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ChaosHeidemarie.Singleton;

[RegisterSingleton]
public class RecycleSingleton : HookedSingletonModel
{
    public RecycleSingleton() : base(HookType.Combat)
    {
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (!card.Keywords.Contains(RecycleKeywords.Recycle))
            return;
        var player = card.Owner;
        var pile = player.Piles.FirstOrDefault(p => p.Cards.Contains(card));
        if (pile != null && (pile.Type == PileType.Discard || pile.Type == PileType.Exhaust))
        {
            var pileCards = pile.Cards;
            foreach (var pileCard in pileCards)
            {
                await CardPileCmd.Add(pileCard, PileType.Hand);
            }
        }
    }
}