using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Component.Core;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ChaosHeidemarie.Tests.Cases;

[RegisterCard(typeof(HeidemarieCardPool))]
public sealed class TestAttack()
    : ManosabaCardTemplate(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, false)
{
    public override CardPoolModel Pool => ModelDb.CardPool<HeidemarieCardPool>();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        if (cardPlay.Target == null)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }
}

[RegisterCard(typeof(HeidemarieCardPool))]
public sealed class TestSkill()
    : ManosabaCardTemplate(1, CardType.Skill, CardRarity.Token, TargetType.Self, false)
{
    public override CardPoolModel Pool => ModelDb.CardPool<HeidemarieCardPool>();

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        ComponentContext componentContext)
    {
        return Task.CompletedTask;
    }
}
