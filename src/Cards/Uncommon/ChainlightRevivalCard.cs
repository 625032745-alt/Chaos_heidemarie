using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using ChaosHeidemarie.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ChainlightRevivalCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("ChainlightRevival", 1)];

    private static readonly LocString SelectFromExhaust =
        new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_EXHAUST_TO_HAND");

    public ChainlightRevivalCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SelectFromExhaustPile(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ChainlightRevival"].UpgradeValueBy(1);
    }

    private async Task SelectFromExhaustPile(PlayerChoiceContext ctx, Player player)
    {
        var selectCount = DynamicVars["ChainlightRevival"].IntValue;
        var selected = await CardSelectionHelper.SelectFromPile(
            ctx, player, PileType.Exhaust, SelectFromExhaust, selectCount);
        foreach (var card in selected)
        {
            await CardPileCmd.Add(card, PileType.Hand);
            if (card.Keywords.Contains(LinkKeywords.Link))
            {
                await CommonUtils.AddOrModifyPower<InherentMemoryPower>(ctx, Owner.Creature, 2m, this);
            }
        }
    }
}