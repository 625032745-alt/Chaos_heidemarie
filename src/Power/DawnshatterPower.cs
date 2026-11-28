using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Power;

[RegisterPower]
public class DawnshatterPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ArtWorks/images/power/DawnshatterPower_Small.png",
        BigIconPath: "res://ArtWorks/images/power/DawnshatterPower_Big.png"
    );

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        var pool = Owner.Player.Character.CardPool
            .GetUnlockedCards(Owner.Player.UnlockState, CombatState.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Attack && c.Rarity == CardRarity.Rare);
        var cards = CardFactory.GetForCombat(Owner.Player, pool, 3, CombatState.RunState.Rng.CombatCardGeneration)
            .ToList();
        foreach (var card in cards)
        {
            card.AddKeyword(CardKeyword.Exhaust);
            card.SetToFreeThisTurn();
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, Owner.Player);
    }
}