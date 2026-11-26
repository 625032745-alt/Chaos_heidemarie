using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.CardSelection;
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
            ctx, player, PileType.Draw, CardSelectorPrefs.DiscardSelectionPrompt, selectCount);
        foreach (var card in selected)
        {
            await CardCmd.Discard(ctx, card);
            await PowerCmd.Apply<InherentMemoryPower>(ctx, Owner.Creature, 1m, Owner.Creature, this);
        }
    }
}