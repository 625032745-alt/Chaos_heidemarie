using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class RadiantShieldCard : ModCardTemplate
{
    public override bool GainsBlock => true;
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("BlockAmount", 5m),
        ModCardVars.ComputedBlock("BlockValue", static ctx =>
        {
            var blockAmount = ctx.GetCardBaseValueOrDefault("BlockAmount", 5m);
            if (!ctx.HasSourceCreature) return blockAmount;
            var powerAmount = ctx.SourceCreature.GetPowerAmount<InherentMemoryPower>();
            return blockAmount + Math.Min(blockAmount, powerAmount * 2);
        })
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];

    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public RadiantShieldCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.EvaluateValueOrDefault("BlockValue"), ValueProp.Move,
            cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockAmount"].UpgradeValueBy(3);
    }
}