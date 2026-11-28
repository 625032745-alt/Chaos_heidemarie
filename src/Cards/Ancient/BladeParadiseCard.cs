using ChaosHeidemarie.Content;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ChaosHeidemarie.Cards.Ancient;

[RegisterCard(typeof(HeidemarieCardPool))]
public class BladeParadiseCard : ModCardTemplate
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public override CardAssetProfile AssetProfile =>
        new(PortraitPath: $"res://ArtWorks/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(15, ValueProp.Move)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    private int _discardedCount;


    public BladeParadiseCard() : base(3, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is { IsHittable: false }) return;
        await PlayVideoAsync(NCombatRoom.Instance, "res://ArtWorks/videos/blade_paradise.ogv", 6.0);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public override Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (_discardedCount < 20)
        {
            _discardedCount++;
        }
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner.Creature || !props.IsPoweredAttack()) return 0m;
        if (cardSource != this) return 0;
        return 5 * _discardedCount;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }

    private static async Task PlayVideoAsync(Node parent, string path, double timeoutSeconds)
    {
        var video = new VideoStreamPlayer
        {
            Stream = ResourceLoader.Load<VideoStream>(path),
            Expand = true,
            Autoplay = true,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        video.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        video.OffsetLeft = video.OffsetTop = video.OffsetRight = video.OffsetBottom = 0;
        parent.AddChildSafely(video);

        var done = new TaskCompletionSource();

        void End()
        {
            if (!done.TrySetResult()) return;
            if (GodotObject.IsInstanceValid(video)) video.QueueFreeSafely();
        }

        video.Finished += End;
        var tree = video.GetTree();
        if (tree == null) End();
        else tree.CreateTimer(timeoutSeconds).Timeout += End;

        await done.Task;
    }
}