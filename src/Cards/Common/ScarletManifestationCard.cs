using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using ChaosHeidemarie.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ScarletManifestationCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("ScarletManifestation", 2)];

    public ScarletManifestationCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var baseValue = DynamicVars["ScarletManifestation"].BaseValue;
        await CommonUtils.AddOrModifyPower<ScarletSwordPower>(choiceContext, Owner.Creature, baseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ScarletManifestation"].UpgradeValueBy(1);
    }
}