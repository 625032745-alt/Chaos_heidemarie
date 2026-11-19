using ChaosHeidemarie.Cards.Rare;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class DawnlightJudgmentPower : ModTemporaryAppliedPowerTemplate<DawnlightJudgmentCard,StrengthPower>
{
    protected override bool IsPositive => false;
    
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/DawnlightJudgmentPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/DawnlightJudgmentPower_Big.png"
    );
}