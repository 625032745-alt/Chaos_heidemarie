using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class FragmentedDawnPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/FragmentedDawnPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/FragmentedDawnPower_Big.png"
    );

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card.Keywords.Contains(LinkKeywords.Link))
        {
            await PlayerCmd.GainEnergy(1, Owner.Player);
            await PowerCmd.ModifyAmount(choiceContext, this, -1m, null, null);
        }
    }
}