using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class BladesAfterglowPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/BladesAfterglowPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/BladesAfterglowPower_Big.png"
    );

    private static readonly LocString SelectPrompt =
        new("card_selection", "CHAOS_HEIDEMARIE_SELECT_CARD_TO_REPLAY");

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        var power = Owner.GetPower<InherentMemoryPower>();
        if (power == null) return;
        var cardToTransfigure = await CardSelectCmd.FromHand(new ThrowingPlayerChoiceContext(), Owner.Player,
            new CardSelectorPrefs(SelectPrompt, 1), null, this);
        foreach (var item in cardToTransfigure)
        {
            if (!item.EnergyCost.CostsX && item.EnergyCost.GetWithModifiers(CostModifiers.None) >= 0)
            {
                item.EnergyCost.AddThisCombat(1);
            }

            item.BaseReplayCount++;
            await power.ConsumeAsync(new ThrowingPlayerChoiceContext(), 1m, Owner, null);
        }
    }
}