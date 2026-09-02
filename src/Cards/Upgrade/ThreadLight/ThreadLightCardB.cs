using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.ThreadLight;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ThreadLightCardB : ModCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/ThreadLightCard.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link, RestKeyword.REST];

    public ThreadLightCardB() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        if (Owner.Creature.GetPower<ThreadLightCardBPower>() != null)
            return;
        await PowerCmd.Apply<ThreadLightCardBPower>(choiceContext, Owner.Creature,
            1m, Owner.Creature, this);
    }
}