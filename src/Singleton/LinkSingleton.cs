using System.Linq;
using System.Threading.Tasks;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace Heidemarie.Core.Models.Singleton;

[RegisterSingleton]
public class LinkSingleton : HookedSingletonModel
{
    public LinkSingleton() : base(HookType.Combat)
    {
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!card.Keywords.Contains(LinkKeywords.Link))
            return;
        var player = card.Owner;
        var combatState = player.PlayerCombatState;
        if (combatState == null)
            return;
        var linkCardsInHand = combatState.Hand.Cards
            .Where(c => c != card && c.Keywords.Contains(LinkKeywords.Link))
            .ToList();
        if (linkCardsInHand.Count == 0)
            return;

        await CardCmd.Discard(choiceContext, linkCardsInHand);
    }
}