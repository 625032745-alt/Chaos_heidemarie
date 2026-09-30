using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Rare;

[RegisterCard(typeof(HeidemarieCardPool))]
public class FragmentedDawnCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("FragmentedDawn", 3)];
    
    public FragmentedDawnCard() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FragmentedDawnPower>(choiceContext, Owner.Creature,
            DynamicVars["FragmentedDawn"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FragmentedDawn"].UpgradeValueBy(2m);
    }
}