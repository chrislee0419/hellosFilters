using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using VRUIControls;
using HMUI;
using IPA.Utilities;
using BeatSaberMarkupLanguage.Attributes;
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
        public event Action FilterUnapplied;
        public event Action FilterReset;
        public event Action FilterCleared;

        public override string ScreenName => "Filter Settings";
        protected override string AssociatedBSMLResource => "HUIFilters.UI.Views.Screens.FilterSettingsScreenView.bsml";
        protected override bool ShowScreenOnSinglePlayerLevelSelectionStarting => false;
        protected override ScreensSettingsTab.BackgroundOpacity DefaultBGOpacity => ScreensSettingsTab.BackgroundOpacity.Translucent;

        public bool IsVisible => this._screen.isActiveAndEnabled;

        [UIValue("apply-button-active")]
        public bool ApplyButtonActive => _hasChanges || !_isApplied;
        [UIValue("unapply-button-active")]
        public bool UnapplyButtonActive => !ApplyButtonActive;

        [UIValue("apply-button-interactable")]
        public bool ApplyButtonInteractable => _hasChanges;
        [UIValue("save-settings-button-interactable")]
        public bool SaveSettingsButtonInteractable => _isApplied;
        [UIValue("reset-button-interactable")]
        public bool ResetButtonInteractable => _hasChanges;

        [UIValue("filter-status-text")]
        public string FilterStatusText
        {
            get
            {
                if (_isApplied)
                    return _hasChanges ? AppliedWithChangesStatusText : AppliedStatusText;
                else
                    return _hasChanges ? NotAppliedWithChangesStatusText : NotAppliedStatusText;
            }
        }

        private bool _isApplied = false;
        private bool IsApplied
        {
            get => _isApplied;
            set
            {
                if (_isApplied == value)
                    return;

                _isApplied = value;
                UpdateStatusImageColour();
                NotifyFilterStatusChanged();

            }
        }
        private bool _hasChanges = false;
        private bool HasChanges
        {
            get => _hasChanges;
            set
            {
                if (_hasChanges == value)
                    return;

                _hasChanges = value;
                UpdateStatusImageColour();
                NotifyFilterStatusChanged();
            }
        }

#pragma warning disable CS0649
        [UIObject("settings-top-bar")]
        private GameObject _settingsTopBar;
        [UIObject("close-button")]
        private GameObject _closeButton;

        [UIComponent("status-image")]
        private ImageView _statusImage;

        [UIObject("apply-button")]
        private GameObject _applyButton;
        [UIObject("unapply-button")]
        private GameObject _unapplyButton;
        [UIObject("save-settings-button")]
        private GameObject _saveSettingsButton;
        [UIObject("reset-button")]
        private GameObject _resetButton;
        [UIObject("clear-button")]
        private GameObject _clearButton;

        [UIObject("settings-container")]
        private GameObject _settingsContainer;
#pragma warning restore CS0649

        private List<IFilter> _filters;
        private IFilter _currentFilter;

        private SimpleTextDropdown _filtersDropdown;

        private const string NotAppliedStatusText = "NOT APPLIED";
        private const string NotAppliedWithChangesStatusText = "<i>NOT APPLIED (*)</i>";
        private const string AppliedStatusText = "<color=#DDFFDD>APPLIED</color>";
        private const string AppliedWithChangesStatusText = "<color=#DDDDFF><i>APPLIED (*)</i></color>";

        public FilterSettingsScreenManager(
            MainMenuViewController mainMenuVC,
            SoloFreePlayFlowCoordinator soloFC,
            PartyFreePlayFlowCoordinator partyFC,
            LevelCollectionNavigationController levelCollectionNC,
            PhysicsRaycasterWithCache physicsRaycaster,
            GameplaySetupViewController gameplaySetupViewController,
            DiContainer container,
            List<IFilter> filters)
            : base(mainMenuVC, soloFC, partyFC, levelCollectionNC, physicsRaycaster, new Vector2(100f, 60f), new Vector3(0f, 0.15f, 1.5f), Quaternion.Euler(80f, 0f, 0f))
        {
            this._screen.name = "HUIFilterSettingsScreen";

            this._animationHandler.UsePointerAnimations = false;
            this._animationHandler.LocalScale = 0.025f;

            _filters = filters;

            // dropdown button
            var playerSettingsPanelController = FieldAccessor<GameplaySetupViewController, PlayerSettingsPanelController>.Get(ref gameplaySetupViewController, "_playerSettingsPanelController");
            var noteJumpStartBeatOffsetDropdown = FieldAccessor<PlayerSettingsPanelController, NoteJumpStartBeatOffsetDropdown>.Get(ref playerSettingsPanelController, "_noteJumpStartBeatOffsetDropdown");
            var dropdownPrefab = FieldAccessor<NoteJumpStartBeatOffsetDropdown, SimpleTextDropdown>.Get(ref noteJumpStartBeatOffsetDropdown, "_simpleTextDropdown");

            _filtersDropdown = GameObject.Instantiate(dropdownPrefab, _settingsTopBar.transform, false);
            _filtersDropdown.name = "FiltersDropdown";

            GameObject.Destroy(_filtersDropdown.GetComponent<NoteJumpStartBeatOffsetDropdown>());

            // fix stuff
            DropdownWithTableView dropdown = _filtersDropdown as DropdownWithTableView;
            GameObjectUtilities.FixRaycaster(FieldAccessor<DropdownWithTableView, TableView>.Get(ref dropdown, "_tableView").gameObject, physicsRaycaster);

            ModalView dropdownModalView = dropdown.transform.Find("DropdownTableView").GetComponent<ModalView>();
            FieldAccessor<ModalView, DiContainer>.Set(ref dropdownModalView, "_container", container);

            var rt = dropdownModalView.transform as RectTransform;
            rt.anchorMin = new Vector2(rt.anchorMin.x, 1f);
            rt.anchorMax = new Vector2(rt.anchorMax.x, 1f);
            rt.pivot = new Vector2(rt.pivot.x, 1f);

            rt = _filtersDropdown.transform as RectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(40f, 0f);

            _filtersDropdown.SetTexts(_filters.Select(x => x.Name).ToList());

            _filtersDropdown.didSelectCellWithIdxEvent += OnFilterDropdownListCellSelected;

            // remove unused stuff from buttons
            GameObject.Destroy(_closeButton.transform.Find("Underline").gameObject);
            GameObject.Destroy(_closeButton.GetComponent<ContentSizeFitter>());
            GameObject.Destroy(_applyButton.GetComponent<ContentSizeFitter>());
            GameObject.Destroy(_unapplyButton.GetComponent<ContentSizeFitter>());
            GameObject.Destroy(_saveSettingsButton.GetComponent<ContentSizeFitter>());
            GameObject.Destroy(_resetButton.GetComponent<ContentSizeFitter>());
            GameObject.Destroy(_clearButton.GetComponent<ContentSizeFitter>());

            // close button
            _closeButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _closeButton.GetComponent<StackLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);
            _closeButton.transform.Find("Content").GetComponent<StackLayoutGroup>().padding = new RectOffset(2, 2, 0, 0);

            // custom animations
            GameObject.Destroy(_closeButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_applyButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_unapplyButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_resetButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_clearButton.GetComponent<ButtonStaticAnimations>());

            var iconBtnAnims = _closeButton.AddComponent<CustomIconButtonAnimations>();
            iconBtnAnims.HighlightedLocalScale = new Vector3(1.1f, 1.1f, 1.1f);
            iconBtnAnims.HighlightedBGColour = Color.red;
            iconBtnAnims.PressedBGColour = Color.red;

            Color applyButtonColour = new Color(0.4f, 1f, 0.133f, 0.75f);
            var textBtnAnims = _applyButton.AddComponent<CustomTextButtonAnimations>();
            textBtnAnims.NormalBGColour = applyButtonColour;
            textBtnAnims.HighlightedBGColour = applyButtonColour;
            textBtnAnims.PressedBGColour = applyButtonColour;

            Color unapplyButtonColour = new Color(0.116f, 0.354f, 0.8f);
            textBtnAnims = _unapplyButton.AddComponent<CustomTextButtonAnimations>();
            textBtnAnims.NormalBGColour = unapplyButtonColour;
            textBtnAnims.HighlightedBGColour = unapplyButtonColour;
            textBtnAnims.PressedBGColour = unapplyButtonColour;

            Color resetClearButtonColour = new Color(1f, 0f, 0f, 0.75f);
            textBtnAnims = _resetButton.AddComponent<CustomTextButtonAnimations>();
            textBtnAnims.NormalBGColour = resetClearButtonColour;
            textBtnAnims.HighlightedBGColour = resetClearButtonColour;
            textBtnAnims.PressedBGColour = resetClearButtonColour;

            textBtnAnims = _clearButton.AddComponent<CustomTextButtonAnimations>();
            textBtnAnims.NormalBGColour = resetClearButtonColour;
            textBtnAnims.HighlightedBGColour = resetClearButtonColour;
            textBtnAnims.PressedBGColour = resetClearButtonColour;

            UpdateStatusImageColour();
        }

        public override void Initialize()
        {
            base.Initialize();

            foreach (var filter in _filters)
                filter.SettingChanged += UpdateFilterStatus;

            _filtersDropdown.SelectCellWithIdx(0);
            _currentFilter = _filters[0];
            _currentFilter.ShowView(_settingsContainer);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_filters != null)
            {
                foreach (var filter in _filters)
                {
                    if (filter != null)
                        filter.SettingChanged -= UpdateFilterStatus;
                }
            }
        }

        protected override void OnLevelCollectionNavigationControllerActivated(bool firstActivation, bool addToHierarchy, bool screenSystemEnabling)
        {
            // do not show screen during activation
        }

        public void ShowScreen() => this._animationHandler.PlayRevealAnimation();

        public void HideScreen() => this._animationHandler.PlayConcealAnimation();

        private void UpdateStatusImageColour()
        {
            if (_isApplied)
                _statusImage.color = _hasChanges ? new Color(0.6f, 0.6f, 1f) : new Color(0.6f, 1f, 0.6f);
            else
                _statusImage.color = _hasChanges ? new Color(1f, 1f, 0.6f) : new Color(0.5f, 0.5f, 0.5f);
        }

        private void NotifyFilterStatusChanged()
        {
            this.NotifyPropertyChanged(nameof(ApplyButtonActive));
            this.NotifyPropertyChanged(nameof(UnapplyButtonActive));
            this.NotifyPropertyChanged(nameof(SaveSettingsButtonInteractable));
            this.NotifyPropertyChanged(nameof(ResetButtonInteractable));

            this.NotifyPropertyChanged(nameof(ApplyButtonInteractable));

            this.NotifyPropertyChanged(nameof(FilterStatusText));
        }

        private void UpdateFilterStatus()
        {
            IsApplied = _filters.Any(x => x.IsApplied);
            HasChanges = _filters.Any(x => x.HasChanges);
        }

        private void OnFilterDropdownListCellSelected(DropdownWithTableView dropdown, int index)
        {
            Plugin.Log.DebugOnly($"Filter at index {index} selected ({_filters[index].Name})");

            if (_currentFilter == _filters[index])
                return;

            _currentFilter.HideView();
            _currentFilter = _filters[index];
            _currentFilter.ShowView(_settingsContainer);
        }

        [UIAction("close-button-clicked")]
        private void OnCloseButtonClicked() => HideScreen();

        [UIAction("apply-button-clicked")]
        private void OnApplyButtonClicked()
        {
            Plugin.Log.DebugOnly("Apply filters button clicked");

            this.CallAndHandleAction(FilterApplied, nameof(FilterApplied));

            UpdateFilterStatus();
        }

        [UIAction("unapply-button-clicked")]
        private void OnUnapplyButtonClicked()
        {
            Plugin.Log.DebugOnly("Unapply filters button clicked");

            this.CallAndHandleAction(FilterUnapplied, nameof(FilterUnapplied));

            UpdateFilterStatus();
        }

        [UIAction("save-settings-button-clicked")]
        private void OnSaveSettingsButtonClicked()
        {
            Plugin.Log.Notice("save settings button clicked");
        }

        [UIAction("reset-button-clicked")]
        private void OnResetButtonClicked()
        {
            Plugin.Log.DebugOnly("Reset filters button clicked");

            this.CallAndHandleAction(FilterReset, nameof(FilterReset));

            UpdateFilterStatus();
        }

        [UIAction("clear-button-clicked")]
        private void OnClearButtonClicked()
        {
            Plugin.Log.DebugOnly("Clear filters button clicked");

            this.CallAndHandleAction(FilterCleared, nameof(FilterCleared));

            UpdateFilterStatus();
        }
    }
}
