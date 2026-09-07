using ChaosHeidemarie.Cards.Rare;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace ChaosHeidemarie.Patches;

//显示先古卡框，但稀有度不变
[HarmonyPatch(typeof(NCard), "Reload")]
[HarmonyPriority(Priority.Low)]
public static class HeidemarieAncientFramePatch
{
    static void Postfix(NCard __instance)
    {
        var container = __instance.GetNode<Control>("CardContainer");
        if (container == null) return;

        bool isOurCard = __instance.Model is LiberationAuroraCard;

        if (!isOurCard)
        {
            if (container.HasNode("AncientBanner/Fire") && container.GetNode("AncientBanner/Fire") is CanvasItem restoredFire)
                restoredFire.Visible = true;
            return;
        }

        //可见性切换

        //隐藏普通框节点
        foreach (var name in new[] { "PortraitBorder", "TitleBanner", "StarIcon" })
        {
            if (container.HasNode(name) && container.GetNode(name) is CanvasItem item)
                item.Visible = false;
        }

        //显示先古框节点
        foreach (var name in new[] { "AncientTextBg", "AncientBanner" })
        {
            if (container.HasNode(name) && container.GetNode(name) is CanvasItem item)
                item.Visible = true;
        }

        //隐藏Fire子节点
        if (container.HasNode("AncientBanner/Fire") && container.GetNode("AncientBanner/Fire") is CanvasItem fire)
            fire.Visible = false;

        bool hasAncientBorder = container.HasNode("AncientBorder");
        if (hasAncientBorder)
        {
            //完整场景：显示 AncientBorder，隐藏普通Frame
            if (container.GetNode("AncientBorder") is TextureRect ancientBorderRect)
            {
                //显式加载Ancient边框纹理
                var ancientBorderPath = "res://images/atlases/compressed.sprites/card_template/ancient_card_border.tres";
                var ancientBorderTex = ResourceLoader.Load<Texture2D>(ancientBorderPath, null, ResourceLoader.CacheMode.Reuse);
                if (ancientBorderTex != null)
                    ancientBorderRect.Texture = ancientBorderTex;
                ancientBorderRect.Visible = true;
            }
            if (container.HasNode("Frame") && container.GetNode("Frame") is CanvasItem frame)
                frame.Visible = false;
        }
        else
        {
            //精简场景：复用 Frame 节点，替换为 Ancient 边框纹理
            if (container.HasNode("Frame") && container.GetNode("Frame") is TextureRect frame)
            {
                var ancientBorderPath = "res://images/atlases/compressed.sprites/card_template/ancient_card_border.tres";
                var ancientBorderTex = ResourceLoader.Load<Texture2D>(ancientBorderPath, null, ResourceLoader.CacheMode.Reuse);
                if (ancientBorderTex != null)
                    frame.Texture = ancientBorderTex;
                frame.Visible = true;
            }
        }

        //显示 AncientHighlight（原版 Ancient 卡的装饰性金色光晕）
        if (container.HasNode("AncientHighlight") && container.GetNode("AncientHighlight") is CanvasItem ancientHL)
            ancientHL.Visible = true;

        //不能隐藏 Highlight（NCardHighlight），它是选中/悬停高亮的唯一提供者
        // 原版 NCard.Reload() 在处理 Ancient 卡时也不碰 Highlight 的可见性

        //纹理设置
        // model.AncientTextBg 内部检查 Rarity==Ancient 才返回，非 Ancient 卡会抛异常
        // 需要手动拼路径加载 Power 类型的 AncientTextBg 纹理
        if (container.HasNode("AncientTextBg") && container.GetNode("AncientTextBg") is TextureRect ancientTextBg)
        {
            var path = "res://images/atlases/compressed.sprites/card_template/ancient_card_text_bg_power.tres";
            var tex = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
            if (tex != null)
                ancientTextBg.Texture = tex;
        }

        //遮罩裁剪
        // 原版 Ancient 卡通过 mask material 裁剪 AncientPortrait 溢出部分
        // 路径来自 NCard.Reload() 中的 _canvasGroupMaskMaterialPath
        var portraitCanvasGroup = container.GetNodeOrNull<CanvasGroup>("PortraitCanvasGroup");
        if (portraitCanvasGroup != null)
        {
            var maskMaterialPath = "res://scenes/cards/card_canvas_group_mask_material.tres";
            var maskMat = ResourceLoader.Load<Material>(maskMaterialPath, null, ResourceLoader.CacheMode.Reuse);
            if (maskMat != null)
                portraitCanvasGroup.Material = maskMat;
        }

        //Portrait处理

        // 隐藏普通Portrait
        if (portraitCanvasGroup.HasNode("Portrait") && portraitCanvasGroup.GetNode("Portrait") is CanvasItem normalPortrait)
            normalPortrait.Visible = false;

        // 显示AncientPortrait并设置纹理
        if (portraitCanvasGroup.HasNode("AncientPortrait") && portraitCanvasGroup.GetNode("AncientPortrait") is TextureRect ancientPortrait)
        {
            var model = __instance.Model;
            if (model?.Portrait != null)
            {
                ancientPortrait.Texture = model.Portrait;
                ancientPortrait.Modulate = Colors.White;
            }
            ancientPortrait.Visible = true;
        }

        //绘制顺序调整

        if (container.HasNode("OverlayContainer") && container.GetNode<Control>("OverlayContainer") is { } overlayContainer)
        {
            if (container.GetChildCount() > 7)
                container.MoveChild(overlayContainer, 7);
        }

        if (container.HasNode("AncientTextBg") && container.GetNode<Control>("AncientTextBg") is { } ancientTextBgCtrl)
        {
            if (container.GetChildCount() > 8)
                container.MoveChild(ancientTextBgCtrl, 8);
        }
    }
}
