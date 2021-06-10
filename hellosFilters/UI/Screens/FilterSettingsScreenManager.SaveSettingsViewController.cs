using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using HMUI;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HUI.UI.Components;
using HUI.UI.CustomBSML.Components;
using HUI.Utilities;
using HUIFilters.Filters;
using HUIFilters.UI.Components;
using BSMLUtilities = BeatSaberMarkupLanguage.Utilities;

namespace HUIFilters.UI.Screens
{
    public partial class FilterSettingsScreenManager
    {
        private class SaveSettingsViewController : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public event Action<string> SavedSettingsCreated;
            public event Action<SavedFilterSettings> SavedSettingsOverwritten;

            public bool IsVisible => _rootContainer.activeSelf;

            private bool _introViewActive = true;
            [UIValue("intro-view-active")]
            public bool IntroViewActive
            {
                get => _introViewActive;
                set
                {
                    if (_introViewActive == value)
                        return;

                    _introViewActive = value;
                    NotifyPropertyChanged();
                }
            }
            private bool _newSavedSettingsViewActive = false;
            [UIValue("new-saved-settings-view-active")]
            public bool NewSavedSettingsViewActive
            {
                get => _newSavedSettingsViewActive;
                set
                {
                    if (_newSavedSettingsViewActive == value)
                        return;

                    _newSavedSettingsViewActive = value;
                    NotifyPropertyChanged();
                }
            }
            private bool _overwriteSavedSettingsViewActive = false;
            [UIValue("overwrite-saved-settings-view-active")]
            public bool OverwriteSavedSettingsViewActive
            {
                get => _overwriteSavedSettingsViewActive;
                set
                {
                    if (_overwriteSavedSettingsViewActive == value)
                        return;

                    _overwriteSavedSettingsViewActive = value;
                    NotifyPropertyChanged();
                }
            }
            private bool _errorTextBoxActive = false;
            [UIValue("error-text-box-active")]
            public bool ErrorTextBoxActive
            {
                get => _errorTextBoxActive;
                set
                {
                    if (_errorTextBoxActive == value)
                        return;

                    _errorTextBoxActive = value;
                    NotifyPropertyChanged();
                }
            }

            private string _saveSettingsName = "";
            [UIValue("save-settings-name-value")]
            public string NewSaveSettingsName
            {
                get => _saveSettingsName;
                set
                {
                    if (_saveSettingsName == value)
                        return;

                    _saveSettingsName = value;
                    NotifyPropertyChanged();
                }
            }

            private bool _newSaveButtonInteractable = false;
            [UIValue("new-save-button-interactable")]
            public bool NewSaveButtonInteractable
            {
                get => _newSaveButtonInteractable;
                set
                {
                    if (_newSaveButtonInteractable == value)
                        return;

                    _newSaveButtonInteractable = value;
                    NotifyPropertyChanged();
                }
            }

            private bool _overwriteSaveButtonInteractable = false;
            [UIValue("overwrite-save-button-interactable")]
            public bool OverwriteSaveButtonInteractable
            {
                get => _overwriteSaveButtonInteractable;
                set
                {
                    if (_overwriteSaveButtonInteractable == value)
                        return;

                    _overwriteSaveButtonInteractable = value;
                    NotifyPropertyChanged();
                }
            }

#pragma warning disable CS0649
            [UIObject("root")]
            public GameObject _rootContainer;
            [UIObject("keyboard-container")]
            public GameObject _keyboardContainer;

            [UIComponent("applied-filters-list-text")]
            private TextMeshProUGUI _appliedFiltersListText;
            [UIComponent("new-error-text")]
            private TextMeshProUGUI _newErrorText;
            [UIComponent("overwrite-error-text")]
            private TextMeshProUGUI _overwriteErrorText;

            [UIComponent("overwrite-list")]
            private HUICustomCellListTableData _overwriteList;

            [UIObject("overwrite-list-page-up-button")]
            private GameObject _overwriteListPageUpButton;
            [UIObject("overwrite-list-page-down-button")]
            private GameObject _overwriteListPageDownButton;
#pragma warning restore CS0649

            private FilterSettingsScreenManager _filterSettingsScreenManager;

            private SaveSettingsKeyboard _saveSettingsKeyboard;
            private StringBuilder _newSaveStringBuilder;

            private const string NamePlaceholderText = "<color=#808080><i>Enter a name...</i></color>";
            private const string CaretText = "<color=#AAAAFF>|</color>";

            public SaveSettingsViewController(FilterSettingsScreenManager filterSettingsScreenManager, GameObject parent)
            {
                _filterSettingsScreenManager = filterSettingsScreenManager;

                BSMLParser.instance.Parse(BSMLUtilities.GetResourceContent(Assembly.GetExecutingAssembly(), "HUIFilters.UI.Views.SaveSettingsView.bsml"), parent, this);

                _rootContainer.name = "SaveSettingsView";

                GameObject.Destroy(_overwriteListPageUpButton.transform.Find("Underline").gameObject);
                GameObject.Destroy(_overwriteListPageDownButton.transform.Find("Underline").gameObject);

                // remove skew
                _overwriteListPageUpButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
                _overwriteListPageDownButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);

                // reduce padding
                var offset = new RectOffset(0, 0, 4, 4);
                _overwriteListPageUpButton.transform.Find("Content").GetComponent<StackLayoutGroup>().padding = offset;
                _overwriteListPageDownButton.transform.Find("Content").GetComponent<StackLayoutGroup>().padding = offset;

                // rotate image
                _overwriteListPageUpButton.GetComponent<ButtonIconImage>().image.rectTransform.Rotate(0f, 0f, 180f, Space.Self);

                // custom button animations
                GameObject.Destroy(_overwriteListPageUpButton.GetComponent<ButtonStaticAnimations>());
                GameObject.Destroy(_overwriteListPageDownButton.GetComponent<ButtonStaticAnimations>());

                Color ListMoveColour = new Color(0.145f, 0.443f, 1f);
                Vector3 ListMoveHighlightedLocalScale = new Vector3(1.2f, 1.2f, 1.2f);

                var btnAnims = _overwriteListPageUpButton.AddComponent<CustomIconButtonAnimations>();
                btnAnims.HighlightedBGColour = ListMoveColour;
                btnAnims.PressedBGColour = ListMoveColour;
                btnAnims.HighlightedLocalScale = ListMoveHighlightedLocalScale;

                btnAnims = _overwriteListPageDownButton.AddComponent<CustomIconButtonAnimations>();
                btnAnims.HighlightedBGColour = ListMoveColour;
                btnAnims.PressedBGColour = ListMoveColour;
                btnAnims.HighlightedLocalScale = ListMoveHighlightedLocalScale;
            }

            public void ShowView()
            {
                if (_filterSettingsScreenManager._filters.Any(x => x.IsApplied))
                {
                    StringBuilder sb = new StringBuilder("<color=#DDFFDD>Currently applied filters:</color>  <i>");
                    foreach (var filter in _filterSettingsScreenManager._filters)
                    {
                        if (filter.IsApplied)
                        {
                            if (filter.HasChanges)
                            {
                                sb.Append("<color=#DDDDFF>");
                                sb.Append(filter.Name.EscapeTextMeshProTags());
                                sb.Append("</color>, ");
                            }
                            else
                            {
                                sb.Append(filter.Name.EscapeTextMeshProTags());
                                sb.Append(", ");
                            }
                        }
                    }

                    sb.Remove(sb.Length - 2, 2);
                    sb.Append("</i>");

                    _appliedFiltersListText.text = sb.ToString();
                }
                else
                {
                    _appliedFiltersListText.text = "No filters are currently applied!";
                }

                IntroViewActive = true;
                NewSavedSettingsViewActive = false;
                OverwriteSavedSettingsViewActive = false;

                _rootContainer.SetActive(true);
            }

            public void HideView() => _rootContainer.SetActive(false);

            public void UpdateSavedFilterSettingsList()
            {
                _overwriteList.data = PluginConfig.Instance.SavedFilterSettings.Select(x => (object)new SavedFilterSettingsListCell(x)).ToList();
                _overwriteList.tableView.ReloadData();
            }

            [UIAction("create-new-button-clicked")]
            private void OnCreateNewButtonClicked()
            {
                if (_saveSettingsKeyboard == null)
                {
                    _keyboardContainer.name = "SaveSettingsKeyboardContainer";
                    _keyboardContainer.AddComponent<LayoutElement>();

                    _saveSettingsKeyboard = _filterSettingsScreenManager._container.InstantiateComponent<SaveSettingsKeyboard>(_keyboardContainer);
                    _saveSettingsKeyboard.KeyPressed += delegate (char character)
                    {
                        _newSaveStringBuilder.Append(character);
                        NewSaveSettingsName = _newSaveStringBuilder.ToString().EscapeTextMeshProTags() + CaretText;
                        NewSaveButtonInteractable = true;
                    };
                    _saveSettingsKeyboard.DeleteButtonPressed += delegate ()
                    {
                        if (_newSaveStringBuilder.Length <= 1)
                        {
                            _newSaveStringBuilder.Clear();
                            NewSaveButtonInteractable = false;
                            NewSaveSettingsName = NamePlaceholderText;
                        }
                        else
                        {
                            _newSaveStringBuilder.Remove(_newSaveStringBuilder.Length - 1, 1);
                            NewSaveSettingsName = _newSaveStringBuilder.ToString().EscapeTextMeshProTags() + CaretText;
                        }
                    };
                    _saveSettingsKeyboard.ClearButtonPressed += delegate ()
                    {
                        _newSaveStringBuilder.Clear();
                        NewSaveButtonInteractable = false;
                        NewSaveSettingsName = NamePlaceholderText;
                    };

                    _newSaveStringBuilder = new StringBuilder();
                }

                IntroViewActive = false;
                NewSavedSettingsViewActive = true;
                OverwriteSavedSettingsViewActive = false;

                _newSaveStringBuilder.Clear();
                NewSaveSettingsName = NamePlaceholderText;
                NewSaveButtonInteractable = false;

                ErrorTextBoxActive = false;
                _newErrorText.text = "";
            }

            [UIAction("overwrite-existing-button-clicked")]
            private void OnOverwriteExistingButtonClicked()
            {
                UpdateSavedFilterSettingsList();

                IntroViewActive = false;
                NewSavedSettingsViewActive = false;
                OverwriteSavedSettingsViewActive = true;

                _overwriteList.tableView.ClearSelection();
                OverwriteSaveButtonInteractable = false;

                ErrorTextBoxActive = false;
                _overwriteErrorText.text = "";
            }

            [UIAction("back-button-clicked")]
            private void OnBackButtonClicked() => ShowView();

            [UIAction("new-save-button-clicked")]
            private void OnNewSaveButtonClicked()
            {
                if (!_filterSettingsScreenManager._filters.Any(x => x.IsApplied))
                {
                    _newErrorText.text = "<color=#FFAAAA>Unable to save settings!</color> (At least one filter needs to be applied)";
                    ErrorTextBoxActive = true;

                    return;
                }
                else if (_newSaveStringBuilder.Length == 0)
                {
                    _newErrorText.text = "<color=#FFAAAA>Unable to save settings!</color> (Name cannot be empty)";
                    ErrorTextBoxActive = true;

                    return;
                }

                string name = _newSaveStringBuilder.ToString();
                if (string.IsNullOrWhiteSpace(name))
                {
                    _newErrorText.text = "<color=#FFAAAA>Unable to save settings!</color> (Name cannot be empty)";
                    ErrorTextBoxActive = true;

                    return;
                }
                else if (PluginConfig.Instance.SavedFilterSettings.Any(x => x.Name == name))
                {
                    _newErrorText.text = "<color=#FFAAAA>Unable to save settings!</color> (The provided name is the same as an existing Saved Settings slot)";
                    ErrorTextBoxActive = true;

                    return;
                }

                this.CallAndHandleAction(SavedSettingsCreated, nameof(SavedSettingsCreated), name);
            }

            [UIAction("overwrite-save-button-clicked")]
            private void OnOverwriteSaveButtonClicked()
            {
                if (!_filterSettingsScreenManager._filters.Any(x => x.IsApplied))
                {
                    _overwriteErrorText.text = "<color=#FFAAAA>Unable to save settings!</color>\n(At least one filter needs to be applied)";
                    ErrorTextBoxActive = true;

                    return;
                }

                var selectedIndices = _overwriteList.tableView.GetSelectedIndices();
                if (selectedIndices.Count == 0)
                {
                    _overwriteErrorText.text = "<color=#FFAAAA>Unable to save settings!</color>\n(Select an existing Saved Settings slot to overwrite)";
                    ErrorTextBoxActive = true;

                    return;
                }

                var savedSettings = (_overwriteList.data[selectedIndices.First()] as SavedFilterSettingsListCell).SavedSettings;

                this.CallAndHandleAction(SavedSettingsOverwritten, nameof(SavedSettingsOverwritten), savedSettings);
            }

            [UIAction("overwrite-list-cell-selected")]
            private void OnOverwriteListCellSelected(TableView tableView, SavedFilterSettingsListCell savedSettingsListCell)
            {
                OverwriteSaveButtonInteractable = true;
            }

            private void NotifyPropertyChanged([CallerMemberName] string propertyName = null) => this.CallAndHandleAction(PropertyChanged, propertyName);

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
}
