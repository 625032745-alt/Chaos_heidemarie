using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ChainSlashCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5, ValueProp.Move), new("ChainSlash", 5)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    private bool WasDiscardAppliedThisTurn => CombatManager.Instance.History.Entries.OfType<CardDiscardedEntry>()
        .Any(e => e.HappenedThisTurn(CombatState) && e.Actor == Owner.Creature);

    public ChainSlashCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars["ChainSlash"].BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["ChainSlash"].BaseValue, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ChainSlash"].UpgradeValueBy(7);
    }

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner.Creature || !props.IsPoweredAttack()) return 0m;
        if (cardSource != this) return 0;
        return WasDiscardAppliedThisTurn ? DynamicVars["ChainSlash"].BaseValue : 0;
    }
}