using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using HMUI;
using VRUIControls;
using BeatSaberMarkupLanguage.Attributes;
using HUI.Attributes;
using HUI.UI.Components;
using HUI.UI.CustomBSML.Components;
using HUI.UI.Screens;
using HUI.UI.Settings;
using HUI.Utilities;
using HUIFilters.Filters;

namespace HUIFilters.UI.Screens
{
    [AutoInstall]
    public class SavedFilterSettingsListScreenManager : ModifiableScreenManagerBase
    {
        public event Action<SavedFilterSettings> SavedFilterSettingsApplied;

        public override string ScreenName => "Saved Filter Settings List";
        protected override string AssociatedBSMLResource => "HUIFilters.UI.Views.Screens.SavedFilterSettingsListScreenView.bsml";
        protected override bool ShowScreenOnSinglePlayerLevelSelectionStarting => false;
        protected override ScreensSettingsTab.BackgroundOpacity DefaultBGOpacity => ScreensSettingsTab.BackgroundOpacity.Translucent;

        public bool IsVisible => this._screen.isActiveAndEnabled;

        [UIValue("empty-list-text-active")]
        public bool EmptyListTextActive
        {
            get
            {
                if (_savedFilterSettingsList?.data == null)
                    return true;
                else
                    return _savedFilterSettingsList.data.Count == 0;
            }
        }

        [UIValue("apply-button-interactable")]
        public bool ApplyButtonInteractable
        {
            get
            {
                if (_savedFilterSettingsList?.tableView == null)
                    return false;
                else
                    return _savedFilterSettingsList.tableView.GetSelectedIndices().Count > 0;
            }
        }

#pragma warning disable CS0649
        [UIComponent("saved-filter-settings-list")]
        private HUICustomCellListTableData _savedFilterSettingsList;

        [UIObject("page-up-button")]
        private GameObject _pageUpButton;
        [UIObject("page-down-button")]
        private GameObject _pageDownButton;

        [UIObject("bottom-buttons-container")]
        private GameObject _bottomButtonsContainer;
        [UIObject("settings-button")]
        private GameObject _settingsButton;
        [UIObject("apply-button")]
        private GameObject _applyButton;
#pragma warning restore CS0649

        private List<SavedFilterSettingsListCell> _savedSettingsListCells = new List<SavedFilterSettingsListCell>();

        private SettingsModalDispatcher _settingsModalDispatcher;

        private Coroutine _concealDelayCoroutine;

        private static readonly WaitForSeconds ConcealDelaySeconds = new WaitForSeconds(1f);

        public SavedFilterSettingsListScreenManager(
            MainMenuViewController mainMenuVC,
            SoloFreePlayFlowCoordinator soloFC,
            PartyFreePlayFlowCoordinator partyFC,
            LevelCollectionNavigationController levelCollectionNC,
            PhysicsRaycasterWithCache physicsRaycaster,
            SettingsModalDispatcher settingsModalDispatcher)
            : base(mainMenuVC, soloFC, partyFC, levelCollectionNC, physicsRaycaster, new Vector2(52f, 60f), new Vector3(1.1f, 0.1f, 2.3f), Quaternion.Euler(85f, 18f, 0f))
        {
            this._screen.name = "HUISavedFilterSettingsListScreen";

            this._animationHandler.LocalScale = 0.025f;
            this._animationHandler.UsePointerAnimations = false;
            this._animationHandler.PointerEntered += OnPointerEntered;
            this._animationHandler.PointerExited += OnPointerExited;

            _settingsModalDispatcher = settingsModalDispatcher;

            // BSMLList needs a VRGraphicRaycaster, so we need to fix the PhysicsRaycaster,
            // just like what is done for FloatingScreen in ScreenManagerBase
            _savedFilterSettingsList.gameObject.FixRaycaster(physicsRaycaster);

            // can't set the size delta (or the preferred width/height) in the bsml file
            // since the visibleCells + listWidth properties will just override it
            // but at least i can do it here
            (_savedFilterSettingsList.transform as RectTransform).sizeDelta = Vector2.zero;

            var icon = _pageUpButton.transform.Find("Content/Icon").GetComponent<ImageView>();
            icon.rectTransform.Rotate(0f, 0f, 180f, Space.Self);

            GameObject.Destroy(_pageUpButton.GetComponent<ContentSizeFitter>());
            GameObject.Destroy(_pageDownButton.GetComponent<ContentSizeFitter>());
            GameObject.Destroy(_bottomButtonsContainer.GetComponent<ContentSizeFitter>());
            GameObject.Destroy(_settingsButton.GetComponent<ContentSizeFitter>());
            GameObject.Destroy(_applyButton.GetComponent<ContentSizeFitter>());

            GameObject.Destroy(_pageUpButton.transform.Find("Underline").gameObject);
            GameObject.Destroy(_pageDownButton.transform.Find("Underline").gameObject);
            GameObject.Destroy(_settingsButton.transform.Find("Underline").gameObject);
            GameObject.Destroy(_applyButton.transform.Find("Underline").gameObject);

            _pageUpButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _pageDownButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _settingsButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _applyButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);

            // custom animations
            GameObject.Destroy(_pageUpButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_pageDownButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_settingsButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_applyButton.GetComponent<ButtonStaticAnimations>());

            Color pageHighlightedColour = new Color(1f, 0.375f, 0f);
            Color settingsHighlightedColour = new Color(0.4f, 1f, 0.15f);
            Color applyHighlightedColour = new Color(0.145f, 0.443f, 1f);

            var iconBtnAnims = _pageUpButton.gameObject.AddComponent<CustomIconButtonAnimations>();
            iconBtnAnims.HighlightedBGColour = pageHighlightedColour;
            iconBtnAnims.PressedBGColour = pageHighlightedColour;
            iconBtnAnims.HighlightedLocalScale = new Vector3(1.2f, 1.2f, 1.2f);

            iconBtnAnims = _pageDownButton.gameObject.AddComponent<CustomIconButtonAnimations>();
            iconBtnAnims.HighlightedBGColour = pageHighlightedColour;
            iconBtnAnims.PressedBGColour = pageHighlightedColour;
            iconBtnAnims.HighlightedLocalScale = new Vector3(1.2f, 1.2f, 1.2f);

            iconBtnAnims = _settingsButton.gameObject.AddComponent<CustomIconButtonAnimations>();
            iconBtnAnims.HighlightedBGColour = settingsHighlightedColour;
            iconBtnAnims.PressedBGColour = settingsHighlightedColour;
            iconBtnAnims.HighlightedLocalScale = new Vector3(1.2f, 1.2f, 1.2f);

            var textBtnAnims = _applyButton.gameObject.AddComponent<CustomTextButtonAnimations>();
            textBtnAnims.HighlightedBGColour = applyHighlightedColour;
            textBtnAnims.PressedBGColour = applyHighlightedColour;

            // change icon size in buttons by changing the RectOffset
            var slg = _pageUpButton.transform.Find("Content").GetComponent<StackLayoutGroup>();
            slg.padding = new RectOffset(0, 0, 1, 1);

            slg = _pageDownButton.transform.Find("Content").GetComponent<StackLayoutGroup>();
            slg.padding = new RectOffset(0, 0, 1, 1);

            slg = _settingsButton.transform.Find("Content").GetComponent<StackLayoutGroup>();
            slg.padding = new RectOffset(0, 0, 1, 1);

            RefreshSavedFilterSettingsList();
        }

        protected override void OnLevelCollectionNavigationControllerActivated(bool firstActivation, bool addToHierarchy, bool screenSystemEnabling)
        {
            // do not show screen during activation
        }

        public void ShowScreen() => this._animationHandler.PlayRevealAnimation();

        public void HideScreen() => this._animationHandler.PlayConcealAnimation();

        internal void RefreshSavedFilterSettingsList()
        {
            _savedSettingsListCells.Clear();
            _savedSettingsListCells.AddRange(PluginConfig.Instance.SavedFilterSettings.Select(x => new SavedFilterSettingsListCell(x)));

            _savedFilterSettingsList.data = _savedSettingsListCells.Select(x => (object)x).ToList();
            _savedFilterSettingsList.tableView.ReloadData();

            NotifyPropertyChanged(nameof(EmptyListTextActive));
        }

        private void OnPointerEntered()
        {
            if (_concealDelayCoroutine != null)
            {
                this._animationHandler.StopCoroutine(_concealDelayCoroutine);
                _concealDelayCoroutine = null;
            }
        }

        private void OnPointerExited()
        {
            if (this._screen.ShowHandle)
                return;
            else if (_concealDelayCoroutine != null)
                this._animationHandler.StopCoroutine(_concealDelayCoroutine);

            _concealDelayCoroutine = this._animationHandler.StartCoroutine(ConcealDelayCoroutine());
        }

        private IEnumerator ConcealDelayCoroutine()
        {
            yield return ConcealDelaySeconds;
            this._animationHandler.PlayConcealAnimation();
        }

        [UIAction("saved-filter-settings-list-cell-selected")]
        private void OnSortModeListCellSelected(TableView tableView, object obj)
        {
            NotifyPropertyChanged(nameof(ApplyButtonInteractable));
        }

        [UIAction("settings-button-clicked")]
        private void OnSettingsButtonClicked() => _settingsModalDispatcher.ToggleModalVisibility();

        [UIAction("apply-button-clicked")]
        private void OnApplyButtonClicked()
        {
            var selectedCells = _savedFilterSettingsList.tableView.GetSelectedIndices();
            if (selectedCells.Count > 0)
            {
                int index = selectedCells.First();
                if (index < _savedSettingsListCells.Count)
                {
                    var savedSettings = _savedSettingsListCells[index].SavedSettings;
                    this.CallAndHandleAction(SavedFilterSettingsApplied, nameof(SavedFilterSettingsApplied), savedSettings);
                }

                _savedFilterSettingsList.tableView.ClearSelection();
            }

            NotifyPropertyChanged(nameof(ApplyButtonInteractable));
        }

        private class SavedFilterSettingsListCell
        {
            [UIValue("name-text")]
            public string NameText => SavedSettings.Name.EscapeTextMeshProTags();

            public SavedFilterSettings SavedSettings { get; private set; }

            public SavedFilterSettingsListCell(SavedFilterSettings savedSettings)
            {
                SavedSettings = savedSettings;
            }
        }
    }
}
