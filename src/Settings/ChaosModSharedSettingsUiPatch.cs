using System;
using System.Collections.Generic;
using System.Reflection;
using ChaosHeidemarie.BattleReady;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace ChaosHeidemarie.Settings;

[HarmonyPatch(typeof(NSettingsScreen), nameof(NSettingsScreen._Ready))]
internal static class ChaosModSharedSettingsUiPatch
{
    private const string ClipperPath = "ScrollContainer/Mask/Clipper";
    private const string SoundSettingsVBoxPath = ClipperPath + "/SoundSettings/VBoxContainer";
    private const string SettingsTabManagerPath = "%SettingsTabManager";
    private const string ModTabScenePath = "res://scenes/screens/settings_tab.tscn";
    private const string SettingsSliderScenePath = "res://scenes/screens/settings_slider.tscn";
    private const string SettingsTickboxScenePath = "res://scenes/screens/settings_tickbox.tscn";
    private const string TemplateLabelPath = "SfxVolume/Label";
    private static readonly string TemplateLabelFullPath = SoundSettingsVBoxPath + "/" + TemplateLabelPath;
    private const string TickboxTickedPath = "TickboxVisuals/Ticked";
    private const string TickboxNotTickedPath = "TickboxVisuals/NotTicked";
    private const string TickboxReticlePath = "SelectionReticle";

    private const string ModTabName = "XCskin_ModSettingsTab";
    private const string ModPanelName = "XCskin_ModSettingsPanel";
    private const string VoiceSectionName = "ChaosModVoiceVolume";
    private const string VoiceSliderName = "ChaosModVoiceSlider";
    private const string VoiceLineName = "Line_ChaosModVoice";
    private const string PortraitsSectionName = "ChaosModPortraitsEnabled";
    private const string PortraitsTickboxName = "ChaosModPortraitsTickbox";
    private const string PortraitsLineName = "Line_ChaosModPortraitsEnabled";
    private const string ScaleSectionName = "ChaosModBattleReadyScale";
    private const string ScaleSliderName = "ChaosModBattleReadyScaleSlider";
    private const string ScaleLineName = "Line_ChaosModBattleReadyScale";
    private const string OffsetYSectionName = "ChaosModBattleReadyOffsetY";
    private const string OffsetYSliderName = "ChaosModBattleReadyOffsetYSlider";
    private const string OffsetYLineName = "Line_ChaosModBattleReadyOffsetY";
    private const string OffsetXSectionName = "ChaosModBattleReadyOffsetX";
    private const string OffsetXSliderName = "ChaosModBattleReadyOffsetXSlider";
    private const string OffsetXLineName = "Line_ChaosModBattleReadyOffsetX";
    private const string ResetSectionName = "ChaosModBattleReadyReset";
    private const string ResetButtonName = "ChaosModBattleReadyResetButton";
    private const string ResetLineName = "Line_ChaosModBattleReadyReset";
    private const string ControlWiredMeta = "ChaosHeidemarie_SettingsWired";

    [HarmonyPostfix]
    public static void Postfix(NSettingsScreen __instance)
    {
        TryInject(__instance, "_Ready");
    }

    public static void TryInject(NSettingsScreen screen, string source)
    {
        try
        {
            TryInjectInner(screen, source);
        }
        catch (Exception ex)
        {
            ModEntry.Logger.Warn($"[Settings] Inject failed ({source}): {ex}");
        }
    }

    private static void TryInjectInner(NSettingsScreen screen, string source)
    {
        if (!EnsureModSettingsTabAndPanel(screen, out NSettingsPanel? panel, source))
            return;

        VBoxContainer? vbox = panel!.GetNodeOrNull<VBoxContainer>("VBoxContainer");
        if (vbox == null)
        {
            ModEntry.Logger.Warn($"[Settings] Inject skipped ({source}): panel missing VBoxContainer.");
            return;
        }

        RichTextLabel? templateLabel = screen.GetNodeOrNull<RichTextLabel>(TemplateLabelFullPath);
        PackedScene? settingsSliderScene = ResourceLoader.Load<PackedScene>(SettingsSliderScenePath);
        PackedScene? settingsTickboxScene = ResourceLoader.Load<PackedScene>(SettingsTickboxScenePath);
        if (settingsSliderScene == null || settingsTickboxScene == null)
        {
            ModEntry.Logger.Warn($"[Settings] Inject skipped ({source}): missing slider or tickbox scene.");
            return;
        }

        EnsureSliderSection(vbox, templateLabel, settingsSliderScene, VoiceLineName, VoiceSectionName,
            VoiceSliderName, "角色语音音量");
        EnsureTickboxSection(vbox, templateLabel, settingsTickboxScene, PortraitsLineName, PortraitsSectionName,
            PortraitsTickboxName, "立绘开关");
        EnsureSliderSection(vbox, templateLabel, settingsSliderScene, ScaleLineName, ScaleSectionName,
            ScaleSliderName, "立绘缩放");
        EnsureSliderSection(vbox, templateLabel, settingsSliderScene, OffsetYLineName, OffsetYSectionName,
            OffsetYSliderName, "立绘位置Y");
        EnsureSliderSection(vbox, templateLabel, settingsSliderScene, OffsetXLineName, OffsetXSectionName,
            OffsetXSliderName, "立绘位置X");
        EnsureResetSection(vbox, templateLabel);

        Control? voiceSliderRoot = vbox.GetNodeOrNull<Control>(VoiceSectionName + "/" + VoiceSliderName);
        Control? portraitsTickboxRoot = vbox.GetNodeOrNull<Control>(PortraitsSectionName + "/" + PortraitsTickboxName);
        Control? scaleSliderRoot = vbox.GetNodeOrNull<Control>(ScaleSectionName + "/" + ScaleSliderName);
        Control? offsetYSliderRoot = vbox.GetNodeOrNull<Control>(OffsetYSectionName + "/" + OffsetYSliderName);
        Control? offsetXSliderRoot = vbox.GetNodeOrNull<Control>(OffsetXSectionName + "/" + OffsetXSliderName);
        Control? resetButtonRoot = vbox.GetNodeOrNull<Control>(ResetSectionName + "/" + ResetButtonName);

        if (voiceSliderRoot != null)
            Callable.From(() => WireVoiceSliderWhenReady(voiceSliderRoot, source, 0)).CallDeferred();
        if (portraitsTickboxRoot != null)
            Callable.From(() => WirePortraitsTickboxWhenReady(portraitsTickboxRoot, source, 0)).CallDeferred();
        if (scaleSliderRoot != null)
            Callable.From(() => WireScaleSliderWhenReady(scaleSliderRoot, source, 0)).CallDeferred();
        if (offsetYSliderRoot != null)
            Callable.From(() => WireOffsetYSliderWhenReady(offsetYSliderRoot, source, 0)).CallDeferred();
        if (offsetXSliderRoot != null)
            Callable.From(() => WireOffsetXSliderWhenReady(offsetXSliderRoot, source, 0)).CallDeferred();
        if (resetButtonRoot != null)
            Callable.From(() => WireResetButtonWhenReady(vbox, resetButtonRoot, source, 0)).CallDeferred();

        RefreshFocusNeighbors(voiceSliderRoot, portraitsTickboxRoot, scaleSliderRoot, offsetYSliderRoot, offsetXSliderRoot, resetButtonRoot);
    }

    private static void EnsureTickboxSection(
        VBoxContainer vbox,
        RichTextLabel? templateLabel,
        PackedScene tickboxScene,
        string lineName,
        string sectionName,
        string tickboxName,
        string labelText)
    {
        if (vbox.GetNodeOrNull(sectionName) != null)
            return;

        vbox.AddChild(CreateLine(lineName));
        VBoxContainer section = CreateSection(sectionName);
        section.AddChild(CreateLabel(templateLabel, labelText));
        Control tickbox = tickboxScene.Instantiate<Control>(PackedScene.GenEditState.Disabled);
        tickbox.Name = tickboxName;
        tickbox.Set("layout_mode", 2);
        tickbox.FocusMode = Control.FocusModeEnum.All;
        section.AddChild(tickbox);
        vbox.AddChild(section);
    }

    private static void EnsureSliderSection(
        VBoxContainer vbox,
        RichTextLabel? templateLabel,
        PackedScene sliderScene,
        string lineName,
        string sectionName,
        string sliderName,
        string labelText)
    {
        if (vbox.GetNodeOrNull(sectionName) != null)
            return;

        vbox.AddChild(CreateLine(lineName));
        VBoxContainer section = CreateSection(sectionName);
        section.AddChild(CreateLabel(templateLabel, labelText));
        Control slider = sliderScene.Instantiate<Control>(PackedScene.GenEditState.Disabled);
        slider.Name = sliderName;
        slider.Set("layout_mode", 2);
        slider.FocusMode = Control.FocusModeEnum.All;
        section.AddChild(slider);
        vbox.AddChild(section);
    }

    private static void EnsureResetSection(VBoxContainer vbox, RichTextLabel? templateLabel)
    {
        if (vbox.GetNodeOrNull(ResetSectionName) != null)
            return;

        vbox.AddChild(CreateLine(ResetLineName));
        VBoxContainer section = CreateSection(ResetSectionName);
        section.AddChild(CreateLabel(templateLabel, "重置立绘"));
        section.AddChild(CreateResetButton(templateLabel));
        vbox.AddChild(section);
    }

    private static ColorRect CreateLine(string name)
    {
        ColorRect line = new()
        {
            Name = name,
            CustomMinimumSize = new Vector2(0f, 2f),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Color = new Color(0.91f, 0.86f, 0.74f, 0.25f)
        };
        line.Set("layout_mode", 2);
        return line;
    }

    private static VBoxContainer CreateSection(string name)
    {
        VBoxContainer section = new()
        {
            Name = name,
            CustomMinimumSize = new Vector2(0f, 64f),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        section.Set("layout_mode", 2);
        section.AddThemeConstantOverride("separation", 6);
        return section;
    }

    private static RichTextLabel CreateLabel(RichTextLabel? templateLabel, string text)
    {
        RichTextLabel label = new()
        {
            Name = "Label",
            BbcodeEnabled = true,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Text = text,
            VerticalAlignment = VerticalAlignment.Center,
            CustomMinimumSize = new Vector2(0f, 28f)
        };
        label.Set("layout_mode", 2);
        if (templateLabel != null)
        {
            label.Theme = templateLabel.Theme;
            label.AddThemeFontOverride("normal_font", templateLabel.GetThemeFont("normal_font"));
            label.AddThemeFontOverride("bold_font", templateLabel.GetThemeFont("bold_font"));
            label.AddThemeFontSizeOverride("normal_font_size", templateLabel.GetThemeFontSize("normal_font_size"));
            label.AddThemeFontSizeOverride("bold_font_size", templateLabel.GetThemeFontSize("bold_font_size"));
        }
        return label;
    }

    private static Control CreateResetButton(RichTextLabel? templateLabel)
    {
        NButton button = new()
        {
            Name = ResetButtonName,
            CustomMinimumSize = new Vector2(320f, 48f),
            FocusMode = Control.FocusModeEnum.All
        };
        button.Set("layout_mode", 2);
        button.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        button.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;

        Texture2D? texture = ResourceLoader.Load<Texture2D>("res://images/ui/reward_screen/reward_skip_button.png");
        if (texture != null)
        {
            TextureRect bg = new()
            {
                Name = "Image",
                Texture = texture,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.Scale,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            bg.Set("layout_mode", 2);
            bg.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
            button.AddChild(bg);
        }

        MegaLabel label = new()
        {
            Name = "Label",
            Text = "重置立绘",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.Set("layout_mode", 2);
        label.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        label.MaxFontSize = 28;
        if (templateLabel != null)
        {
            label.Theme = templateLabel.Theme;
            label.AddThemeFontOverride("font", templateLabel.GetThemeFont("normal_font"));
            label.AddThemeFontSizeOverride("font_size", templateLabel.GetThemeFontSize("normal_font_size"));
        }
        button.AddChild(label);

        return button;
    }

    private static bool EnsureModSettingsTabAndPanel(NSettingsScreen screen, out NSettingsPanel? panel, string source)
    {
        panel = null;
        NSettingsTabManager? tabManager = screen.GetNodeOrNull<NSettingsTabManager>(SettingsTabManagerPath) ??
                                          screen.GetNodeOrNull<NSettingsTabManager>("SettingsTabManager");
        if (tabManager == null)
        {
            ModEntry.Logger.Warn($"[Settings] Inject skipped ({source}): missing SettingsTabManager.");
            return false;
        }

        Control? clipper = screen.GetNodeOrNull<Control>(ClipperPath);
        if (clipper == null)
        {
            ModEntry.Logger.Warn($"[Settings] Inject skipped ({source}): missing Clipper.");
            return false;
        }

        NSettingsTab? tab = tabManager.GetNodeOrNull<NSettingsTab>(ModTabName);
        NSettingsPanel? settingsPanel = clipper.GetNodeOrNull<NSettingsPanel>(ModPanelName);
        if (settingsPanel == null)
        {
            NSettingsPanel? templatePanel = screen.GetNodeOrNull<NSettingsPanel>("%SoundSettings") ??
                                            screen.GetNodeOrNull<NSettingsPanel>(ClipperPath + "/SoundSettings");
            if (templatePanel == null)
            {
                ModEntry.Logger.Warn($"[Settings] Inject skipped ({source}): missing SoundSettings template panel.");
                return false;
            }

            try
            {
                settingsPanel = (NSettingsPanel)templatePanel.Duplicate();
            }
            catch (Exception ex)
            {
                ModEntry.Logger.Warn($"[Settings] Duplicate settings panel failed ({source}): {ex.Message}");
                return false;
            }

            settingsPanel.Name = ModPanelName;
            settingsPanel.UniqueNameInOwner = true;
            settingsPanel.Visible = false;

            VBoxContainer? content = settingsPanel.GetNodeOrNull<VBoxContainer>("VBoxContainer");
            if (content == null)
            {
                ModEntry.Logger.Warn($"[Settings] Inject skipped ({source}): duplicated panel missing VBoxContainer.");
                return false;
            }

            Node? keep = null;
            foreach (Node child in content.GetChildren())
            {
                if (keep == null && child is Control)
                    keep = child;
            }

            foreach (Node child in content.GetChildren())
            {
                if (child == keep)
                    continue;
                content.RemoveChild(child);
                child.QueueFree();
            }

            clipper.AddChild(settingsPanel);
        }

        if (tab == null)
        {
            PackedScene? tabScene = ResourceLoader.Load<PackedScene>(ModTabScenePath);
            if (tabScene == null)
            {
                ModEntry.Logger.Warn($"[Settings] Inject skipped ({source}): missing tab scene.");
                return false;
            }

            try
            {
                tab = tabScene.Instantiate<NSettingsTab>(PackedScene.GenEditState.Disabled);
            }
            catch (Exception ex)
            {
                ModEntry.Logger.Warn($"[Settings] Instantiate tab failed ({source}): {ex.Message}");
                return false;
            }

            tab.Name = ModTabName;
            tab.UniqueNameInOwner = true;
            int rightIconIndex = -1;
            Node? rightIcon = tabManager.GetNodeOrNull("RightTriggerIcon");
            if (rightIcon != null)
                rightIconIndex = rightIcon.GetIndex();

            tabManager.AddChild(tab);
            if (rightIconIndex >= 0)
                tabManager.MoveChild(tab, rightIconIndex);

            tab.Set("layout_mode", 2);
            Callable.From(() =>
            {
                if (GodotObject.IsInstanceValid(tab) && tab.IsNodeReady())
                    tab.SetLabel("ChaosMod");
            }).CallDeferred();
        }
        else
        {
            tab.Set("layout_mode", 2);
        }

        panel = settingsPanel;
        EnsureTabBinding(tabManager, tab, settingsPanel);
        return true;
    }

    private static void EnsureTabBinding(NSettingsTabManager tabManager, NSettingsTab tab, NSettingsPanel panel)
    {
        try
        {
            FieldInfo? field = AccessTools.Field(typeof(NSettingsTabManager), "_tabs");
            if (field?.GetValue(tabManager) is not Dictionary<NSettingsTab, NSettingsPanel> dict)
                return;

            dict[tab] = panel;
            Callable callable = Callable.From<NButton>(_ => SwitchTabTo(tabManager, tab));
            if (!tab.IsConnected(NClickableControl.SignalName.Released, callable))
                tab.Connect(NClickableControl.SignalName.Released, callable);
        }
        catch
        {
        }
    }

    private static void SwitchTabTo(NSettingsTabManager tabManager, NSettingsTab tab)
    {
        try
        {
            MethodInfo? method = AccessTools.Method(typeof(NSettingsTabManager), "SwitchTabTo");
            method?.Invoke(tabManager, [tab]);
        }
        catch
        {
        }
    }

    private static void WirePortraitsTickboxWhenReady(Control tickboxRoot, string source, int attempt)
    {
        if (!GodotObject.IsInstanceValid(tickboxRoot))
            return;
        if (!tickboxRoot.IsNodeReady())
        {
            if (attempt < 8)
                Callable.From(() => WirePortraitsTickboxWhenReady(tickboxRoot, source, attempt + 1)).CallDeferred();
            return;
        }

        WirePortraitsTickbox(tickboxRoot);
    }

    private static void WirePortraitsTickbox(Control tickboxRoot)
    {
        ApplySettingsTickboxState(tickboxRoot, ChaosModSharedSettings.PortraitsEnabled);
        if (tickboxRoot.HasMeta(ControlWiredMeta))
            return;

        tickboxRoot.SetMeta(ControlWiredMeta, true);
        tickboxRoot.FocusMode = Control.FocusModeEnum.All;

        NSelectionReticle? reticle = tickboxRoot.GetNodeOrNull<NSelectionReticle>(TickboxReticlePath);
        WireFocusReticle(tickboxRoot, reticle);

        tickboxRoot.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(input =>
        {
            if (!ShouldToggleSettingsTickboxInput(input))
                return;

            bool enabled = !ChaosModSharedSettings.PortraitsEnabled;
            ChaosModSharedSettings.SetPortraitsEnabled(enabled, persist: true);
            ApplySettingsTickboxState(tickboxRoot, enabled);
            HeidemarieBattleReadyOverlay.RefreshPortraitState();
            tickboxRoot.AcceptEvent();
        }));
    }

    private static bool ShouldToggleSettingsTickboxInput(InputEvent input)
    {
        if (input is InputEventMouseButton mouseButton)
            return mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed;

        return input.IsActionReleased(MegaInput.select);
    }

    private static void ApplySettingsTickboxState(Control tickboxRoot, bool enabled)
    {
        Control? ticked = tickboxRoot.GetNodeOrNull<Control>(TickboxTickedPath);
        Control? notTicked = tickboxRoot.GetNodeOrNull<Control>(TickboxNotTickedPath);
        if (ticked == null || notTicked == null)
            return;

        ticked.Visible = enabled;
        notTicked.Visible = !enabled;
    }

    private static void WireVoiceSliderWhenReady(Control sliderRoot, string source, int attempt)
    {
        if (!GodotObject.IsInstanceValid(sliderRoot))
            return;
        if (!sliderRoot.IsNodeReady())
        {
            if (attempt < 8)
                Callable.From(() => WireVoiceSliderWhenReady(sliderRoot, source, attempt + 1)).CallDeferred();
            return;
        }

        WireVoiceSlider(sliderRoot);
    }

    private static void WireVoiceSlider(Control sliderRoot)
    {
        NSlider slider = sliderRoot.GetNode<NSlider>("Slider");
        Label? valueLabel = GetSliderValueLabel(sliderRoot);
        NSelectionReticle? reticle = sliderRoot.GetNodeOrNull<NSelectionReticle>("SelectionReticle");

        if (!sliderRoot.HasMeta(ControlWiredMeta))
        {
            sliderRoot.SetMeta(ControlWiredMeta, true);
            slider.MinValue = 0.0;
            slider.MaxValue = 100.0;
            slider.Step = 5.0;
            slider.Connect(Godot.Range.SignalName.ValueChanged, Callable.From<double>(value =>
            {
                SetSliderValueText(valueLabel, $"{value}%");
                ChaosModSharedSettings.SetVoiceVolume((float)value * 0.01f, persist: false);
            }));
            slider.Connect(NSlider.SignalName.MouseReleased, Callable.From<bool>(valueChanged =>
            {
                if (valueChanged)
                    ChaosModSharedSettings.SetVoiceVolume(ChaosModSharedSettings.VoiceVolume, persist: true);
            }));
            sliderRoot.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(input =>
            {
                if (input.IsActionPressed(MegaInput.left))
                    slider.Value -= 5.0;
                if (input.IsActionPressed(MegaInput.right))
                    slider.Value += 5.0;
            }));
            WireFocusReticle(sliderRoot, reticle);
        }

        SetVoiceSliderValue(sliderRoot, ChaosModSharedSettings.VoiceVolume);
    }

    private static void WireScaleSliderWhenReady(Control sliderRoot, string source, int attempt)
    {
        if (!GodotObject.IsInstanceValid(sliderRoot))
            return;
        if (!sliderRoot.IsNodeReady())
        {
            if (attempt < 8)
                Callable.From(() => WireScaleSliderWhenReady(sliderRoot, source, attempt + 1)).CallDeferred();
            return;
        }

        WireScaleSlider(sliderRoot);
    }

    private static void WireScaleSlider(Control sliderRoot)
    {
        NSlider slider = sliderRoot.GetNode<NSlider>("Slider");
        Label? valueLabel = GetSliderValueLabel(sliderRoot);
        NSelectionReticle? reticle = sliderRoot.GetNodeOrNull<NSelectionReticle>("SelectionReticle");

        if (!sliderRoot.HasMeta(ControlWiredMeta))
        {
            sliderRoot.SetMeta(ControlWiredMeta, true);
            slider.MinValue = 50.0;
            slider.MaxValue = 200.0;
            slider.Step = 5.0;
            slider.Connect(Godot.Range.SignalName.ValueChanged, Callable.From<double>(value =>
            {
                SetSliderValueText(valueLabel, $"{value}%");
                ChaosModSharedSettings.SetBattleReadyScale((float)value * 0.01f, persist: false);
                HeidemarieBattleReadyOverlay.ApplyTransformFromSettings();
            }));
            slider.Connect(NSlider.SignalName.MouseReleased, Callable.From<bool>(valueChanged =>
            {
                if (valueChanged)
                    ChaosModSharedSettings.SetBattleReadyScale(ChaosModSharedSettings.BattleReadyScale, persist: true);
            }));
            sliderRoot.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(input =>
            {
                if (input.IsActionPressed(MegaInput.left))
                    slider.Value -= 5.0;
                if (input.IsActionPressed(MegaInput.right))
                    slider.Value += 5.0;
            }));
            WireFocusReticle(sliderRoot, reticle);
        }

        SetScaleSliderValue(sliderRoot, ChaosModSharedSettings.BattleReadyScale);
    }

    private static void WireOffsetYSliderWhenReady(Control sliderRoot, string source, int attempt)
    {
        if (!GodotObject.IsInstanceValid(sliderRoot))
            return;
        if (!sliderRoot.IsNodeReady())
        {
            if (attempt < 8)
                Callable.From(() => WireOffsetYSliderWhenReady(sliderRoot, source, attempt + 1)).CallDeferred();
            return;
        }

        WireOffsetYSlider(sliderRoot);
    }

    private static void WireOffsetYSlider(Control sliderRoot)
    {
        NSlider slider = sliderRoot.GetNode<NSlider>("Slider");
        Label? valueLabel = GetSliderValueLabel(sliderRoot);
        NSelectionReticle? reticle = sliderRoot.GetNodeOrNull<NSelectionReticle>("SelectionReticle");

        if (!sliderRoot.HasMeta(ControlWiredMeta))
        {
            sliderRoot.SetMeta(ControlWiredMeta, true);
            slider.MinValue = 0.0;
            slider.MaxValue = 800.0;
            slider.Step = 10.0;
            slider.Connect(Godot.Range.SignalName.ValueChanged, Callable.From<double>(value =>
            {
                int display = (int)Math.Round(value - 400.0);
                SetSliderValueText(valueLabel, $"{display:+0;-0;0}px");
                ChaosModSharedSettings.SetBattleReadyOffsetY((float)value - 400f, persist: false);
                HeidemarieBattleReadyOverlay.ApplyTransformFromSettings();
            }));
            slider.Connect(NSlider.SignalName.MouseReleased, Callable.From<bool>(valueChanged =>
            {
                if (valueChanged)
                    ChaosModSharedSettings.SetBattleReadyOffsetY(ChaosModSharedSettings.BattleReadyOffsetY, persist: true);
            }));
            sliderRoot.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(input =>
            {
                if (input.IsActionPressed(MegaInput.left))
                    slider.Value -= 10.0;
                if (input.IsActionPressed(MegaInput.right))
                    slider.Value += 10.0;
            }));
            WireFocusReticle(sliderRoot, reticle);
        }

        SetOffsetSliderValue(sliderRoot, ChaosModSharedSettings.BattleReadyOffsetY);
    }

    private static void WireOffsetXSliderWhenReady(Control sliderRoot, string source, int attempt)
    {
        if (!GodotObject.IsInstanceValid(sliderRoot))
            return;
        if (!sliderRoot.IsNodeReady())
        {
            if (attempt < 8)
                Callable.From(() => WireOffsetXSliderWhenReady(sliderRoot, source, attempt + 1)).CallDeferred();
            return;
        }

        WireOffsetXSlider(sliderRoot);
    }

    private static void WireOffsetXSlider(Control sliderRoot)
    {
        NSlider slider = sliderRoot.GetNode<NSlider>("Slider");
        Label? valueLabel = GetSliderValueLabel(sliderRoot);
        NSelectionReticle? reticle = sliderRoot.GetNodeOrNull<NSelectionReticle>("SelectionReticle");

        if (!sliderRoot.HasMeta(ControlWiredMeta))
        {
            sliderRoot.SetMeta(ControlWiredMeta, true);
            slider.MinValue = 0.0;
            slider.MaxValue = 800.0;
            slider.Step = 10.0;
            slider.Connect(Godot.Range.SignalName.ValueChanged, Callable.From<double>(value =>
            {
                int display = (int)Math.Round(value - 400.0);
                SetSliderValueText(valueLabel, $"{display:+0;-0;0}px");
                ChaosModSharedSettings.SetBattleReadyOffsetX((float)value - 400f, persist: false);
                HeidemarieBattleReadyOverlay.ApplyTransformFromSettings();
            }));
            slider.Connect(NSlider.SignalName.MouseReleased, Callable.From<bool>(valueChanged =>
            {
                if (valueChanged)
                    ChaosModSharedSettings.SetBattleReadyOffsetX(ChaosModSharedSettings.BattleReadyOffsetX, persist: true);
            }));
            sliderRoot.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(input =>
            {
                if (input.IsActionPressed(MegaInput.left))
                    slider.Value -= 10.0;
                if (input.IsActionPressed(MegaInput.right))
                    slider.Value += 10.0;
            }));
            WireFocusReticle(sliderRoot, reticle);
        }

        SetOffsetSliderValue(sliderRoot, ChaosModSharedSettings.BattleReadyOffsetX);
    }

    private static void WireResetButtonWhenReady(VBoxContainer vbox, Control buttonRoot, string source, int attempt)
    {
        if (!GodotObject.IsInstanceValid(buttonRoot))
            return;
        if (!buttonRoot.IsNodeReady())
        {
            if (attempt < 8)
                Callable.From(() => WireResetButtonWhenReady(vbox, buttonRoot, source, attempt + 1)).CallDeferred();
            return;
        }

        WireResetButton(vbox, buttonRoot);
    }

    private static void WireResetButton(VBoxContainer vbox, Control buttonRoot)
    {
        if (buttonRoot.HasMeta(ControlWiredMeta))
            return;
        if (buttonRoot is not NClickableControl clickable)
            return;

        buttonRoot.SetMeta(ControlWiredMeta, true);
        clickable.Connect(NClickableControl.SignalName.Released,
            Callable.From<NClickableControl>(_ => ResetBattleReadyPosition(vbox)));
    }

    private static void ResetBattleReadyPosition(VBoxContainer vbox)
    {
        ChaosModSharedSettings.SetBattleReadyScale(1f, persist: false);
        ChaosModSharedSettings.SetBattleReadyOffsetX(0f, persist: false);
        ChaosModSharedSettings.SetBattleReadyOffsetY(0f, persist: true);
        HeidemarieBattleReadyOverlay.ApplyTransformFromSettings();

        Control? scaleRoot = vbox.GetNodeOrNull<Control>(ScaleSectionName + "/" + ScaleSliderName);
        Control? offsetXRoot = vbox.GetNodeOrNull<Control>(OffsetXSectionName + "/" + OffsetXSliderName);
        Control? offsetYRoot = vbox.GetNodeOrNull<Control>(OffsetYSectionName + "/" + OffsetYSliderName);
        if (scaleRoot != null)
            SetScaleSliderValue(scaleRoot, 1f);
        if (offsetXRoot != null)
            SetOffsetSliderValue(offsetXRoot, 0f);
        if (offsetYRoot != null)
            SetOffsetSliderValue(offsetYRoot, 0f);
    }

    private static void SetScaleSliderValue(Control sliderRoot, float scale)
    {
        if (!sliderRoot.IsNodeReady())
            return;

        NSlider? slider = sliderRoot.GetNodeOrNull<NSlider>("Slider");
        Label? valueLabel = GetSliderValueLabel(sliderRoot);
        if (slider == null || valueLabel == null)
            return;

        double value = Mathf.Clamp(scale * 100f, 50f, 200f);
        slider.SetValueWithoutAnimation(value);
        SetSliderValueText(valueLabel, $"{value}%");
    }

    private static void SetVoiceSliderValue(Control sliderRoot, float volume)
    {
        if (!sliderRoot.IsNodeReady())
            return;

        NSlider? slider = sliderRoot.GetNodeOrNull<NSlider>("Slider");
        Label? valueLabel = GetSliderValueLabel(sliderRoot);
        if (slider == null || valueLabel == null)
            return;

        double value = Mathf.Clamp(volume * 100f, 0f, 100f);
        slider.SetValueWithoutAnimation(value);
        SetSliderValueText(valueLabel, $"{value}%");
    }

    private static void SetOffsetSliderValue(Control sliderRoot, float offset)
    {
        if (!sliderRoot.IsNodeReady())
            return;

        NSlider? slider = sliderRoot.GetNodeOrNull<NSlider>("Slider");
        Label? valueLabel = GetSliderValueLabel(sliderRoot);
        if (slider == null || valueLabel == null)
            return;

        double value = Mathf.Clamp(offset + 400f, 0f, 800f);
        slider.SetValueWithoutAnimation(value);
        int display = (int)Math.Round(value - 400.0);
        SetSliderValueText(valueLabel, $"{display:+0;-0;0}px");
    }

    private static Label? GetSliderValueLabel(Control sliderRoot)
    {
        return sliderRoot.GetNodeOrNull<Label>("SliderValue");
    }

    private static void SetSliderValueText(Label? label, string text)
    {
        if (label == null)
            return;

        if (label is MegaLabel megaLabel)
        {
            megaLabel.SetTextAutoSize(text);
            return;
        }

        label.Text = text;
    }

    private static void WireFocusReticle(Control root, NSelectionReticle? reticle)
    {
        if (reticle == null)
            return;

        root.Connect(Control.SignalName.FocusEntered, Callable.From(() =>
        {
            if (NControllerManager.Instance?.IsUsingController == true)
                reticle.OnSelect();
        }));
        root.Connect(Control.SignalName.FocusExited, Callable.From(reticle.OnDeselect));
    }

    private static void RefreshFocusNeighbors(params Control?[] controls)
    {
        List<Control> focusables = [];
        foreach (Control? control in controls)
        {
            if (control != null)
                focusables.Add(control);
        }

        for (int i = 0; i < focusables.Count; i++)
        {
            Control current = focusables[i];
            current.FocusNeighborLeft = current.GetPath();
            current.FocusNeighborRight = current.GetPath();
            current.FocusNeighborTop = (i > 0 ? focusables[i - 1] : current).GetPath();
            current.FocusNeighborBottom = (i < focusables.Count - 1 ? focusables[i + 1] : current).GetPath();
        }
    }
}

[HarmonyPatch(typeof(NSettingsScreen), nameof(NSettingsScreen.OnSubmenuOpened))]
internal static class ChaosModSharedSettingsUiOpenPatch
{
    [HarmonyPostfix]
    public static void Postfix(NSettingsScreen __instance)
    {
        ChaosModSharedSettingsUiPatch.TryInject(__instance, "OnSubmenuOpened");
    }
}
