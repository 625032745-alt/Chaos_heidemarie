using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class DawnPierceCard : ModCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public DawnPierceCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var powerAmount = Owner.Creature.GetPowerAmount<InherentMemoryPower>();
        if (powerAmount >= 5)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", 0.1f);
            await using AttackContext context =
                await AttackCommand.CreateContextAsync(CombatState, choiceContext, cardPlay);
            var hits = (await CreatureCmd.Damage(choiceContext, cardPlay.Target,
                DynamicVars.Damage.BaseValue,
                ValueProp.Move | ValueProp.Unblockable, this, cardPlay)).ToList();
            context.AddHit(hits);
        }
        else
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}