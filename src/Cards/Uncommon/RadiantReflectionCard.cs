using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using ChaosHeidemarie.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class RadiantReflectionCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("StrengthPowerStar", 1),
        ModCardVars.ComputedPowerAmountGiven<StrengthPower>(
            "StrengthPower",
            DynamicVars["StrengthPowerStar"].BaseValue,
            static (card, _) =>
            {
                var baseValue = card.DynamicVars["StrengthPowerStar"].BaseValue;
                var power = card.Owner?.Creature.GetPowerAmount<InherentMemoryPower>() ?? 0;
                int amount = power / 3;
                return baseValue + amount;
            }),
    ];

    public RadiantReflectionCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonUtils.AddOrModifyPower<StrengthPower>(choiceContext, Owner.Creature,
            DynamicVars.GetComputedValue("StrengthPower"), this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPowerStar"].UpgradeValueBy(1);
    }
}