using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2RitsuLib.Patching.Models;

namespace ChaosHeidemarie.Patches;

internal static class HeidemarieAnimationPatchHelper
{
    public static bool IsTarget(CharacterModel? character)
    {
        return character != null && character.Id == ModelDb.GetId<Characters.Heidemarie>();
    }

    public static bool IsTarget(Player? player)
    {
        return IsTarget(player?.Character);
    }
}

internal sealed class HeidemarieCharacterSelectAnimationLoopPatch : IPatchMethod
{
    private const string BgContainerNodeName = "AnimatedBg";
    private const string LoopAnimationName = "animation";

    public static string PatchId => "chaos_heidemarie_character_select_animation_loop";

    public static string Description =>
        "Start the Heidemarie character select Spine background loop after selection";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))];
    }

    public static void Postfix(
        NCharacterSelectScreen __instance,
        NCharacterSelectButton charSelectButton,
        CharacterModel characterModel)
    {
        if (charSelectButton.IsLocked || !HeidemarieAnimationPatchHelper.IsTarget(characterModel))
            return;

        var bgContainer = __instance.GetNodeOrNull<Control>(BgContainerNodeName);
        if (bgContainer == null)
        {
            ModEntry.Logger.Warn($"[CharacterSelect] Missing bg container '{BgContainerNodeName}'.");
            return;
        }

        foreach (Node child in bgContainer.GetChildren())
            ForceLoopAnimationOnAllSpineSprites(child);
    }

    private static void ForceLoopAnimationOnAllSpineSprites(Node root)
    {
        Stack<Node> stack = new();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            foreach (Node child in node.GetChildren())
                stack.Push(child);

            if (node is not CanvasItem)
                continue;

            try
            {
                MegaSprite sprite = new(node);
                if (sprite.HasAnimation(LoopAnimationName))
                    sprite.GetAnimationState().SetAnimation(LoopAnimationName, loop: true);
            }
            catch
            {
                if (string.Equals(node.GetType().Name, "SpineSprite", System.StringComparison.Ordinal))
                    ModEntry.Logger.Warn($"[CharacterSelect] Failed to start animation on '{node.Name}'.");
            }
        }
    }
}

internal sealed class HeidemarieCombatInitialIdleBootstrapPatch : IPatchMethod
{
    private const string PrimeLoopAnimationName = "idle_loop";
    private const string ReadyAnimationName = "b_idle";
    private const string IdleAnimationName = "idle";

    public static string PatchId => "chaos_heidemarie_combat_initial_idle_bootstrap";

    public static string Description =>
        "Bootstrap Heidemarie combat idle animation when the local player creature becomes ready";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(NCreature), nameof(NCreature._Ready))];
    }

    public static void Postfix(NCreature __instance)
    {
        if (!GodotObject.IsInstanceValid(__instance) ||
            !__instance.Entity.IsPlayer ||
            !LocalContext.IsMe(__instance.Entity) ||
            !HeidemarieAnimationPatchHelper.IsTarget(__instance.Entity.Player))
        {
            return;
        }

        Callable.From(() => TryBootstrapInitialIdle(__instance, retries: 4)).CallDeferred();
    }

    private static void TryBootstrapInitialIdle(NCreature creature, int retries)
    {
        if (!GodotObject.IsInstanceValid(creature) ||
            !GodotObject.IsInstanceValid(creature.Visuals) ||
            !creature.HasSpineAnimation ||
            creature.Visuals.SpineBody == null)
        {
            if (retries > 0)
                Callable.From(() => TryBootstrapInitialIdle(creature, retries - 1)).CallDeferred();
            return;
        }

        var spine = creature.Visuals.SpineBody;
        if (spine == null)
            return;

        if (spine.HasAnimation(PrimeLoopAnimationName))
            spine.GetAnimationState().SetAnimation(PrimeLoopAnimationName, loop: true);

        if (spine.HasAnimation(ReadyAnimationName))
        {
            spine.GetAnimationState().SetAnimation(ReadyAnimationName, loop: false);

            if (spine.HasAnimation(IdleAnimationName))
            {
                spine.GetAnimationState().AddAnimation(IdleAnimationName, 0f, loop: true);
                return;
            }

            if (spine.HasAnimation(PrimeLoopAnimationName))
                spine.GetAnimationState().AddAnimation(PrimeLoopAnimationName, 0f, loop: true);

            return;
        }

        if (spine.HasAnimation(IdleAnimationName))
        {
            spine.GetAnimationState().SetAnimation(IdleAnimationName, loop: true);
            return;
        }

        if (spine.HasAnimation(PrimeLoopAnimationName))
            spine.GetAnimationState().SetAnimation(PrimeLoopAnimationName, loop: true);
    }
}
