using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HMUI;
using VRUIControls;
using BeatSaberMarkupLanguage.Attributes;
using HUI.Attributes;
using HUI.UI.Components;
using HUI.UI.Screens;
using HUI.Utilities;
using Object = UnityEngine.Object;
using BSMLUtilities = BeatSaberMarkupLanguage.Utilities;

namespace HUIFilters.UI.Screens
{
    [AutoInstall]
    public class FilterWidgetScreenManager : ModifiableScreenManagerBase
    {
        public event Action FilterButtonPressed;
        public event Action CancelFilterButtonPressed;

        public override string ScreenName => "Filter Widget";
        protected override string AssociatedBSMLResource => "HUIFilters.UI.Views.Screens.FilterWidgetScreenView.bsml";

        public bool FilterApplied
        {
            set
            {
                if (value)
                {
                    _filterText.text = "<i>Applied</i>";
                    _filterIcon.sprite = AppliedFilterSprite;

                    _filterButtonAnimations.HighlightedBGColour = FilterAppliedHighlightedBGColour;
                    _filterButtonAnimations.PressedBGColour = FilterAppliedHighlightedBGColour;

                    _cancelFilterButtonAnimations.NormalBGColour = new Color(1f, 0.125f, 0f, 0.5f);
                }
                else
                {
                    _filterText.text = "Filter";
                    _filterIcon.sprite = NotAppliedFilterSprite;

                    _filterButtonAnimations.HighlightedBGColour = FilterNotAppliedHighlightedBGColour;
                    _filterButtonAnimations.PressedBGColour = FilterNotAppliedHighlightedBGColour;

                    _cancelFilterButtonAnimations.NormalBGColour = new Color(0f, 0f, 0f, 0.5f);
                }
            }
        }

#pragma warning disable CS0649
        [UIObject("filter-button")]
        private GameObject _filterButton;
        [UIObject("saved-filter-settings-list-button")]
        private GameObject _savedFilterSettingsListButton;
        [UIObject("cancel-filter-button")]
        private GameObject _cancelFilterButton;
#pragma warning restore CS0649

        private Image _filterIcon;
        private TextMeshProUGUI _filterText;

        private CustomIconButtonAnimations _filterButtonAnimations;
        private CustomIconButtonAnimations _cancelFilterButtonAnimations;

        private static Sprite NotAppliedFilterSprite;
        private static Sprite AppliedFilterSprite;

        private static readonly Color FilterNotAppliedHighlightedBGColour = new Color(0.145f, 0.443f, 1f);
        private static readonly Color FilterAppliedHighlightedBGColour = new Color(0f, 0.875f, 0f);
        private static readonly Color CancelFilterHighlightedBGColour = new Color(1f, 0f, 0f, 0.75f);
        private static readonly Color SavedFilterSettingsHighlightedBGColour = new Color(0.145f, 0.443f, 1f);

        public FilterWidgetScreenManager(
            MainMenuViewController mainMenuVC,
            SoloFreePlayFlowCoordinator soloFC,
            PartyFreePlayFlowCoordinator partyFC,
            LevelCollectionNavigationController levelCollectionNC,
            PhysicsRaycasterWithCache physicsRaycaster)
            : base(mainMenuVC, soloFC, partyFC, levelCollectionNC, physicsRaycaster, new Vector2(48f, 10f), new Vector3(1.44f, 0.15f, 2.875f), Quaternion.Euler(65f, 18f, 0f))
        {
            this._screen.name = "HUIFilterWidgetScreen";

            this._animationHandler.UsePointerAnimations = false;

            if (NotAppliedFilterSprite == null)
                NotAppliedFilterSprite = BSMLUtilities.FindSpriteInAssembly("hellosFilters:HUIFilters.Assets.filter.png");
            if (AppliedFilterSprite == null)
                AppliedFilterSprite = BSMLUtilities.FindSpriteInAssembly("hellosFilters:HUIFilters.Assets.appliedfilter.png");

            Object.Destroy(_filterButton.GetComponent<ContentSizeFitter>());
            Object.Destroy(_cancelFilterButton.GetComponent<ContentSizeFitter>());
            Object.Destroy(_savedFilterSettingsListButton.GetComponent<ContentSizeFitter>());

            // remove skew
            _filterButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _filterButton.transform.Find("Underline").GetComponent<ImageView>().SetSkew(0f);

            _cancelFilterButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _cancelFilterButton.transform.Find("Underline").GetComponent<ImageView>().SetSkew(0f);

            _savedFilterSettingsListButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _savedFilterSettingsListButton.transform.Find("Underline").GetComponent<ImageView>().SetSkew(0f);

            var container = _filterButton.transform.Find("Content");
            Object.DestroyImmediate(container.GetComponent<StackLayoutGroup>());

            _filterText = container.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _filterText.alignment = TextAlignmentOptions.Center;

            var hlg = container.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 1f;
            hlg.padding = new RectOffset(2, 1, 1, 1);

            _filterIcon = new GameObject("Icon").AddComponent<ImageView>();
            _filterIcon.sprite = NotAppliedFilterSprite;
            _filterIcon.preserveAspect = true;

            var layoutElement = _filterIcon.gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = 3f;
            layoutElement.preferredWidth = 4f;

            layoutElement = _filterText.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 20f;

            _filterIcon.transform.SetParent(container, false);
            _filterIcon.transform.SetAsFirstSibling();

            var slg = _savedFilterSettingsListButton.transform.Find("Content").GetComponent<StackLayoutGroup>();
            slg.padding = new RectOffset(2, 2, 0, 0);

            // custom animations
            Object.Destroy(_filterButton.GetComponent<ButtonStaticAnimations>());
            Object.Destroy(_cancelFilterButton.GetComponent<ButtonStaticAnimations>());
            Object.Destroy(_savedFilterSettingsListButton.GetComponent<ButtonStaticAnimations>());

            _filterButtonAnimations = _filterButton.AddComponent<CustomIconButtonAnimations>();
            _filterButtonAnimations.HighlightedLocalScale = new Vector3(1.2f, 1.2f, 1.2f);

            _cancelFilterButtonAnimations = _cancelFilterButton.AddComponent<CustomIconButtonAnimations>();
            _cancelFilterButtonAnimations.HighlightedLocalScale = new Vector3(1.1f, 1.1f, 1.1f);
            _cancelFilterButtonAnimations.NormalIconColour = Color.white;
            _cancelFilterButtonAnimations.HighlightedBGColour = CancelFilterHighlightedBGColour;
            _cancelFilterButtonAnimations.PressedBGColour = CancelFilterHighlightedBGColour;

            var btnAnims = _savedFilterSettingsListButton.AddComponent<CustomIconButtonAnimations>();
            btnAnims.HighlightedLocalScale = new Vector3(1.1f, 1.1f, 1.1f);
            btnAnims.NormalIconColour = Color.white;
            btnAnims.HighlightedBGColour = SavedFilterSettingsHighlightedBGColour;
            btnAnims.PressedBGColour = SavedFilterSettingsHighlightedBGColour;

            FilterApplied = false;
        }

        [UIAction("filter-button-clicked")]
        private void OnFilterButtonClicked()
        {
            Plugin.Log.DebugOnly("Filter button clicked");

            this.CallAndHandleAction(FilterButtonPressed, nameof(FilterButtonPressed));
        }

        [UIAction("saved-filter-settings-list-button-clicked")]
        private void OnSavedFilterSettingsListButtonClicked()
        {
            Plugin.Log.DebugOnly("Saved filter settings list button clicked");
        }

        [UIAction("cancel-filter-button-clicked")]
        private void OnCancelFilterButtonClicked()
        {
            Plugin.Log.DebugOnly("Cancel filter button clicked");

            this.CallAndHandleAction(CancelFilterButtonPressed, nameof(CancelFilterButtonPressed));
        }
    }
}
