using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using HUI.Interfaces;
using HUI.Utilities;
using HUIFilters.UI.Screens;

namespace HUIFilters.Filters
{
    public class FilterManager : IInitializable, IDisposable, ILevelCollectionModifier
    {
        public event Action<bool> LevelCollectionRefreshRequested;

        public int Priority => 0;

        private FilterWidgetScreenManager _filterWidgetScreenManager;
        private FilterSettingsScreenManager _filterSettingsScreenManager;

        private List<IFilter> _filters;

        [Inject]
        public FilterManager(
            FilterWidgetScreenManager filterWidgetScreenManager,
            FilterSettingsScreenManager filterSettingsScreenManager,
            List<IFilter> filters)
        {
            _filterWidgetScreenManager = filterWidgetScreenManager;
            _filterSettingsScreenManager = filterSettingsScreenManager;

            _filters = filters;
        }

        public void Initialize()
        {
            // TODO: load saved filters

            _filterWidgetScreenManager.FilterButtonPressed += OnWidgetFilterButtonPressed;
            _filterWidgetScreenManager.CancelFilterButtonPressed += UnapplyFilters;

            _filterSettingsScreenManager.FilterApplied += OnFilterSettingsFilterApplied;
            _filterSettingsScreenManager.FilterUnapplied += UnapplyFilters;
            _filterSettingsScreenManager.FilterReset += OnFilterSettingsFilterReset;
            _filterSettingsScreenManager.FilterCleared += OnFilterSettingsFilterCleared;
        }

        public void Dispose()
        {
            if (_filterWidgetScreenManager != null)
            {
                _filterWidgetScreenManager.FilterButtonPressed += OnWidgetFilterButtonPressed;
                _filterWidgetScreenManager.CancelFilterButtonPressed += UnapplyFilters;
            }

            if (_filterSettingsScreenManager != null)
            {
                _filterSettingsScreenManager.FilterApplied -= OnFilterSettingsFilterApplied;
                _filterSettingsScreenManager.FilterUnapplied -= UnapplyFilters;
                _filterSettingsScreenManager.FilterReset -= OnFilterSettingsFilterReset;
                _filterSettingsScreenManager.FilterCleared -= OnFilterSettingsFilterCleared;
            }
        }

        public void OnLevelCollectionSelected(IAnnotatedBeatmapLevelCollection annotatedBeatmapLevelCollection)
        {
            // no action needed
        }

        public bool ApplyModifications(IEnumerable<IPreviewBeatmapLevel> levelCollection, out IEnumerable<IPreviewBeatmapLevel> modifiedLevelCollection)
        {
            if (!_filters.Any(x => x.IsApplied))
            {
                modifiedLevelCollection = levelCollection;
                return false;
            }

            List<IPreviewBeatmapLevel> levels = levelCollection.ToList();
            foreach (var filter in _filters)
            {
                if (filter.IsApplied)
                    filter.FilterLevels(ref levels);
            }

            modifiedLevelCollection = levels;
            return true;
        }

        private void UnapplyFilters()
        {
            foreach (var filter in _filters)
                filter.ApplyDefaultValues();
        }

        private void OnWidgetFilterButtonPressed()
        {
            if (_filterSettingsScreenManager.IsVisible)
                _filterSettingsScreenManager.HideScreen();
            else
                _filterSettingsScreenManager.ShowScreen();
        }

        private void OnFilterSettingsFilterApplied()
        {
            foreach (var filter in _filters)
                filter.ApplyStagingValues();

            this.CallAndHandleAction(LevelCollectionRefreshRequested, nameof(LevelCollectionRefreshRequested), false);
        }

        private void OnFilterSettingsFilterReset()
        {
            foreach (var filter in _filters)
                filter.SetAppliedValuesToStaging();
        }

        private void OnFilterSettingsFilterCleared()
        {
            foreach (var filter in _filters)
                filter.SetDefaultValuesToStaging();
        }
    }
}
