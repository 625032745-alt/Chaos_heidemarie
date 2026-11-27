using ChaosHeidemarie.Cards.Base;
using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Uncommon;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ScarletPreparationCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("ScarletPreparation", 2)];

    private static readonly LocString SelectPrompt =
        new("card_selection", "CHAOS_HEIDEMARIE_SELECT_CARD_TO_FREE");

    public ScarletPreparationCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = Owner.Creature.GetPower<ScarletSwordPower>();
        var consumeAmount = DynamicVars["ScarletPreparation"].BaseValue;
        // 赤光剑不足时，不触发效果
        if (power == null || power.Amount < consumeAmount)
            return;
        // 选择1张手牌
        var selected =
            await CardSelectionHelper.SelectFromPile(choiceContext, Owner, PileType.Hand, SelectPrompt, 1);

        var card = selected.FirstOrDefault();
        if (card == null)
            return;
        await power.ConsumeAsync(choiceContext, consumeAmount, Owner.Creature, this);

        // 选中的牌本回合费用变为0
        card.SetToFreeThisTurn();
        await CardPileCmd.Draw(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars["ScarletPreparation"].UpgradeValueBy(-1);
    }
}