using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace HUIFilters.Filters.BuiltIn
{
    public sealed class DurationFilter : NotifiableBSMLViewFilterBase
    {
        public override string Name => "Song Length";
        public override bool IsAvailable => true;

        public override bool IsApplied => MinEnabledAppliedValue || MaxEnabledAppliedValue;
        public override bool HasChanges => 
            MinEnabledAppliedValue != _minEnabledStagingValue ||
            MaxEnabledAppliedValue != _maxEnabledStagingValue ||
            (_minEnabledStagingValue && MinAppliedValue != _minStagingValue) ||
            (_maxEnabledStagingValue && MaxAppliedValue != _maxStagingValue);

        private bool _minEnabledStagingValue = false;
        [UIValue("min-enabled-value")]
        public bool MinEnabledStagingValue
        {
            get => _minEnabledStagingValue;
            set
            {
                if (_minEnabledStagingValue == value)
                    return;

                _minEnabledStagingValue = value;

                InvokePropertyChanged(nameof(MinSettingInteractable));

                if (_maxEnabledStagingValue)
                {
                    if (_minStagingValue > MinSettingMaxValue)
                    {
                        _minStagingValue = MinSettingMaxValue;
                        InvokePropertyChanged(nameof(MinStagingValue));
                    }

                    UpdateMaxSettingDecrementButtonInteractable();
                }

                if (value)
                    UpdateMinSettingIncrementButtonInteractable();

                OnPropertyChanged();
            }
        }
        private bool _maxEnabledStagingValue = false;
        [UIValue("max-enabled-value")]
        public bool MaxEnabledStagingValue
        {
            get => _maxEnabledStagingValue;
            set
            {
                if (_maxEnabledStagingValue == value)
                    return;

                _maxEnabledStagingValue = value;

                InvokePropertyChanged(nameof(MaxSettingInteractable));

                if (_minEnabledStagingValue)
                {
                    if (_maxStagingValue < MaxSettingMinValue)
                    {
                        _maxStagingValue = MaxSettingMinValue;
                        InvokePropertyChanged(nameof(MaxStagingValue));
                    }

                    UpdateMinSettingIncrementButtonInteractable();
                }

                if (value)
                    UpdateMaxSettingDecrementButtonInteractable();

                OnPropertyChanged();
            }
        }
        private int _minStagingValue = DefaultMinValue;
        [UIValue("min-value")]
        public int MinStagingValue
        {
            get => _minStagingValue;
            set
            {
                if (_minStagingValue == value)
                    return;

                _minStagingValue = value;

                UpdateMinSettingIncrementButtonInteractable();
                if (_maxEnabledStagingValue)
                    UpdateMaxSettingDecrementButtonInteractable();

                OnPropertyChanged();
            }
        }
        private int _maxStagingValue = DefaultMaxValue;
        [UIValue("max-value")]
        public int MaxStagingValue
        {
            get => _maxStagingValue;
            set
            {
                if (_maxStagingValue == value)
                    return;

                _maxStagingValue = value;

                UpdateMaxSettingDecrementButtonInteractable();
                if (_minEnabledStagingValue)
                    UpdateMinSettingIncrementButtonInteractable();

                OnPropertyChanged();
            }
        }

        public bool MinEnabledAppliedValue { get; private set; } = false;
        public bool MaxEnabledAppliedValue { get; private set; } = false;
        public int MinAppliedValue { get; private set; } = DefaultMinValue;
        public int MaxAppliedValue { get; private set; } = DefaultMaxValue;

        [UIValue("min-setting-interactable")]
        public bool MinSettingInteractable => _minEnabledStagingValue;
        [UIValue("max-setting-interactable")]
        public bool MaxSettingInteractable => _maxEnabledStagingValue;

        public int MinSettingMaxValue => _maxEnabledStagingValue ? _maxStagingValue - IncrementValue : MaximumValue;
        public int MaxSettingMinValue => _minEnabledStagingValue ? _minStagingValue + IncrementValue : MinimumValue;

        protected override string AssociatedBSMLFile => "HUIFilters.UI.Views.Filters.DurationFilterView.bsml";

#pragma warning disable CS0649
        [UIComponent("min-setting")]
        private IncrementSetting _minSetting;
        [UIComponent("max-setting")]
        private IncrementSetting _maxSetting;
#pragma warning restore CS0649

        private const string MinEnabledSettingName = "minEnabled";
        private const string MaxEnabledSettingName = "maxEnabled";
        private const string MinSettingName = "minValue";
        private const string MaxSettingName = "maxValue";

        private const int DefaultMinValue = 60;
        private const int DefaultMaxValue = 120;

        [UIValue("inc-value")]
        private const int IncrementValue = 15;
        [UIValue("setting-min-value")]
        private const int MinimumValue = IncrementValue;
        [UIValue("setting-max-value")]
        private const int MaximumValue = 60 * 60;

        protected override void InternalSetDefaultValuesToStaging()
        {
            MinEnabledStagingValue = false;
            MaxEnabledStagingValue = false;
            MinStagingValue = DefaultMinValue;
            MaxStagingValue = DefaultMaxValue;
        }

        protected override void InternalSetAppliedValuesToStaging()
        {
            MinEnabledStagingValue = MinEnabledAppliedValue;
            MaxEnabledStagingValue = MaxEnabledAppliedValue;
            MinStagingValue = MinAppliedValue;
            MaxStagingValue = MaxAppliedValue;
        }

        protected override void InternalSetSavedValuesToStaging(IReadOnlyDictionary<string, string> settings)
        {
            int errorCount = 0;

            if (settings.TryGetValue(MinEnabledSettingName, out string value) && bool.TryParse(value, out bool boolValue))
                MinEnabledStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{MinEnabledSettingName}' value for Duration filter ({++errorCount})");

            if (settings.TryGetValue(MaxEnabledSettingName, out value) && bool.TryParse(value, out boolValue))
                MaxEnabledStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{MaxEnabledSettingName}' value for Duration filter ({++errorCount})");

            if (settings.TryGetValue(MinSettingName, out value) && int.TryParse(value, out int intValue))
                MinStagingValue = (intValue / IncrementValue) * IncrementValue;
            else
                Plugin.Log.Debug($"Unable to load '{MinSettingName}' value for Duration filter ({++errorCount})");

            if (settings.TryGetValue(MaxSettingName, out value) && int.TryParse(value, out intValue))
                MaxStagingValue = (intValue / IncrementValue) * IncrementValue;
            else
                Plugin.Log.Debug($"Unable to load '{MaxSettingName}' value for Duration filter ({++errorCount})");

            if (errorCount > 0)
                Plugin.Log.Warn("Some value(s) could not be loaded by the Duration filter");
        }

        public override void ApplyStagingValues()
        {
            MinEnabledAppliedValue = _minEnabledStagingValue;
            MaxEnabledAppliedValue = _maxEnabledStagingValue;
            MinAppliedValue = _minStagingValue;
            MaxAppliedValue = _maxStagingValue;
        }

        public override void ApplyDefaultValues()
        {
            MinEnabledAppliedValue = false;
            MaxEnabledAppliedValue = false;
            MinAppliedValue = DefaultMinValue;
            MaxAppliedValue = DefaultMaxValue;
        }

        public override void FilterLevels(ref List<IPreviewBeatmapLevel> levels)
        {
            for (int i = 0; i < levels.Count;)
            {
                float songDuration = levels[i].songDuration;

                if ((MinEnabledAppliedValue && songDuration < MinAppliedValue) ||
                    (MaxEnabledAppliedValue && songDuration > MaxAppliedValue))
                {
                    levels.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        public override IDictionary<string, string> GetAppliedSettings()
        {
            return new Dictionary<string, string>
            {
                { MinEnabledSettingName, MinEnabledAppliedValue.ToString() },
                { MaxEnabledSettingName, MaxEnabledAppliedValue.ToString() },
                { MinSettingName, MinAppliedValue.ToString() },
                { MaxSettingName, MaxAppliedValue.ToString() }
            };
        }

        private void UpdateMinSettingIncrementButtonInteractable()
        {
            if (_minSetting != null)
                _minSetting.EnableInc = _minStagingValue < MinSettingMaxValue;
        }

        private void UpdateMaxSettingDecrementButtonInteractable()
        {
            if (_maxSetting != null)
                _maxSetting.EnableDec = _maxStagingValue > MaxSettingMinValue;
        }

        [UIAction("duration-formatter")]
        private string DurationFormatter(int duration) => TimeSpan.FromSeconds(duration).ToString("m':'ss");
    }
}
