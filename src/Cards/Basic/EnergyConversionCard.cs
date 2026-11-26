using ChaosHeidemarie.Content;
using ChaosHeidemarie.Power;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Basic;

[RegisterCard(typeof(HeidemarieCardPool))]
[RegisterCharacterStarterCard(typeof(Characters.Heidemarie))]
public class EnergyConversionCard : ModCardTemplate
{
    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    public EnergyConversionCard() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        var auroraPower = creature.GetPower<InherentMemoryPower>();
        var scarletPower = creature.GetPower<ScarletSwordPower>();
        var auroraAmount = auroraPower?.Amount ?? 0m;
        var scarletAmount = scarletPower?.Amount ?? 0m;

        // 层数相等时不产生效果
        if (auroraAmount == scarletAmount)
            return;
        if (auroraPower is { Amount: > 0 })
        {
            await auroraPower.ConsumeAsync(choiceContext, auroraAmount, creature, this);
        }

        if (scarletPower is { Amount: > 0 })
        {
            await scarletPower.ConsumeAsync(choiceContext, scarletAmount, creature, this);
        }

        if (scarletAmount > 0m)
        {
            await PowerCmd.Apply<InherentMemoryPower>(choiceContext, creature, scarletAmount, creature, this);
        }

        if (auroraAmount > 0m)
        {
            await PowerCmd.Apply<ScarletSwordPower>(choiceContext, creature, Math.Min(auroraAmount, 8m), creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}