using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class SunderedGlowCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];

    private static readonly LocString SelectFromHand =
        new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_HAND_TO_DISCARD");

    public SunderedGlowCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await SelectFromHandPile(choiceContext, Owner, cardPlay);
    }

    private async Task SelectFromHandPile(PlayerChoiceContext ctx, Player player, CardPlay cardPlay)
    {
        var pile = PileType.Hand.GetPile(player);
        var selectFrom = (from c in pile.Cards orderby c.Rarity, c.Id select c).ToList();
        var selected =
            await CardSelectCmd.FromSimpleGrid(ctx, selectFrom, player, new CardSelectorPrefs(SelectFromHand, 1));
        var picked = selected.FirstOrDefault();
        if (picked != null)
        {
            await CardCmd.Discard(ctx, picked);
            if (picked.Keywords.Contains(LinkKeywords.Link))
            {
                await DamageCmd.Attack(6)
                    .FromCard(this, cardPlay)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(ctx);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}