using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HMUI;
using VRUIControls;
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
    public class SavedFilterSettingsListScreenManager : ModifiableScreenManagerBase
    {
        public event Action<int> SavedFilterSettingsApplied;

        public override string ScreenName => "Saved Filter Settings List";
        protected override string AssociatedBSMLResource => "HUIFilters.UI.Views.Screens.SavedFilterSettingsListScreenView.bsml";
        protected override bool ShowScreenOnSinglePlayerLevelSelectionStarting => false;
        protected override ScreensSettingsTab.BackgroundOpacity DefaultBGOpacity => ScreensSettingsTab.BackgroundOpacity.Translucent;

        public bool IsVisible => this._screen.isActiveAndEnabled;

#pragma warning disable CS0649
        [UIComponent("saved-filter-settings-list")]
        private CustomListTableData _savedFilterSettingsList;

        [UIObject("page-up-button")]
        private GameObject _pageUpButton;
        [UIObject("page-down-button")]
        private GameObject _pageDownButton;
#pragma warning restore CS0649

        private Coroutine _concealDelayCoroutine;

        private static readonly WaitForSeconds ConcealDelaySeconds = new WaitForSeconds(1f);

        public SavedFilterSettingsListScreenManager(
            MainMenuViewController mainMenuVC,
            SoloFreePlayFlowCoordinator soloFC,
            PartyFreePlayFlowCoordinator partyFC,
            LevelCollectionNavigationController levelCollectionNC,
            PhysicsRaycasterWithCache physicsRaycaster)
            : base(mainMenuVC, soloFC, partyFC, levelCollectionNC, physicsRaycaster, new Vector2(50f, 56f), new Vector3(1.2f, 0.1f, 1.9f), Quaternion.Euler(85f, 15f, 0f))
        {
            this._screen.name = "HUISavedFilterSettingsListScreen";

            this._animationHandler.LocalScale = 0.025f;
            this._animationHandler.UsePointerAnimations = false;
            this._animationHandler.PointerEntered += OnPointerEntered;
            this._animationHandler.PointerExited += OnPointerExited;

            // BSMLList needs a VRGraphicRaycaster, so we need to fix the PhysicsRaycaster,
            // just like what is done for FloatingScreen in ScreenManagerBase
            _savedFilterSettingsList.gameObject.FixRaycaster(physicsRaycaster);

            var icon = _pageUpButton.transform.Find("Content/Icon").GetComponent<ImageView>();
            icon.rectTransform.Rotate(0f, 0f, 180f, Space.Self);

            GameObject.Destroy(_pageUpButton.GetComponent<ContentSizeFitter>());
            GameObject.Destroy(_pageDownButton.GetComponent<ContentSizeFitter>());

            GameObject.Destroy(_pageUpButton.transform.Find("Underline").gameObject);
            GameObject.Destroy(_pageDownButton.transform.Find("Underline").gameObject);

            _pageUpButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _pageDownButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);

            // custom animations
            GameObject.Destroy(_pageUpButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_pageDownButton.GetComponent<ButtonStaticAnimations>());

            Color highlightedColour = new Color(1f, 0.375f, 0f);

            var iconBtnAnims = _pageUpButton.gameObject.AddComponent<CustomIconButtonAnimations>();
            iconBtnAnims.HighlightedBGColour = highlightedColour;
            iconBtnAnims.PressedBGColour = highlightedColour;
            iconBtnAnims.HighlightedLocalScale = new Vector3(1.2f, 1.2f, 1.2f);

            iconBtnAnims = _pageDownButton.gameObject.AddComponent<CustomIconButtonAnimations>();
            iconBtnAnims.HighlightedBGColour = highlightedColour;
            iconBtnAnims.PressedBGColour = highlightedColour;
            iconBtnAnims.HighlightedLocalScale = new Vector3(1.2f, 1.2f, 1.2f);

            // change icon size in buttons by changing the RectOffset
            var slg = _pageUpButton.transform.Find("Content").GetComponent<StackLayoutGroup>();
            slg.padding = new RectOffset(0, 0, 1, 1);

            slg = _pageDownButton.transform.Find("Content").GetComponent<StackLayoutGroup>();
            slg.padding = new RectOffset(0, 0, 1, 1);
        }

        protected override void OnLevelCollectionNavigationControllerActivated(bool firstActivation, bool addToHierarchy, bool screenSystemEnabling)
        {
            // do not show screen during activation
        }

        public void ShowScreen() => this._animationHandler.PlayRevealAnimation();

        public void HideScreen() => this._animationHandler.PlayConcealAnimation();

        internal void RefreshSavedFilterSettingsList(IEnumerable<SavedFilterSettings> savedFilterSettings)
        {
            _savedFilterSettingsList.data.Clear();

            foreach (SavedFilterSettings sf in savedFilterSettings)
                _savedFilterSettingsList.data.Add(new CustomListTableData.CustomCellInfo(sf.Name.EscapeTextMeshProTags()));

            _savedFilterSettingsList.tableView.ReloadData();
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
        private void OnSortModeListCellSelected(TableView tableView, int index)
        {
            this.CallAndHandleAction(SavedFilterSettingsApplied, nameof(SavedFilterSettingsApplied), index);
        }
    }
}
