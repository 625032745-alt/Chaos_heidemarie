using ChaosHeidemarie.Cards.Upgrade.ThreadLight;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class ThreadLightCardBPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/ThreadLightPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/ThreadLightPower_Big.png"
    );

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!cardSource.Keywords.Contains(LinkKeywords.Link))
            return 0m;
        var player = cardPlay.Player;
        var combatState = player.PlayerCombatState;
        var discard = combatState.DiscardPile.Cards.Where(c => c is ThreadLightCardB).ToList();
        var hands = combatState.Hand.Cards.Where(c => c is ThreadLightCardB).ToList();
        if (hands.Count == 0 && discard.Count == 0) return 0m;
        return 2m;
    }
}