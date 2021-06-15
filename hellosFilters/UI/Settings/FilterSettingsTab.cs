using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using UnityEngine;
using HMUI;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HUI.Attributes;
using HUI.Interfaces;
using HUI.UI.Components;
using HUI.UI.CustomBSML.Components;
using HUI.Utilities;
using HUIFilters.Filters;

namespace HUIFilters.UI.Settings
{
    [AutoInstall]
    public class FilterSettingsTab : SettingsModalTabBase
    {
        public event Action SavedFilterSettingsListChanged;

        public override string TabName => "Filter";
        protected override string AssociatedBSMLResource => "HUIFilters.UI.Views.FilterSettingsView.bsml";

        [UIValue("top-buttons-interactable")]
        public bool AnyChanges => _savedSettingsListCells.Any(x => x.HasChanges);

        [UIValue("up-buttons-interactable")]
        public bool UpButtonsInteractable => (_selectedSavedSettings?.Index ?? 0) > 0;
        [UIValue("down-buttons-interactable")]
        public bool DownButtonsInteractable
        {
            get
            {
                int count = _savedSettingsListCells.Count;
                return (_selectedSavedSettings?.Index ?? count) < count - 1;
            }
        }

        [UIValue("delete-button-interactable")]
        public bool DeleteButtonInteractable => _selectedSavedSettings != null;
        [UIValue("delete-button-text")]
        public string DeleteButtonText
        {
            get
            {
                if (_selectedSavedSettings == null)
                    return "Delete";
                else
                    return _selectedSavedSettings.Delete ? "Keep" : "Delete";
            }
        }

#pragma warning disable CS0649
        [UIComponent("saved-settings-list")]
        private HUICustomCellListTableData _savedSettingsListTableData;

        [UIObject("top-button")]
        private GameObject _topButton;
        [UIObject("up-button")]
        private GameObject _upButton;
        [UIObject("down-button")]
        private GameObject _downButton;
        [UIObject("bottom-button")]
        private GameObject _bottomButton;

        [UIObject("list-up-button")]
        private GameObject _listUpButton;
        [UIObject("list-down-button")]
        private GameObject _listDownButton;
#pragma warning restore CS0649

        private List<SavedFilterSettingsListCell> _savedSettingsListCells = new List<SavedFilterSettingsListCell>();
        private SavedFilterSettingsListCell _selectedSavedSettings;

        public override void SetupView()
        {
            base.SetupView();

            GameObject.Destroy(_listUpButton.transform.Find("Underline").gameObject);
            GameObject.Destroy(_listDownButton.transform.Find("Underline").gameObject);

            // remove skew
            _listUpButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _listDownButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);

            _topButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _topButton.transform.Find("Underline").GetComponent<ImageView>().SetSkew(0f);
            _upButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _upButton.transform.Find("Underline").GetComponent<ImageView>().SetSkew(0f);
            _downButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _downButton.transform.Find("Underline").GetComponent<ImageView>().SetSkew(0f);
            _bottomButton.transform.Find("BG").GetComponent<ImageView>().SetSkew(0f);
            _bottomButton.transform.Find("Underline").GetComponent<ImageView>().SetSkew(0f);

            // reduce padding
            var offset = new RectOffset(0, 0, 4, 4);
            _listUpButton.transform.Find("Content").GetComponent<StackLayoutGroup>().padding = offset;
            _listDownButton.transform.Find("Content").GetComponent<StackLayoutGroup>().padding = offset;

            offset = new RectOffset();
            _topButton.GetComponent<StackLayoutGroup>().padding = offset;
            _upButton.GetComponent<StackLayoutGroup>().padding = offset;
            _downButton.GetComponent<StackLayoutGroup>().padding = offset;
            _bottomButton.GetComponent<StackLayoutGroup>().padding = offset;

            offset = new RectOffset(1, 1, 1, 1);
            _topButton.transform.Find("Content").GetComponent<StackLayoutGroup>().padding = offset;
            _bottomButton.transform.Find("Content").GetComponent<StackLayoutGroup>().padding = offset;

            offset = new RectOffset(2, 2, 2, 2);
            _upButton.transform.Find("Content").GetComponent<StackLayoutGroup>().padding = offset;
            _downButton.transform.Find("Content").GetComponent<StackLayoutGroup>().padding = offset;

            // rotate images
            _listUpButton.GetComponent<ButtonIconImage>().image.rectTransform.Rotate(0f, 0f, 180f, Space.Self);
            _downButton.GetComponent<ButtonIconImage>().image.rectTransform.Rotate(0f, 0f, 180f, Space.Self);
            _bottomButton.GetComponent<ButtonIconImage>().image.rectTransform.Rotate(0f, 0f, 180f, Space.Self);

            // custom button animations
            GameObject.Destroy(_listUpButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_listDownButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_topButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_upButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_downButton.GetComponent<ButtonStaticAnimations>());
            GameObject.Destroy(_bottomButton.GetComponent<ButtonStaticAnimations>());

            Color ListMoveColour = new Color(0.145f, 0.443f, 1f);

            var btnAnims = _listUpButton.AddComponent<CustomIconButtonAnimations>();
            btnAnims.HighlightedBGColour = ListMoveColour;
            btnAnims.PressedBGColour = ListMoveColour;

            btnAnims = _listDownButton.AddComponent<CustomIconButtonAnimations>();
            btnAnims.HighlightedBGColour = ListMoveColour;
            btnAnims.PressedBGColour = ListMoveColour;

            Vector3 HighlighedScale = new Vector3(1.2f, 1.2f, 1.2f);
            btnAnims = _topButton.AddComponent<CustomIconButtonAnimations>();
            btnAnims.HighlightedLocalScale = HighlighedScale;

            btnAnims = _upButton.AddComponent<CustomIconButtonAnimations>();
            btnAnims.HighlightedLocalScale = HighlighedScale;

            btnAnims = _downButton.AddComponent<CustomIconButtonAnimations>();
            btnAnims.HighlightedLocalScale = HighlighedScale;

            btnAnims = _bottomButton.AddComponent<CustomIconButtonAnimations>();
            btnAnims.HighlightedLocalScale = HighlighedScale;

            SetListData();
        }

        public void RefreshSavedFilterSettingsList()
        {
            if (_parserParams != null)
                SetListData();
        }

        private void SetListData()
        {
            _savedSettingsListCells.Clear();
            _savedSettingsListCells.AddRange(PluginConfig.Instance.SavedFilterSettings.Select(savedSettings => new SavedFilterSettingsListCell(savedSettings)));
            UpdateListIndices();

            _savedSettingsListTableData.data = _savedSettingsListCells.Select(x => (object)x).ToList();
            _savedSettingsListTableData.tableView.ReloadData();
            _savedSettingsListTableData.tableView.ClearSelection();

            _selectedSavedSettings = null;

            NotifyPropertyChanged(nameof(AnyChanges));
            NotifyPropertyChanged(nameof(UpButtonsInteractable));
            NotifyPropertyChanged(nameof(DownButtonsInteractable));
            NotifyPropertyChanged(nameof(DeleteButtonInteractable));
            NotifyPropertyChanged(nameof(DeleteButtonText));
        }

        private void RefreshCellPositions()
        {
            UpdateListIndices();
            _savedSettingsListCells.Sort();

            _savedSettingsListTableData.data = _savedSettingsListCells.Select(x => (object)x).ToList();
            _savedSettingsListTableData.tableView.ReloadData();

            if (_selectedSavedSettings != null)
            {
                _savedSettingsListTableData.tableView.ScrollToCellWithIdx(_selectedSavedSettings.Index, TableView.ScrollPositionType.Center, false);
                _savedSettingsListTableData.tableView.SelectCellWithIdx(_selectedSavedSettings.Index);

                NotifyPropertyChanged(nameof(UpButtonsInteractable));
                NotifyPropertyChanged(nameof(DownButtonsInteractable));
                NotifyPropertyChanged(nameof(DeleteButtonInteractable));
            }
        }

        private void UpdateListIndices()
        {
            for (int i = 0; i < _savedSettingsListCells.Count; ++i)
                _savedSettingsListCells[i].Index = i;
        }

        [UIAction("cell-selected")]
        private void OnCellSelected(TableView tableView, object data)
        {
            _selectedSavedSettings = (SavedFilterSettingsListCell)data;

            NotifyPropertyChanged(nameof(UpButtonsInteractable));
            NotifyPropertyChanged(nameof(DownButtonsInteractable));
            NotifyPropertyChanged(nameof(DeleteButtonInteractable));
            NotifyPropertyChanged(nameof(DeleteButtonText));
        }

        [UIAction("apply-button-clicked")]
        private void OnApplyButtonClicked()
        {
            if (!AnyChanges)
            {
                NotifyPropertyChanged(nameof(AnyChanges));
                return;
            }

            var savedSettingsList = PluginConfig.Instance.SavedFilterSettings;
            savedSettingsList.Clear();

            foreach (var savedSettingsCell in _savedSettingsListCells.Where(x => !x.Delete))
                savedSettingsList.Add(savedSettingsCell.SavedSettings);

            this.CallAndHandleAction(SavedFilterSettingsListChanged, nameof(SavedFilterSettingsListChanged));
        }

        [UIAction("undo-button-clicked")]
        private void OnUndoButtonClicked() => SetListData();

        private bool IsCellSelected()
        {
            if (_selectedSavedSettings == null)
            {
                NotifyPropertyChanged(nameof(UpButtonsInteractable));
                NotifyPropertyChanged(nameof(DownButtonsInteractable));
                NotifyPropertyChanged(nameof(DeleteButtonInteractable));
                NotifyPropertyChanged(nameof(AnyChanges));

                return false;
            }

            return true;
        }

        [UIAction("top-button-clicked")]
        private void OnTopButtonClicked()
        {
            if (IsCellSelected())
            {
                _savedSettingsListCells.Remove(_selectedSavedSettings);
                _savedSettingsListCells.Insert(0, _selectedSavedSettings);

                RefreshCellPositions();
                NotifyPropertyChanged(nameof(AnyChanges));
            }
        }

        [UIAction("up-button-clicked")]
        private void OnUpButtonClicked()
        {
            if (IsCellSelected())
            {
                int index = _selectedSavedSettings.Index - 1;
                if (index < 0)
                    index = 0;

                _savedSettingsListCells.Remove(_selectedSavedSettings);
                _savedSettingsListCells.Insert(index, _selectedSavedSettings);

                RefreshCellPositions();
                NotifyPropertyChanged(nameof(AnyChanges));
            }
        }

        [UIAction("down-button-clicked")]
        private void OnDownButtonClicked()
        {
            if (IsCellSelected())
            {
                int index = _selectedSavedSettings.Index + 1;
                if (index >= _savedSettingsListCells.Count)
                    index = _savedSettingsListCells.Count - 1;

                _savedSettingsListCells.Remove(_selectedSavedSettings);
                _savedSettingsListCells.Insert(index, _selectedSavedSettings);

                RefreshCellPositions();
                NotifyPropertyChanged(nameof(AnyChanges));
            }
        }

        [UIAction("bottom-button-clicked")]
        private void OnBottomButtonClicked()
        {
            if (IsCellSelected())
            {
                _savedSettingsListCells.Remove(_selectedSavedSettings);
                _savedSettingsListCells.Add(_selectedSavedSettings);

                RefreshCellPositions();
                NotifyPropertyChanged(nameof(AnyChanges));
            }
        }

        [UIAction("delete-button-clicked")]
        private void OnDeleteButtonClicked()
        {
            if (IsCellSelected())
            {
                _selectedSavedSettings.Delete = !_selectedSavedSettings.Delete;
                NotifyPropertyChanged(nameof(DeleteButtonText));
                NotifyPropertyChanged(nameof(AnyChanges));
            }
        }

        private class SavedFilterSettingsListCell : INotifyPropertyChanged, IComparable<SavedFilterSettingsListCell>
        {
            public event PropertyChangedEventHandler PropertyChanged;

            [UIValue("name-text")]
            public string NameText => SavedSettings.Name;

            [UIValue("status-text")]
            public string StatusText
            {
                get
                {
                    _statusStringBuilder.Clear();

                    if (_delete)
                        _statusStringBuilder.Append("<size=80%><color=#B08060>[Delete]</color></size>  ");

                    if (PluginConfig.Instance.SavedFilterSettings.IndexOf(SavedSettings) == _index)
                    {
                        _statusStringBuilder.Append(_index + 1);
                    }
                    else
                    {
                        _statusStringBuilder.Append("<color=#FFBBAA>");
                        _statusStringBuilder.Append(_index + 1);
                        _statusStringBuilder.Append("</color>");
                    }

                    return _statusStringBuilder.ToString();
                }
            }

            private bool _delete = false;
            public bool Delete
            {
                get => _delete;
                set
                {
                    if (_delete == value)
                        return;

                    _delete = value;
                    NotifyPropertyChanged(nameof(StatusText));
                }
            }

            private int _index;
            public int Index
            {
                get => _index;
                set
                {
                    if (_index == value)
                        return;

                    _index = value;
                    NotifyPropertyChanged(nameof(StatusText));
                }
            }

            public bool HasChanges => PluginConfig.Instance.SavedFilterSettings.IndexOf(SavedSettings) != _index;

            public SavedFilterSettings SavedSettings { get; private set; }

            private StringBuilder _statusStringBuilder = new StringBuilder();

            public SavedFilterSettingsListCell(SavedFilterSettings savedSettings)
            {
                if (savedSettings == null)
                    throw new ArgumentNullException(nameof(savedSettings));

                SavedSettings = savedSettings;
            }

            public int CompareTo(SavedFilterSettingsListCell other) => this.Index - other.Index;

            private void NotifyPropertyChanged([CallerMemberName] string propertyName = null) => this.CallAndHandleAction(PropertyChanged, propertyName);
        }
    }
}
