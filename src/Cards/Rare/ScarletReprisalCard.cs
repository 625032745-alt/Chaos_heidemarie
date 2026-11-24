using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Rare;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ScarletReprisalCard : ModCardTemplate
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    private bool LostPowerThisTurn => CombatManager.Instance.History.Entries
        .OfType<PowerReceivedEntry>()
        .Any(e => e.HappenedThisTurn(CombatState) && e.Actor == Owner.Creature && e.Amount < 0m &&
                  e.Power is ScarletSwordPower);

    public ScarletReprisalCard() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        if (!LostPowerThisTurn)
            return;
        
        var actualDamage = attack.Results.SelectMany(r => r).Sum(r => r.UnblockedDamage);
        if (actualDamage <= 0m)
            return;
        
        var block = Math.Floor(actualDamage / 2m);
        if (block <= 0m)
            return;

        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Unpowered, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}