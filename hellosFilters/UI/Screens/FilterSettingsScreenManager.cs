using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;
using HMUI;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HUI.Attributes;
using HUI.UI.Components;
using HUI.UI.Screens;
using HUI.UI.Settings;
using HUI.Utilities;
using HUIFilters.Filters;

namespace HUIFilters.UI.Screens
{
    [AutoInstall]
    public class FilterSettingsScreenManager : ModifiableScreenManagerBase
    {
        public event Action FilterApplied;

        public override string ScreenName => "Filter Settings";
        protected override string AssociatedBSMLResource => "HUIFilters.UI.Views.FilterSettingsScreenView.bsml";
        protected override bool ShowScreenOnSinglePlayerLevelSelectionStarting => false;
        protected override ScreensSettingsTab.BackgroundOpacity DefaultBGOpacity => ScreensSettingsTab.BackgroundOpacity.Translucent;

        public bool IsVisible => this._screen.isActiveAndEnabled;

        [UIValue("main-page-active")]
        public bool MainPageActive { get; set; } = true;
        [UIValue("settings-page-active")]
        public bool SettingsPageActive { get; set; } = false;
        [UIValue("saved-filters-page-active")]
        public bool SavedFiltersPageActive { get; set; } = false;

        [UIValue("filter-status-text")]
        public string FilterStatusText { get; set; } = NotAppliedStatusText;
        [UIValue("save-settings-button-interactable")]
        public bool SaveSettingsButtonInteractable { get; set; } = false;
        [UIValue("edit-saved-settings-button-interactable")]
        public bool EditSavedSettingsButtonInteractable { get; set; } = false;

#pragma warning disable CS0649
        [UIObject("main-page-buttons-container")]
        private GameObject _mainPageLayoutContainer;
        [UIObject("modify-settings-button")]
        private GameObject _mainModifySettingsButton;
        [UIObject("save-settings-button")]
        private GameObject _mainSaveSettingsButton;
        [UIObject("edit-saved-settings-button")]
        private GameObject _mainEditSavedSettingsButton;
        [UIObject("close-button")]
        private GameObject _mainCloseButton;
        [UIObject("main-saved-filters-list-up-button")]
        private GameObject _mainSavedFiltersListUpButton;
        [UIObject("main-saved-filters-list-down-button")]
        private GameObject _mainSavedFiltersListDownButton;

        [UIComponent("main-saved-filters-list")]
        private CustomListTableData _mainSavedFiltersList;

        [UIObject("settings-slide-out-container")]
        private GameObject _settingsSlideOutContainer;

        [UIComponent("settings-filters-list")]
        private CustomListTableData _settingsFiltersList;

        [UIComponent("edit-saved-filters-list")]
        private CustomListTableData _editSavedFiltersList;
#pragma warning restore CS0649

        private List<IFilter> _filters;

        private const string NotAppliedStatusText = "<color=#FFDDDD>Not applied</color>";
        private const string NotAppliedWithChangesStatusText = "<color=#FFFFDD>* Not applied</color>";
        private const string AppliedStatusText = "<color=#DDFFDD>Applied</color>";
        private const string AppliedWithChangesStatusText = "<color=#DDDDFF>* Applied</color>";

        public FilterSettingsScreenManager(
            MainMenuViewController mainMenuVC,
            SoloFreePlayFlowCoordinator soloFC,
            PartyFreePlayFlowCoordinator partyFC,
            LevelCollectionNavigationController levelCollectionNC,
            PhysicsRaycasterWithCache physicsRaycaster,
            List<IFilter> filters)
            : base(mainMenuVC, soloFC, partyFC, levelCollectionNC, physicsRaycaster, new Vector2(100f, 60f), new Vector3(0f, 0.15f, 1.5f), Quaternion.Euler(80f, 0f, 0f))
        {
            this._screen.name = "HUIFilterSettingsScreen";

            this._animationHandler.UsePointerAnimations = false;
            this._animationHandler.LocalScale = 0.025f;

            _filters = filters;

            // fix raycaster for BSMLList
            _mainSavedFiltersList.gameObject.FixRaycaster(physicsRaycaster);

            // remove underlines
            GameObject.Destroy(_mainSaveSettingsButton.transform.Find("Underline").gameObject);
            GameObject.Destroy(_mainEditSavedSettingsButton.transform.Find("Underline").gameObject);
            GameObject.Destroy(_mainSavedFiltersListUpButton.transform.Find("Underline").gameObject);
            GameObject.Destroy(_mainSavedFiltersListDownButton.transform.Find("Underline").gameObject);

            var icon = _mainSavedFiltersListUpButton.transform.Find("Content/Icon").GetComponent<ImageView>();
            icon.rectTransform.Rotate(0f, 0f, 180f, Space.Self);

            GameObject.Destroy(_mainSavedFiltersListUpButton.GetComponent<ContentSizeFitter>());
            GameObject.Destroy(_mainSavedFiltersListDownButton.GetComponent<ContentSizeFitter>());

            // remove skew
            _mainModifySettingsButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _mainModifySettingsButton.transform.Find("Underline").GetComponent<ImageView>().SetSkew(0f);
            _mainSaveSettingsButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _mainEditSavedSettingsButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _mainCloseButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _mainCloseButton.transform.Find("Underline").GetComponent<ImageView>().SetSkew(0f);
            _mainSavedFiltersListUpButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _mainSavedFiltersListDownButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);

            // custom animations
            GameObject.Destroy(_mainSavedFiltersListUpButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_mainSavedFiltersListDownButton.GetComponent<ButtonStaticAnimations>());

            Color highlightedColour = new Color(1f, 0.375f, 0f);

            var iconBtnAnims = _mainSavedFiltersListUpButton.gameObject.AddComponent<CustomIconButtonAnimations>();
            iconBtnAnims.HighlightedBGColour = highlightedColour;
            iconBtnAnims.PressedBGColour = highlightedColour;
            iconBtnAnims.HighlightedLocalScale = new Vector3(1.2f, 1.2f, 1.2f);

            iconBtnAnims = _mainSavedFiltersListDownButton.gameObject.AddComponent<CustomIconButtonAnimations>();
            iconBtnAnims.HighlightedBGColour = highlightedColour;
            iconBtnAnims.PressedBGColour = highlightedColour;
            iconBtnAnims.HighlightedLocalScale = new Vector3(1.2f, 1.2f, 1.2f);

            // change icon size in buttons by changing the RectOffset
            var slg = _mainSavedFiltersListUpButton.transform.Find("Content").GetComponent<StackLayoutGroup>();
            slg.padding = new RectOffset(0, 0, 1, 1);

            slg = _mainSavedFiltersListDownButton.transform.Find("Content").GetComponent<StackLayoutGroup>();
            slg.padding = new RectOffset(0, 0, 1, 1);

            // test data
            _mainSavedFiltersList.data.Clear();
            _mainSavedFiltersList.data.Add(new CustomListTableData.CustomCellInfo("test 1"));
            _mainSavedFiltersList.data.Add(new CustomListTableData.CustomCellInfo("saved filter test 2"));
            _mainSavedFiltersList.data.Add(new CustomListTableData.CustomCellInfo("test 3"));
            _mainSavedFiltersList.data.Add(new CustomListTableData.CustomCellInfo("saved filter 4"));
            _mainSavedFiltersList.data.Add(new CustomListTableData.CustomCellInfo("test 5"));
            _mainSavedFiltersList.data.Add(new CustomListTableData.CustomCellInfo("saved filter test 6"));
            _mainSavedFiltersList.data.Add(new CustomListTableData.CustomCellInfo("test 7"));
            _mainSavedFiltersList.data.Add(new CustomListTableData.CustomCellInfo("saved filter 8"));
            _mainSavedFiltersList.data.Add(new CustomListTableData.CustomCellInfo("9"));
            _mainSavedFiltersList.data.Add(new CustomListTableData.CustomCellInfo("10th test"));
            _mainSavedFiltersList.tableView.ReloadData();
        }

        protected override void OnLevelCollectionNavigationControllerActivated(bool firstActivation, bool addToHierarchy, bool screenSystemEnabling)
        {
            // do not show screen during activation
        }

        public void ShowScreen()
        {
            MainPageActive = true;
            SettingsPageActive = false;
            SavedFiltersPageActive = false;

            this._animationHandler.PlayRevealAnimation();
        }

        public void HideScreen() => this._animationHandler.PlayConcealAnimation();

        private void RefreshSavedFiltersList()
        {
            _mainSavedFiltersList.data.Clear();
            _editSavedFiltersList.data.Clear();

            // TODO: load saved filters from config
            List<SavedFilter> savedFilters = new List<SavedFilter>();
            foreach (var savedFilter in savedFilters)
            {
                var cell = new CustomListTableData.CustomCellInfo(savedFilter.Name);

                _mainSavedFiltersList.data.Add(cell);
                _editSavedFiltersList.data.Add(cell);
            }

            _mainSavedFiltersList.tableView.ReloadData();
            _editSavedFiltersList.tableView.ReloadData();
        }

        [UIAction("modify-settings-button-clicked")]
        public void OnModifySettingsButtonClicked()
        {
            MainPageActive = false;
            SettingsPageActive = true;
            SavedFiltersPageActive = false;
        }

        [UIAction("save-settings-button-clicked")]
        public void OnSaveSettingsButtonClicked()
        {

        }

        [UIAction("edit-saved-settings-button-clicked")]
        public void OnEditSavedSettingsButtonClicked()
        {

        }

        [UIAction("close-button-clicked")]
        public void OnCloseButtonClicked() => HideScreen();

        [UIAction("main-saved-filters-cell-selected")]
        public void OnMainSavedFiltersListCellSelected(TableView tableView, int index)
        {

        }
    }
}
