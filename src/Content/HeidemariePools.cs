using ChaosHeidemarie.Cards;
using ChaosHeidemarie.Cards.Upgrade.HeroAll;
using ChaosHeidemarie.Cards.Upgrade.SwordRain;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Content;

public sealed class HeidemarieCardPool : TypeListCardPoolModel
{
    public override string Title => "Heidemarie";
    public override string EnergyColorName => "heidemarie";
    public override string? BigEnergyIconPath => "res://ArtWorks/images/ui/combat/energy_counters/heidemarie/heidemarie_orb_layer_1.png";
    public override string? TextEnergyIconPath => "res://ArtWorks/images/ui/combat/energy_counters/heidemarie/heidemarie_orb_layer_1.png";
    public override string CardFrameMaterialPath => "card_frame_blue";
    public override Color DeckEntryCardColor => new("7A4F9AFF");
    public override bool IsColorless => false;

    protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards)
    {
        var result = base.FilterThroughEpochs(unlockState, cards);
        var excludedCardTypes = new HashSet<Type>
        {
            typeof(EffulgentBladeCard),
            typeof(LiberationAuroraCard),
            typeof(HeroAllUpgradeCardA),
            typeof(HeroAllUpgradeCardB),
            typeof(HeroAllUpgradeCardC),
            typeof(HeroAllUpgradeCardD),
            typeof(SwordRainCardA),
            typeof(SwordRainCardB),
            typeof(SwordRainCardC),
            typeof(SwordRainCardD)
        };
        return result.Where(card => !excludedCardTypes.Contains(card.GetType())).ToList();
    }
}

public sealed class HeidemarieRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => "heidemarie";
    public override string? BigEnergyIconPath => "res://ArtWorks/images/ui/combat/energy_counters/heidemarie/heidemarie_orb_layer_1.png";
    public override string? TextEnergyIconPath => "res://ArtWorks/images/ui/combat/energy_counters/heidemarie/heidemarie_orb_layer_1.png";
}

public sealed class HeidemariePotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "heidemarie";
    public override string? BigEnergyIconPath => "res://ArtWorks/images/ui/combat/energy_counters/heidemarie/heidemarie_orb_layer_1.png";
    public override string? TextEnergyIconPath => "res://ArtWorks/images/ui/combat/energy_counters/heidemarie/heidemarie_orb_layer_1.png";
}
