using ChaosHeidemarie.Cards.Token;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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
    private int _discardCount;

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card is EffulgentBladeCard)
        {
            _discardCount++;

            if (_discardCount >= 2)
            {
                _discardCount -= 2;
                await PlayerCmd.GainEnergy(Amount, Owner.Player);
            }
        }
    }
}