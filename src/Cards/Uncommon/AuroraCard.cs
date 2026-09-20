using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using ChaosHeidemarie.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class AuroraCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("Aurora", 1)];

    public AuroraCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonUtils.AddOrModifyPower<AuroraPower>(choiceContext, Owner.Creature, DynamicVars["Aurora"].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Aurora"].UpgradeValueBy(1);
    }
}