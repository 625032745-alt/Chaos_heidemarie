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

namespace ChaosHeidemarie.Cards.Rare;

[RegisterCard(typeof(HeidemarieCardPool))]
public class ScarletResonanceCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new("ScarletResonance", 2)];

    public ScarletResonanceCard()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 先获得临时Power，让本张牌触发的赤光剑攻击也获得格挡
        var blockAmount = DynamicVars["ScarletResonance"].BaseValue;
        await PowerCmd.Apply<ScarletResonancePower>(choiceContext, Owner.Creature, blockAmount, Owner.Creature, this);
        // 选择0至3张手牌丢弃
        var selected = await CardSelectionHelper.SelectFromPile(choiceContext, Owner, PileType.Hand,
            CardSelectorPrefs.DiscardSelectionPrompt, 3, 1);
        foreach (var card in selected)
        {
            await CardCmd.Discard(choiceContext, card);
            if (card.Keywords.Contains(LinkKeywords.Link))
            {
                await PowerCmd.Apply<ScarletSwordPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ScarletResonance"].UpgradeValueBy(1);
    }
}