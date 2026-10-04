using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Cards.Upgrade.SwordRain;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
[RegisterCharacterStarterCard(typeof(Characters.Heidemarie))]
public class SwordRainCard : TransformAtTurnStartCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move), new("SwordRain", 1)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link];

    public SwordRainCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override void OnUpgrade()
    {
        CardCmd.ApplyKeyword(this, RestKeyword.REST);
        DynamicVars["SwordRain"].UpgradeValueBy(1);
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card != this)
            return;
        var combatState = card.CombatState;
        if (null == combatState)
            return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        var baseValue = DynamicVars["SwordRain"].IntValue;
        await CardGenerationHelper.GenerateEffulgentBlades(this, baseValue);
    }
    
    protected override IReadOnlyList<Func<ICombatState, Player, CardModel>> CandidateCardFactories =>
    [
        static (combatState, player) => combatState.CreateCard<SwordRainCardA>(player),
        static (combatState, player) => combatState.CreateCard<SwordRainCardB>(player),
        static (combatState, player) => combatState.CreateCard<SwordRainCardC>(player),
        static (combatState, player) => combatState.CreateCard<SwordRainCardD>(player)
    ];
}