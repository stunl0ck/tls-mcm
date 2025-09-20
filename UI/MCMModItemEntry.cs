using System;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TheLastStand.View.Generic;
using TheLastStand.View.Modding;
using Stunl0ck.TLS.Shared;
using Stunl0ck.ModConfigManager.DTO;

namespace Stunl0ck.ModConfigManager.UI
{
    internal static class MCMModItemEntry
    {
        private static readonly FieldInfo TextField = AccessTools.Field(typeof(ModItemView), "text");
        private static readonly FieldInfo CheckImageField = AccessTools.Field(typeof(ModItemView), "checkImage");
        private static readonly FieldInfo RawTooltipField = AccessTools.Field(typeof(ModItemView), "rawTextTooltipDisplayer");
        private static readonly FieldInfo UploadingLabelField = AccessTools.Field(typeof(ModItemView), "uploadingLabel");
        private static readonly FieldInfo GenericTooltipField = AccessTools.Field(typeof(ModItemView), "genericTooltipDisplayer");
        private static readonly FieldInfo UploadButtonField = AccessTools.Field(typeof(ModItemView), "uploadButton");
        private static readonly FieldInfo UploadButtonLabelField = AccessTools.Field(typeof(ModItemView), "uploadButtonLabel");
        private static readonly FieldInfo ActiveDataColorField = AccessTools.Field(typeof(ModItemView), "activeDataColor");
        private static readonly FieldInfo InactiveDataColorField = AccessTools.Field(typeof(ModItemView), "inactiveDataColor");

        public static void Spawn(ModsView modsView, Transform parent, ModItemView prefab, ModConfig mod)
        {
            var view = InstantiateView(parent, prefab, mod);

            ConfigureLabel(view, mod);
            ShowCheckIcon(view);
            ConfigureTooltip(view, modsView, mod);
            HideUploadOnlyElements(view);
            ReplaceUploadButton(view, modsView, mod);
        }

        private static ModItemView InstantiateView(Transform parent, ModItemView prefab, ModConfig mod)
        {
            var view = UnityEngine.Object.Instantiate(prefab, parent);
            view.gameObject.name = $"MCM Entry - {mod.modName}";
            return view;
        }

        private static void ConfigureLabel(ModItemView view, ModConfig mod)
        {
            var text = TextField.GetValue(view) as TextMeshProUGUI;
            text.text = $"<color=yellow>[MCM]</color> {mod.modName} ({Localization.LocalizeOrDefault("MCM_Creator", "Creator")}: {mod.author})";
            ApplyDataColor(view, text, active: true);
        }

        private static void ShowCheckIcon(ModItemView view)
        {
            var checkImage = CheckImageField.GetValue(view) as Image;
            checkImage.gameObject.SetActive(true);
        }

        private static void ConfigureTooltip(ModItemView view, ModsView modsView, ModConfig mod)
        {
            var displayer = RawTooltipField.GetValue(view) as RawTextTooltipDisplayer;
            displayer.Text = $"{Localization.LocalizeOrDefaultForModDescription(mod.modId, mod.description)}\r\n\r\n Version: {mod.version}";
            displayer.TargetTooltip = modsView.RawTextTooltip;
        }

        private static void HideUploadOnlyElements(ModItemView view)
        {
            var uploadingLabel = UploadingLabelField.GetValue(view) as TextMeshProUGUI;
            uploadingLabel.gameObject.SetActive(false);

            var genericTooltip = GenericTooltipField.GetValue(view) as GenericTooltipDisplayer;
            genericTooltip.gameObject.SetActive(false);
        }

        private static void ReplaceUploadButton(ModItemView view, ModsView modsView, ModConfig mod)
        {
            var uploadButton = UploadButtonField.GetValue(view);
            var uploadLabel = UploadButtonLabelField.GetValue(view) as TextMeshProUGUI;

            var buttonType = uploadButton.GetType();
            var gameObjectProp = buttonType.GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance);
            var uploadGO = gameObjectProp.GetValue(uploadButton) as GameObject;

            var interactableProp = buttonType.GetProperty("interactable", BindingFlags.Public | BindingFlags.Instance);
            interactableProp?.SetValue(uploadButton, false);

            uploadLabel?.gameObject.SetActive(false);
            foreach (var graphic in uploadGO.GetComponentsInChildren<Graphic>(true))
            {
                graphic.enabled = false;
            }

            var hasConfig = mod.config != null && mod.config.Count > 0;
            if (!hasConfig)
            {
                uploadGO.SetActive(false);
                return;
            }

            MCMButtonFactory.AddButtonSimple(
                uploadGO.transform,
                Localization.LocalizeOrDefault("MCM_Options", "Options"),
                () => MCMPopup.Show(modsView, mod),
                normalColor: new Color(0.75f, 0.75f, 0.75f),
                highlightedColor: Color.white
            );
        }

        private static void ApplyDataColor(ModItemView view, TextMeshProUGUI text, bool active)
        {
            var field = active ? ActiveDataColorField : InactiveDataColorField;
            var data = field?.GetValue(view);
            if (data == null) return;

            var type = data.GetType();
            var property = type.GetProperty("_Color", BindingFlags.Public | BindingFlags.Instance);

            text.color = (Color)property.GetValue(data);
        }
    }
}
