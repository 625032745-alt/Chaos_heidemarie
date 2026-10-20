using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Common;

[RegisterCard(typeof(HeidemarieCardPool))]
public class SlumberDrawCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("SlumberDraw", 2)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link];

    public SlumberDrawCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var baseValue = DynamicVars["SlumberDraw"].IntValue;
        await CardPileCmd.Draw(choiceContext, baseValue, Owner);
        var selected = await CardSelectionHelper.SelectFromPile(
            choiceContext, Owner, PileType.Hand, CardSelectorPrefs.DiscardSelectionPrompt, 1, 1);
        foreach (var cardModel in selected)
        {
            await CardCmd.Discard(choiceContext, cardModel);
            if (!cardModel.Keywords.Contains(LinkKeywords.Link)) continue;
            await PowerCmd.Apply<InherentMemoryPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SlumberDraw"].UpgradeValueBy(1);
    }
}