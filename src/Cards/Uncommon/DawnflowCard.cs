using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class DawnflowCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,RestKeyword.REST];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("Dawnflow", 2)];

    public DawnflowCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var baseValue = DynamicVars["Dawnflow"].BaseValue;
        await CardPileCmd.Draw(choiceContext, baseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Dawnflow"].UpgradeValueBy(1);
    }
}