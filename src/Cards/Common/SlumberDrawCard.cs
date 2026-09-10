using ChaosHeidemarie.Content;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class SlumberDrawCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("DrawDiscards", 1)];
    private static readonly LocString SelectFromHand = new("card_selection", "CHAOS_HEIDEMARIE_SELECT_FROM_HAND_TO_DISCARD");

    public SlumberDrawCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var baseValue = DynamicVars["DrawDiscards"].IntValue;
        await CardPileCmd.Draw(choiceContext, baseValue, Owner);
        var pile = PileType.Hand.GetPile(Owner);
        var selectFrom = (from c in pile.Cards orderby c.Rarity, c.Id select c).ToList();
        var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, selectFrom, Owner,
            new CardSelectorPrefs(SelectFromHand, baseValue));
        foreach (var cardModel in selected)
        {
            await CardCmd.Discard(choiceContext, cardModel);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DrawDiscards"].UpgradeValueBy(1);
    }
}