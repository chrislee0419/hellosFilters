using System;
using System.Collections.Generic;
using Zenject;
using HUIFilters.UI.Screens;

namespace HUIFilters.Filters
{
    public class FilterManager : IInitializable, IDisposable
    {
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
            _filterWidgetScreenManager.CancelFilterButtonPressed += OnWidgetCancelFilterButtonPressed;
        }

        public void Dispose()
        {
            if (_filterWidgetScreenManager != null)
            {
                _filterWidgetScreenManager.FilterButtonPressed += OnWidgetFilterButtonPressed;
                _filterWidgetScreenManager.CancelFilterButtonPressed += OnWidgetCancelFilterButtonPressed;
            }
        }

        private void OnWidgetFilterButtonPressed()
        {
            if (_filterSettingsScreenManager.IsVisible)
                _filterSettingsScreenManager.HideScreen();
            else
                _filterSettingsScreenManager.ShowScreen();
        }

        private void OnWidgetCancelFilterButtonPressed()
        {

        }
    }
}
