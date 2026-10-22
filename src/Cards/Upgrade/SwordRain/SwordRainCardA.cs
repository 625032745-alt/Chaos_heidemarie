using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Cards.Token;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.SwordRain;

[RegisterCard(typeof(HeidemarieCardPool))]
public class SwordRainCardA : ModCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13, ValueProp.Move), ModCardVars.Computed("Repeat",
            static ctx =>
            {
                if (!ctx.HasPlayer || ctx.Player.PlayerCombatState is not { } pcs)
                    return ctx.BaseValue;
                var count = pcs.Hand.Cards.Count(c => c != ctx.Card && c is EffulgentBladeCard);
                return ctx.BaseValue + count;
            }, 1m)
    ];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/SwordRainCard.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link, RestKeyword.REST];

    public SwordRainCardA() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount((int)DynamicVars.EvaluateValueOrDefault("Repeat", 1m, cardPlay.Target))
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this)
            return;
        await CardGenerationHelper.GenerateEffulgentBlades(Owner, 1);
    }
}