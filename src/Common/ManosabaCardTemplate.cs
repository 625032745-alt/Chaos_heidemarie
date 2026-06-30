using MegaCrit.Sts2.Core.Entities.Cards;
using ChaosHeidemarie.RitsuAdapters;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Common;

public abstract class ManosabaCardTemplate(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : ModComponentsCardTemplate(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://ArtWorks/images/cards/card_effects/card_ego_basic.png");
}
