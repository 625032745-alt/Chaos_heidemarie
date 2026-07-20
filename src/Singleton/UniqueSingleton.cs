using System.Linq;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ChaosHeidemarie.Singleton;

[RegisterSingleton]
public class UniqueSingleton : HookedSingletonModel
{
    public UniqueSingleton() : base(HookType.Combat)
    {
    }

    public override bool ShouldAddToDeck(CardModel card)
    {
        if (!card.Keywords.Contains(UniqueKeyword.Unique))
            return true;
        if (card == null || card.Id != card.Id)
            return true;

        var owner = card.Owner;
        return owner.Piles.SelectMany(p => p.Cards).All(c => c.Id.Entry != card.Id.Entry);
    }
}