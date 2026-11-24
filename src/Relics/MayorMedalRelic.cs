using ChaosHeidemarie.Cards.Curse;
using ChaosHeidemarie.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Relics;

[RegisterRelic(typeof(HeidemarieRelicPool))]
public class MayorMedalRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override bool HasUponPickupEffect => true;
    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/relic/MayorMedalRelic_Small.png",
        IconOutlinePath: "res://ArtWorks/images/relic/MayorMedalRelic_Outline.png",
        BigIconPath: "res://ArtWorks/images/relic/MayorMedalRelic_Big.png");

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<RemorseCard>()];

    public override async Task AfterObtained()
    {
        var card = Owner.RunState.CreateCard(ModelDb.Card<RemorseCard>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
    }
}