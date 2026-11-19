using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Rare;

[RegisterCard(typeof(HeidemarieCardPool))]
public class DawnlightJudgmentCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new("JudgmentLoss", 5)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    public DawnlightJudgmentCard() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IReadOnlyList<Creature> enemies = CombatState.HittableEnemies;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .SpawningHitVfxOnEachCreature()
            .Execute(choiceContext);
        var power = Owner.Creature.GetPower<InherentMemoryPower>();
        if (power != null)
        {
            await power.ConsumeAsync(choiceContext, DynamicVars["JudgmentLoss"].BaseValue, Owner.Creature, this);
            foreach (Creature enemy in enemies)
            {
                await PowerCmd.Apply<DyingStarPower>(choiceContext, enemy, DynamicVars["JudgmentLoss"].BaseValue,
                    Owner.Creature, this);
                VfxCmd.PlayOnCreature(enemy, "vfx/vfx_attack_slash");
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["JudgmentLoss"].UpgradeValueBy(1);
    }
}