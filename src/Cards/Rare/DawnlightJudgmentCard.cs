using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_starry_impact")
            .SpawningHitVfxOnEachCreature()
            .Execute(choiceContext);
        var loss = DynamicVars["JudgmentLoss"].BaseValue;
        var used = 0m;
        var memory = Owner.Creature.GetPower<InherentMemoryPower>();
        if (memory != null)
        {
            used = Math.Min(memory.Amount, loss);
            if (used > 0m)
                await memory.ConsumeAsync(choiceContext, used, Owner.Creature, this);
        }

        if (used <= 0m)
            return;

        foreach (Creature enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<DawnlightJudgmentPower>(choiceContext, enemy, used, Owner.Creature, this);
            VfxCmd.PlayOnCreature(enemy, "vfx/vfx_attack_slash");
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["JudgmentLoss"].UpgradeValueBy(1);
    }
}