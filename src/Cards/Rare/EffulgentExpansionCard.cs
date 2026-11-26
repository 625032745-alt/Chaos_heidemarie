using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Cards.Upgrade.EffulgentExpansion;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Keywords;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Rare;

[RegisterCard(typeof(HeidemarieCardPool))]
public class EffulgentExpansionCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => [RestKeyword.REST];
    public override CardVisualStyle CustomVisualStyle => CardVisualStyle.Ancient;

    public EffulgentExpansionCard() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.GetPower<EffulgentExpansionPower>() != null)
            return;
        await PowerCmd.Apply<EffulgentExpansionPower>(choiceContext, Owner.Creature,
            1m, Owner.Creature, this);
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card != this)
            return;
        var combatState = card.CombatState;
        if (combatState == null)
            return;
        await CardGenerationHelper.GenerateEffulgentBlades(Owner, 2);
    }

    protected override void OnUpgrade()
    {
        CardCmd.ApplyKeyword(this, LinkKeywords.Link);
    }
}
