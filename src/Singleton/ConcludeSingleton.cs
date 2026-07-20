using System.Threading.Tasks;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ChaosHeidemarie.Singleton;

[RegisterSingleton]
public class ConcludeSingleton : HookedSingletonModel
{
    public ConcludeSingleton() : base(HookType.Combat)
    {
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!card.Keywords.Contains(ConcludeKeywords.Conclude))
            return Task.CompletedTask;
        var player = card.Owner;
        PlayerCmd.EndTurn(player, false);
        return Task.CompletedTask;
    }
}