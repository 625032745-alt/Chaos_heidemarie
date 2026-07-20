using System.Collections.Generic;
using System.Threading.Tasks;
using ChaosHeidemarie.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards;

[RegisterCard(typeof(ColorlessCardPool))]
public class EffulgentBladeCard : ModCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9, ValueProp.Move)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [LinkKeywords.Link, CardKeyword.Exhaust];
    public override CardAssetProfile AssetProfile => new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public EffulgentBladeCard() : base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Keywords.Contains(RecycleKeywords.Recycle))
        {
            card.RemoveKeyword(RecycleKeywords.Recycle);
        }
        if (EffulgentExpansionCard.RemainingCharges)
        {
            var damage = DynamicVars.Damage.BaseValue;
            damage += 2;
            await DamageCmd.Attack(damage)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card != this)
            return;
        var player = card.Owner;
        var combatState = player.Creature.CombatState;
        if (null == combatState)
            return;
        var damage = DynamicVars.Damage.BaseValue;
        if (EffulgentExpansionCard.RemainingCharges)
        {
            damage += 2;
        }

        await DamageCmd.Attack(damage)
            .FromCard(card, null)
            .TargetingRandomOpponents(combatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }
    
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel clonedBy)
    {
        if (card != this)
            return;
        
        if (oldPileType != PileType.Discard)
            return;

        var currentPile = card.Pile?.Type;
        
        if (currentPile != PileType.Draw)
            return;
        await CardPileCmd.Add(card, PileType.Discard);
    }
}
