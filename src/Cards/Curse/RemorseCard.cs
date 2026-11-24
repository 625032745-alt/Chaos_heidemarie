using ChaosHeidemarie.Cards.Ancient;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Curse;

[RegisterCard(typeof(CurseCardPool))]
public class RemorseCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("Combats", 6m)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<BladeParadiseCard>()];
    public override int MaxUpgradeLevel => 0;
    private const int MaxCombats = 6;
    private int _combatsSeen;

    public RemorseCard() : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
    {
    }

    [SavedProperty]
    public int CombatsSeen
    {
        get { return _combatsSeen; }
        set
        {
            AssertMutable();
            _combatsSeen = value;
            DynamicVars["Combats"].BaseValue = MaxCombats - CombatsSeen;
        }
    }

    public override async Task AfterCombatEnd(CombatRoom _)
    {
        var pile = Pile;
        if (pile != null && pile.Type == PileType.Deck)
        {
            CombatsSeen++;
            if (CombatsSeen >= 5 && Pile.Type == PileType.Deck)
            {
                var bladeParadiseCard = CombatState.CreateCard<BladeParadiseCard>(Owner);
                await CardCmd.Transform(this, bladeParadiseCard);
            }
        }
    }
}