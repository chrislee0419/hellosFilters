using System;
using UnityEngine;
using UnityEngine.UI;
using HMUI;
using VRUIControls;
using BS_Utils.Utilities;
using BeatSaberMarkupLanguage.Attributes;
using HUI.Attributes;
using HUI.UI.Components;
using HUI.UI.Screens;
using HUI.Utilities;
using Object = UnityEngine.Object;

namespace HUIFilters.UI.Screens
{
    [AutoInstall]
    public class FilterWidgetScreenManager : ModifiableScreenManagerBase
    {
        public event Action FilterButtonPressed;
        public event Action CancelFilterButtonPressed;

        public override string ScreenName => "Filter Widget";
        protected override string AssociatedBSMLResource => "HUIFilters.UI.Views.FilterWidgetScreenView.bsml";

        public bool FilterApplied
        {
            set
            {
                if (value)
                {
                    _filterIcon.sprite = AppliedFilterSprite;

                    _filterButtonAnimations.NormalIconColour = new Color(0.5f, 0.475f, 0.45f);
                    _filterButtonAnimations.HighlightedBGColour = FilterAppliedHighlightedBGColour;
                    _filterButtonAnimations.PressedBGColour = FilterAppliedHighlightedBGColour;

                    _cancelFilterButtonAnimations.NormalBGColour = new Color(1f, 0.375f, 0f, 0.5f);
                }
                else
                {
                    _filterIcon.sprite = NotAppliedFilterSprite;

                    _filterButtonAnimations.NormalIconColour = new Color(0.5f, 0.5f, 0.5f);
                    _filterButtonAnimations.HighlightedBGColour = FilterNotAppliedHighlightedBGColour;
                    _filterButtonAnimations.PressedBGColour = FilterNotAppliedHighlightedBGColour;

                    _cancelFilterButtonAnimations.NormalBGColour = new Color(0f, 0f, 0f, 0.5f);
                }
            }
        }

#pragma warning disable CS0649
        [UIObject("filter-button")]
        private GameObject _filterButton;
        [UIObject("cancel-filter-button")]
        private GameObject _cancelFilterButton;
#pragma warning restore CS0649

        private Image _filterIcon;

        private CustomIconButtonAnimations _filterButtonAnimations;
        private CustomIconButtonAnimations _cancelFilterButtonAnimations;

        private static Sprite NotAppliedFilterSprite;
        private static Sprite AppliedFilterSprite;

        private static readonly Color FilterNotAppliedHighlightedBGColour = new Color(0.145f, 0.443f, 1f);
        private static readonly Color FilterAppliedHighlightedBGColour = new Color(1f, 0.375f, 0f, 0.5f);
        private static readonly Color CancelFilterHighlightedBGColour = new Color(1f, 0f, 0f, 0.5f);

        public FilterWidgetScreenManager(
            MainMenuViewController mainMenuVC,
            SoloFreePlayFlowCoordinator soloFC,
            PartyFreePlayFlowCoordinator partyFC,
            LevelCollectionNavigationController levelCollectionNC,
            PhysicsRaycasterWithCache physicsRaycaster)
            : base(mainMenuVC, soloFC, partyFC, levelCollectionNC, physicsRaycaster, new Vector2(44f, 10f), new Vector3(1.385f, 0.15f, 2.885f), Quaternion.Euler(65f, 18f, 0f))
        {
            this._screen.name = "HUIFilterWidgetScreen";

            this._animationHandler.UsePointerAnimations = false;

            if (NotAppliedFilterSprite == null)
                NotAppliedFilterSprite = UIUtilities.LoadSpriteFromResources("HUIFilters.Assets.filter.png");
            if (AppliedFilterSprite == null)
                AppliedFilterSprite = UIUtilities.LoadSpriteFromResources("HUIFilters.Assets.appliedfilter.png");

            Object.Destroy(_filterButton.GetComponent<ContentSizeFitter>());
            Object.Destroy(_cancelFilterButton.GetComponent<ContentSizeFitter>());

            // remove skew
            _filterButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _filterButton.transform.Find("Underline").GetComponent<ImageView>().SetSkew(0f);

            _cancelFilterButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _cancelFilterButton.transform.Find("Underline").GetComponent<ImageView>().SetSkew(0f);

            var container = _filterButton.transform.Find("Content");
            Object.DestroyImmediate(container.GetComponent<StackLayoutGroup>());

            var hlg = container.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 2f;
            hlg.padding = new RectOffset(2, 1, 1, 1);

            _filterIcon = new GameObject("Icon").AddComponent<Image>();
            _filterIcon.sprite = NotAppliedFilterSprite;
            _filterIcon.preserveAspect = true;

            var layoutElement = _filterIcon.gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = 3f;
            layoutElement.preferredWidth = 4f;

            layoutElement = _filterButton.transform.Find("Content/Text").gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 30f;

            _filterIcon.transform.SetParent(container, false);
            _filterIcon.transform.SetAsFirstSibling();

            // custom animations
            Object.Destroy(_filterButton.GetComponent<ButtonStaticAnimations>());
            Object.Destroy(_cancelFilterButton.GetComponent<ButtonStaticAnimations>());

            _filterButtonAnimations = _filterButton.AddComponent<CustomIconButtonAnimations>();
            _filterButtonAnimations.HighlightedLocalScale = new Vector3(1.2f, 1.2f, 1.2f);

            _cancelFilterButtonAnimations = _cancelFilterButton.AddComponent<CustomIconButtonAnimations>();
            _cancelFilterButtonAnimations.HighlightedLocalScale = new Vector3(1.1f, 1.1f, 1.1f);
            _cancelFilterButtonAnimations.NormalIconColour = Color.white;
            _cancelFilterButtonAnimations.HighlightedBGColour = CancelFilterHighlightedBGColour;
            _cancelFilterButtonAnimations.PressedBGColour = CancelFilterHighlightedBGColour;

            FilterApplied = false;
        }

        [UIAction("filter-button-clicked")]
        private void OnFilterButtonClicked()
        {
            this.CallAndHandleAction(FilterButtonPressed, nameof(FilterButtonPressed));
        }

        [UIAction("cancel-filter-button-clicked")]
        private void OnCancelFilterButtonClicked()
        {
            this.CallAndHandleAction(CancelFilterButtonPressed, nameof(CancelFilterButtonPressed));
        }
    }
}
