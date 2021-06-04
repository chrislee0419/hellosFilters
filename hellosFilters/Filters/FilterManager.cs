using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Zenject;
using HUI.Interfaces;
using HUI.Utilities;
using HUIFilters.UI.Screens;
using HUIFilters.Utilities;

namespace HUIFilters.Filters
{
    public class FilterManager : IInitializable, IDisposable, ILevelCollectionModifier
    {
        public event Action<bool> LevelCollectionRefreshRequested;

        public int Priority => 0;

        private FilterWidgetScreenManager _filterWidgetScreenManager;
        private SavedFilterSettingsListScreenManager _savedFilterSettingsListScreenManager;
        private FilterSettingsScreenManager _filterSettingsScreenManager;

        private bool _availabilityChangedThisFrame = false;

        private List<IFilter> _filters;

        [Inject]
        public FilterManager(
            FilterWidgetScreenManager filterWidgetScreenManager,
            SavedFilterSettingsListScreenManager savedFilterSettingsListScreenManager,
            FilterSettingsScreenManager filterSettingsScreenManager,
            List<IFilter> filters)
        {
            _filterWidgetScreenManager = filterWidgetScreenManager;
            _savedFilterSettingsListScreenManager = savedFilterSettingsListScreenManager;
            _filterSettingsScreenManager = filterSettingsScreenManager;

            _filters = filters;
        }

        public void Initialize()
        {
            _filterWidgetScreenManager.FilterButtonPressed += OnWidgetFilterButtonPressed;
            _filterWidgetScreenManager.SavedFilterSettingsListButtonPressed += OnWidgetSavedFilterSettingsListButtonPressed;
            _filterWidgetScreenManager.CancelFilterButtonPressed += OnWidgetCancelFilterButtonPressed;

            _savedFilterSettingsListScreenManager.SavedFilterSettingsApplied += OnSavedFilterSettingsApplied;

            _filterSettingsScreenManager.FilterApplied += ApplyFilters;
            _filterSettingsScreenManager.FilterUnapplied += UnapplyFilters;
            _filterSettingsScreenManager.FilterReset += OnFilterSettingsFilterReset;
            _filterSettingsScreenManager.FilterCleared += OnFilterSettingsFilterCleared;

            foreach (var filter in _filters)
                filter.AvailabilityChanged += OnFilterAvailabilityChanged;
        }

        public void Dispose()
        {
            if (_filterWidgetScreenManager != null)
            {
                _filterWidgetScreenManager.FilterButtonPressed += OnWidgetFilterButtonPressed;
                _filterWidgetScreenManager.SavedFilterSettingsListButtonPressed -= OnWidgetSavedFilterSettingsListButtonPressed;
                _filterWidgetScreenManager.CancelFilterButtonPressed += OnWidgetCancelFilterButtonPressed;
            }

            if (_savedFilterSettingsListScreenManager != null)
                _savedFilterSettingsListScreenManager.SavedFilterSettingsApplied -= OnSavedFilterSettingsApplied;

            if (_filterSettingsScreenManager != null)
            {
                _filterSettingsScreenManager.FilterApplied -= ApplyFilters;
                _filterSettingsScreenManager.FilterUnapplied -= UnapplyFilters;
                _filterSettingsScreenManager.FilterReset -= OnFilterSettingsFilterReset;
                _filterSettingsScreenManager.FilterCleared -= OnFilterSettingsFilterCleared;
            }

            if (_filters != null)
            {
                foreach (var filter in _filters)
                {
                    if (filter != null)
                        filter.AvailabilityChanged -= OnFilterAvailabilityChanged;
                }
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
                Plugin.Log.DebugOnly($"No modifications done to the level collection by {nameof(FilterManager)} (no filters are applied)");

                modifiedLevelCollection = levelCollection;
                return false;
            }

            List<IPreviewBeatmapLevel> levels = levelCollection.ToList();

#if DEBUG
            Stopwatch sw = Stopwatch.StartNew();
            int levelCount = levels.Count;
#endif

            foreach (var filter in _filters)
            {
                if (filter.IsApplied)
                    filter.FilterLevels(ref levels);
            }

#if DEBUG
            sw.Stop();
            Plugin.Log.Info($"Filters applied in {sw.ElapsedMilliseconds}ms (kept {levels.Count} out of {levelCount} levels)");
#endif

            modifiedLevelCollection = levels;
            return true;
        }

        private void ApplyFilters()
        {
            foreach (var filter in _filters)
                filter.ApplyStagingValues();

            _filterWidgetScreenManager.FilterApplied = true;
            _filterSettingsScreenManager.UpdateFilterStatus();

            this.CallAndHandleAction(LevelCollectionRefreshRequested, nameof(LevelCollectionRefreshRequested), false);
        }

        private void UnapplyFilters()
        {
            foreach (var filter in _filters)
                filter.ApplyDefaultValues();

            _filterWidgetScreenManager.FilterApplied = false;
            _filterSettingsScreenManager.UpdateFilterStatus();

            this.CallAndHandleAction(LevelCollectionRefreshRequested, nameof(LevelCollectionRefreshRequested), false);
        }

        private void OnWidgetFilterButtonPressed()
        {
            if (_filterSettingsScreenManager.IsVisible)
                _filterSettingsScreenManager.HideScreen();
            else
                _filterSettingsScreenManager.ShowScreen();
        }

        private void OnWidgetSavedFilterSettingsListButtonPressed()
        {
            if (_savedFilterSettingsListScreenManager.IsVisible)
                _savedFilterSettingsListScreenManager.HideScreen();
            else
                _savedFilterSettingsListScreenManager.ShowScreen();
        }

        private void OnWidgetCancelFilterButtonPressed()
        {
            if (_filters.Any(x => x.IsApplied))
                UnapplyFilters();
        }

        private void OnSavedFilterSettingsApplied(SavedFilterSettings savedSettings)
        {
            foreach (var filter in _filters)
            {
                filter.SetDefaultValuesToStaging();

                if (savedSettings.FilterSettings.TryGetValue(filter.GetIdentifier(), out var settings))
                    filter.SetSavedValuesToStaging(settings);
            }

            ApplyFilters();
        }

        private void OnFilterSettingsFilterReset()
        {
            foreach (var filter in _filters)
                filter.SetAppliedValuesToStaging();

            _filterSettingsScreenManager.UpdateFilterStatus();
        }

        private void OnFilterSettingsFilterCleared()
        {
            foreach (var filter in _filters)
                filter.SetDefaultValuesToStaging();

            _filterSettingsScreenManager.UpdateFilterStatus();
        }

        private void OnFilterAvailabilityChanged()
        {
            if (!_availabilityChangedThisFrame)
            {
                _availabilityChangedThisFrame = true;

                CoroutineUtilities.StartDelayedAction(delegate ()
                {
                    _availabilityChangedThisFrame = false;
                    _filterSettingsScreenManager.UpdateFiltersDropdownList();
                });
            }
        }
    }
}
