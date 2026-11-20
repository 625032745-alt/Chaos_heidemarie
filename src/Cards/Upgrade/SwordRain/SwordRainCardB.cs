using ChaosHeidemarie.Cards.Token;
using ChaosHeidemarie.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Upgrade.SwordRain;

[RegisterCard(typeof(HeidemarieCardPool))]
public class SwordRainCardB : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/SwordRainCard.png");
    public override CardVisualStyle CustomVisualStyle => CardVisualStyle.Ancient;
    public SwordRainCardB() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cards = PileType.Exhaust.GetPile(Owner).Cards
            .OfType<EffulgentBladeCard>().Take(5).ToList();
        foreach (var card in cards)
        {
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }
}