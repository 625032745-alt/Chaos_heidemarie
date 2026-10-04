using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Cards.Upgrade.ThreadLight;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ThreadLightCard : TransformAtTurnStartCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10, ValueProp.Move), new("ThreadLight", 3)];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link];

    public ThreadLightCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner.Creature || !props.IsPoweredAttack())
        {
            return 0m;
        }
        if (cardSource != this)
            return 0M;

        var combatState = cardSource.Owner.PlayerCombatState;
        if (combatState == null)
            return 0M;

        var linkCount = combatState.Hand.Cards
            .Count(c => c != cardSource && c.Keywords.Contains(LinkKeywords.Link));
        return linkCount * DynamicVars["ThreadLight"].BaseValue;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["ThreadLight"].UpgradeValueBy(1);
    }
    
    protected override IReadOnlyList<Func<ICombatState, Player, CardModel>> CandidateCardFactories =>
    [
        static (combatState, player) => combatState.CreateCard<ThreadLightCardA>(player),
        static (combatState, player) => combatState.CreateCard<ThreadLightCardB>(player),
        static (combatState, player) => combatState.CreateCard<ThreadLightCardC>(player),
        static (combatState, player) => combatState.CreateCard<ThreadLightCardD>(player)
    ];
}