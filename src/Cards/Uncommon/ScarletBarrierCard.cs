using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ScarletBarrierCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedBlock("Block", 6m, static ctx =>
        {
            if (ctx == null)
                return 6m;
            var discardCount = CombatManager.Instance.History.Entries
                .OfType<CardDiscardedEntry>()
                .Count(e => e.HappenedThisTurn(ctx.CombatState) && e.Actor == ctx.Owner.Creature);

            return 6m + Math.Min(3m * discardCount, 9m);
        })
    ];

    public ScarletBarrierCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.ComputeDynamicValue("Block", 6m),
            ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(3);
    }
}