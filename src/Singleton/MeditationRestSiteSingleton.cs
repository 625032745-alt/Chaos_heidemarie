using ChaosHeidemarie.RestSite;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ChaosHeidemarie.Singleton;

[RegisterSingleton]
public class MeditationRestSiteSingleton : HookedSingletonModel
{
    public MeditationRestSiteSingleton() : base(HookType.Run)
    {
    }

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        options.Add(new MeditationRestSiteOption(player));
        return true;
    }
}