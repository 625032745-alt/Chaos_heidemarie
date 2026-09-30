using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
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

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,RestKeyword.REST];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("RadiantReflection", 1)];

    public RadiantReflectionCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var baseValue = DynamicVars["RadiantReflection"].BaseValue;
        var power = Owner.Creature.GetPower<InherentMemoryPower>();
        if (power != null && power.Amount >= 3)
        {
            baseValue += power.Amount / 3;
        }
        await CommonUtils.AddOrModifyPower<StrengthPower>(choiceContext, Owner.Creature,
            baseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["RadiantReflection"].UpgradeValueBy(1);
    }
}