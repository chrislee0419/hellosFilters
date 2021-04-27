using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HUI.Utilities;

namespace HUIFilters.Filters
{
    public interface IFilter
    {
        event Action SettingChanged;

        string Name { get; }
        bool IsAvailable { get; }

        bool IsApplied { get; }
        bool HasChanges { get; }

        void ShowView(GameObject parentGO);
        void HideView();

        void SetDefaultValuesToStaging();
        void SetAppliedValuesToStaging();
        void SetSavedValuesToStaging(IReadOnlyDictionary<string, string> settings);
        void ApplyStagingValues();
        void ApplyDefaultValues();

        void FilterLevels(ref List<IPreviewBeatmapLevel> levels);

        IDictionary<string, string> GetAppliedSettings();
    }

    public abstract class BSMLViewFilterBase : IFilter
    {
        public event Action SettingChanged;

        public abstract string Name { get; }
        public virtual bool IsAvailable => true;

        public abstract bool IsApplied { get; }
        public abstract bool HasChanges { get; }

        protected abstract string AssociatedBSMLFile { get; }

        [UIObject("root")]
        protected GameObject _viewGO;

        protected void InvokeSettingChanged() => this.CallAndHandleAction(SettingChanged, nameof(SettingChanged));

        public virtual void ShowView(GameObject parentGO)
        {
            if (_viewGO != null)
            {
                if (parentGO != _viewGO.transform.parent.gameObject)
                    _viewGO.transform.SetParent(parentGO.transform, false);

                _viewGO.SetActive(true);
                return;
            }

            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), AssociatedBSMLFile), parentGO, this);
        }

        public virtual void HideView() => _viewGO?.SetActive(false);

        public abstract void SetDefaultValuesToStaging();

        public abstract void SetAppliedValuesToStaging();

        public abstract void SetSavedValuesToStaging(IReadOnlyDictionary<string, string> settings);

        public abstract void ApplyStagingValues();

        public abstract void ApplyDefaultValues();

        public abstract void FilterLevels(ref List<IPreviewBeatmapLevel> levels);

        public abstract IDictionary<string, string> GetAppliedSettings();
    }
}
