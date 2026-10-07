using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ChaosHeidemarie.Singleton;

[RegisterSingleton]
public class UniqueSingleton : HookedSingletonModel
{
    public UniqueSingleton() : base(HookType.Run)
    {
    }

    public override bool ShouldAddToDeck(CardModel card)
    {
        if (!card.Keywords.Contains(UniqueKeyword.Unique))
            return true;

        return card.Owner.Deck.Cards.All(c => c == card || c.Id.Entry != card.Id.Entry);
    }
}