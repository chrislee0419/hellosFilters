using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HUI.Utilities;
using BSMLUtilities = BeatSaberMarkupLanguage.Utilities;

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

    /// <summary>
    /// An incomplete implementation of <see cref="IFilter"/>
    /// that uses a BSML file to represent the view.
    /// </summary>
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

            BSMLParser.instance.Parse(BSMLUtilities.GetResourceContent(Assembly.GetExecutingAssembly(), AssociatedBSMLFile), parentGO, this);

            _viewGO.name = $"{this.GetType().Name}View";
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

    /// <summary>
    /// A version of <see cref="BSMLViewFilterBase"/> that implements <see cref="INotifyPropertyChanged"/>.
    /// 
    /// <para>
    /// Make sure to override the <see cref="InternalSetDefaultValuesToStaging"/> and
    /// <see cref="InternalSetAppliedValuesToStaging"/> functions instead of the
    /// <see cref="SetDefaultValuesToStaging"/> and <see cref="SetAppliedValuesToStaging"/>.
    /// </para>
    /// </summary>
    public abstract class NotifiableBSMLViewFilterBase : BSMLViewFilterBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool IsStagingValuesFromUI { get; private set; } = true;

        protected void InvokePropertyChanged([CallerMemberName] string propertyName = null) => this.CallAndHandleAction(PropertyChanged, propertyName);

        /// <summary>
        /// Use this function in the setter of each property that your BSML view will use to 
        /// trigger the PropertyChanged event or SettingChanged event.
        /// 
        /// <para>
        /// This function will ensure values changed in the UI by the user will not fire the
        /// <see cref="PropertyChanged"/> event (and force BSML to unnecessarily get the value)
        /// and values changed in code will not fire the <see cref="BSMLViewFilterBase.SettingChanged"/> event
        /// (so HUI doesn't have to deal with this event when the user resets or clears all filters).
        /// </para>
        /// 
        /// <para>
        /// Make sure to override the <see cref="InternalSetDefaultValuesToStaging"/> and
        /// <see cref="InternalSetAppliedValuesToStaging"/> functions instead of the
        /// <see cref="SetDefaultValuesToStaging"/> and <see cref="SetAppliedValuesToStaging"/>.
        /// </para>
        /// </summary>
        /// <param name="propertyName">The name of the property that has been changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (IsStagingValuesFromUI)
                InvokeSettingChanged();
            else
                InvokePropertyChanged(propertyName);
        }

        public override void SetDefaultValuesToStaging()
        {
            IsStagingValuesFromUI = false;

            InternalSetDefaultValuesToStaging();

            IsStagingValuesFromUI = true;
        }

        public override void SetAppliedValuesToStaging()
        {
            IsStagingValuesFromUI = false;

            InternalSetAppliedValuesToStaging();

            IsStagingValuesFromUI = true;
        }

        public override void SetSavedValuesToStaging(IReadOnlyDictionary<string, string> settings)
        {
            IsStagingValuesFromUI = false;

            InternalSetDefaultValuesToStaging();
            InternalSetSavedValuesToStaging(settings);

            IsStagingValuesFromUI = true;
        }

        protected abstract void InternalSetDefaultValuesToStaging();
        protected abstract void InternalSetAppliedValuesToStaging();
        protected abstract void InternalSetSavedValuesToStaging(IReadOnlyDictionary<string, string> settings);
    }
}
