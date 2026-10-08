using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ChainResonanceCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("ChainResonance", 7)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];
    public ChainResonanceCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var baseValue = DynamicVars["ChainResonance"].BaseValue;
        var powerAmount = Owner.Creature.GetPowerAmount<InherentMemoryPower>();
        if (powerAmount >= baseValue)
        {
            await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature,
                4m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ChainResonance"].UpgradeValueBy(-2);
    }
}