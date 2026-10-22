using ChaosHeidemarie.Cards.Token;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.SwordRain;

[RegisterCard(typeof(HeidemarieCardPool))]
public class SwordRainCardD : ModCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage("BladeDamage",
            static ctx =>
            {
                var baseValue = ctx.BaseValue;
                if (!ctx.HasPlayer || ctx.Player.PlayerCombatState is not { } pcs)
                    return ctx.BaseValue;
                var count = pcs.ExhaustPile.Cards.Count(c => c is EffulgentBladeCard);
                return baseValue + count * 3;
            }, 30m)
    ];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/SwordRainCard.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust, CardKeyword.Retain, ConcludeKeywords.Conclude];

    public SwordRainCardD() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var player = card.Owner;
        var playerCombatState = player.PlayerCombatState;
        var cards = playerCombatState.Hand.Cards.Where(c => c is LiberationAuroraCard).ToList();
        foreach (var cardModel in cards)
        {
            await CardCmd.Exhaust(choiceContext, cardModel);
        }

        await DamageCmd.Attack(DynamicVars.EvaluateValueOrDefault("BladeDamage", 30m, cardPlay.Target))
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }
}