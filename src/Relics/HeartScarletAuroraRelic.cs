using ChaosHeidemarie.Cards.Quest;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Relics;

[RegisterRelic(typeof(HeidemarieRelicPool))]
public class HeartScarletAuroraRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/relic/HeartScarletAuroraRelic_Small.png",
        IconOutlinePath: "res://ArtWorks/images/relic/HeartScarletAuroraRelic_Outline.png",
        BigIconPath: "res://ArtWorks/images/relic/HeartScarletAuroraRelic_Big.png");

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromCard<EffulgentCompressionCard>()];

    public override async Task BeforeCombatStart()
    {
        var card = Owner.RunState.CreateCard<EffulgentCompressionCard>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Hand));
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner.Creature))
            return;
        await PowerCmd.Apply<ScarletSwordPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 5m, Owner.Creature,
            null);
    }
}