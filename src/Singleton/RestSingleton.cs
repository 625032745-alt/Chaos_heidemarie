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
public class RestSingleton : HookedSingletonModel
{
    public RestSingleton() : base(HookType.Combat)
    {
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (!card.Keywords.Contains(RestKeyword.REST))
            return;
        await CardCmd.AutoPlay(choiceContext, card, null);
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel clonedBy)
    {
        if (!card.Keywords.Contains(RestKeyword.REST))
            return;
        
        if (oldPileType != PileType.Discard)
            return;

        var currentPile = card.Pile?.Type;
        
        if (currentPile != PileType.Hand)
            return;
        await CardPileCmd.Add(card, PileType.Discard);
    }
}