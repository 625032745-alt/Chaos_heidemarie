using ChaosHeidemarie.Content;
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
public class AfterglowTetherCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("AfterglowTether", 1)];

    public AfterglowTetherCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawn = (await CardPileCmd.Draw(choiceContext, 2m, Owner)).ToList();
        var card = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1), null, this)).FirstOrDefault();
        if (card == null) return;
        bool fromThisDraw = drawn.Any(c => ReferenceEquals(c, card));
        await CardCmd.Discard(choiceContext, card);

        if (fromThisDraw)
        {
            var baseValue = DynamicVars["AfterglowTether"].BaseValue;
            await PowerCmd.Apply<ScarletSwordPower>(choiceContext, Owner.Creature, baseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AfterglowTether"].UpgradeValueBy(1);
    }
}