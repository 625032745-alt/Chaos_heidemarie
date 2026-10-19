using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ForsakenDawnCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("ForsakenDawn", 1)];

    public ForsakenDawnCard() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var baseValue = DynamicVars["ForsakenDawn"].IntValue;
        var selected = await CardSelectionHelper.SelectFromPile(choiceContext, Owner, PileType.Hand,
            CardSelectorPrefs.DiscardSelectionPrompt, baseValue, 1);
        await CardCmd.Discard(choiceContext, selected);
        await CardPileCmd.Draw(choiceContext, baseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ForsakenDawn"].UpgradeValueBy(1);
    }
}