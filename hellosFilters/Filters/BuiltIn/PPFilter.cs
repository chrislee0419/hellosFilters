using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using HUI.Utilities;
using HUIFilters.DataSources;
using HUIFilters.Utilities;

namespace HUIFilters.Filters.BuiltIn
{
    public sealed class PPFilter : NotifiableBSMLViewFilterBase, IInitializable, IDisposable
    {
        public override string Name => "PP";
        [UIValue("is-available")]
        public override bool IsAvailable => _scoreSaberDataSource.IsDataAvailable;
        [UIValue("is-not-available")]
        public bool IsNotAvailable => !IsAvailable;
        [UIValue("is-mod-available")]
        public bool IsModAvailable => !(_scoreSaberDataSource is EmptyDataSource);

        public override bool IsApplied => RankedAppliedValue != RankedOptions.Off;
        public override bool HasChanges =>
            RankedAppliedValue != _rankedStagingValue ||
                (_rankedStagingValue == RankedOptions.Ranked &&
                    (MinEnabledAppliedValue != _minEnabledStagingValue ||
                    MaxEnabledAppliedValue != _maxEnabledStagingValue ||
                        (_minEnabledStagingValue && MinAppliedValue != _minStagingValue) ||
                        (_maxEnabledStagingValue && MaxAppliedValue != _maxStagingValue)));

        private RankedOptions _rankedStagingValue = RankedOptions.Off;
        [UIValue("ranked-value")]
        public RankedOptions RankedStagingValue
        {
            get => _rankedStagingValue;
            set
            {
                if (_rankedStagingValue == value)
                    return;

                _rankedStagingValue = value;
                OnPropertyChanged();

                InvokePropertyChanged(nameof(EnabledSettingsInteractable));
                InvokePropertyChanged(nameof(MinSettingInteractable));
                InvokePropertyChanged(nameof(MaxSettingInteractable));
            }
        }

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

        public RankedOptions RankedAppliedValue { get; private set; } = RankedOptions.Off;
        public bool MinEnabledAppliedValue { get; private set; } = false;
        public bool MaxEnabledAppliedValue { get; private set; } = false;
        public int MinAppliedValue { get; private set; } = DefaultMinValue;
        public int MaxAppliedValue { get; private set; } = DefaultMaxValue;

        [UIValue("enabled-settings-interactable")]
        public bool EnabledSettingsInteractable => _rankedStagingValue == RankedOptions.Ranked;
        [UIValue("min-setting-interactable")]
        public bool MinSettingInteractable => _minEnabledStagingValue && EnabledSettingsInteractable;
        [UIValue("max-setting-interactable")]
        public bool MaxSettingInteractable => _maxEnabledStagingValue && EnabledSettingsInteractable;

        public int MinSettingMaxValue => _maxEnabledStagingValue ? _maxStagingValue - IncrementValue : MaximumValue;
        public int MaxSettingMinValue => _minEnabledStagingValue ? _minStagingValue + IncrementValue : MinimumValue;

        protected override string AssociatedBSMLFile => "HUIFilters.UI.Views.Filters.PPFilterView.bsml";

#pragma warning disable CS0649
        [UIComponent("min-setting")]
        private IncrementSetting _minSetting;
        [UIComponent("max-setting")]
        private IncrementSetting _maxSetting;
#pragma warning restore CS0649

        private IScoreSaberDataSource _scoreSaberDataSource;

        private const string RankedSettingName = "ranked";
        private const string MinEnabledSettingName = "minEnabled";
        private const string MaxEnabledSettingName = "maxEnabled";
        private const string MinSettingName = "minValue";
        private const string MaxSettingName = "maxValue";

        private const int DefaultMinValue = 100;
        private const int DefaultMaxValue = 200;

        [UIValue("inc-value")]
        private const int IncrementValue = 25;
        [UIValue("setting-min-value")]
        private const int MinimumValue = IncrementValue;
        [UIValue("setting-max-value")]
        private const int MaximumValue = 1000;

        [UIValue("ranked-options")]
        private static readonly List<object> RankedOptionsOptions = FilterUtilities.EnumerateEnumValuesAsObjectList<RankedOptions>();

        [Inject]
        public PPFilter(IScoreSaberDataSource scoreSaberDataSource)
        {
            _scoreSaberDataSource = scoreSaberDataSource;
        }

        public void Initialize()
        {
            _scoreSaberDataSource.AvailabilityChanged += OnScoreSaberDataSourceAvailabilityChanged;
        }

        public void Dispose()
        {
            if (_scoreSaberDataSource != null)
                _scoreSaberDataSource.AvailabilityChanged -= OnScoreSaberDataSourceAvailabilityChanged;
        }

        protected override void InternalSetDefaultValuesToStaging()
        {
            RankedStagingValue = RankedOptions.Off;
            MinEnabledStagingValue = false;
            MaxEnabledStagingValue = false;
            MinStagingValue = DefaultMinValue;
            MaxStagingValue = DefaultMaxValue;
        }

        protected override void InternalSetAppliedValuesToStaging()
        {
            RankedStagingValue = RankedAppliedValue;
            MinEnabledStagingValue = MinEnabledAppliedValue;
            MaxEnabledStagingValue = MaxEnabledAppliedValue;
            MinStagingValue = MinAppliedValue;
            MaxStagingValue = MaxAppliedValue;
        }

        protected override void InternalSetSavedValuesToStaging(IReadOnlyDictionary<string, string> settings)
        {
            int errorCount = 0;

            if (settings.TryGetValue(RankedSettingName, out string value) && Enum.TryParse(value, out RankedOptions enumValue))
                RankedStagingValue = enumValue;
            else
                Plugin.Log.Debug($"Unable to load '{RankedSettingName}' value for PP filter ({++errorCount})");

            if (settings.TryGetValue(MinEnabledSettingName, out value) && bool.TryParse(value, out bool boolValue))
                MinEnabledStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{MinEnabledSettingName}' value for PP filter ({++errorCount})");

            if (settings.TryGetValue(MaxEnabledSettingName, out value) && bool.TryParse(value, out boolValue))
                MaxEnabledStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{MaxEnabledSettingName}' value for PP filter ({++errorCount})");

            if (settings.TryGetValue(MinSettingName, out value) && int.TryParse(value, out int intValue))
                MinStagingValue = (intValue / IncrementValue) * IncrementValue;
            else
                Plugin.Log.Debug($"Unable to load '{MinSettingName}' value for PP filter ({++errorCount})");

            if (settings.TryGetValue(MaxSettingName, out value) && int.TryParse(value, out intValue))
                MaxStagingValue = (intValue / IncrementValue) * IncrementValue;
            else
                Plugin.Log.Debug($"Unable to load '{MaxSettingName}' value for PP filter ({++errorCount})");

            if (errorCount > 0)
                Plugin.Log.Warn("Some value(s) could not be loaded by the PP filter");
        }

        public override void ApplyStagingValues()
        {
            RankedAppliedValue = _rankedStagingValue;
            MinEnabledAppliedValue = _minEnabledStagingValue;
            MaxEnabledAppliedValue = _maxEnabledStagingValue;
            MinAppliedValue = _minStagingValue;
            MaxAppliedValue = _maxStagingValue;
        }

        public override void ApplyDefaultValues()
        {
            RankedAppliedValue = RankedOptions.Off;
            MinEnabledAppliedValue = false;
            MaxEnabledAppliedValue = false;
            MinAppliedValue = DefaultMinValue;
            MaxAppliedValue = DefaultMaxValue;
        }

        public override void FilterLevels(ref List<IPreviewBeatmapLevel> levels)
        {
            for (int i = 0; i < levels.Count;)
            {
                // handle ost levels first
                if (levels[i] is IBeatmapLevel)
                {
                    if (RankedAppliedValue == RankedOptions.Ranked)
                        levels.RemoveAt(i);
                    else
                        ++i;

                    continue;
                }

                var customLevel = levels[i] as CustomPreviewBeatmapLevel;
                if (customLevel == null ||
                    !_scoreSaberDataSource.ScoreSaberData.TryGetValue(BeatmapUtilities.GetCustomLevelHash(customLevel), out var scoreSaberData))
                {
                    Plugin.Log.Debug($"Unable to get pp values for '{levels[i].songName}' by '{levels[i].songAuthorName}'");

                    // if we can't get information on a map, consider it unranked
                    // (pretty sure a ranked map wouldn't somehow be not in the data anyways)
                    if (RankedAppliedValue == RankedOptions.Ranked)
                        levels.RemoveAt(i);
                    else
                        ++i;

                    continue;
                }

                var ppValues = scoreSaberData.Characteristics.Values
                    .SelectMany(x => x.Values)
                    .Select(x => x.PP);

                if (RankedAppliedValue == RankedOptions.Ranked)
                {
                    bool remove = ppValues.All(pp => 
                        pp == 0 ||
                        (MinEnabledAppliedValue && pp < MinAppliedValue) ||
                        (MaxEnabledAppliedValue && pp > MaxAppliedValue));
                    if (remove)
                    {
                        levels.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }
                else
                {
                    // keep unranked
                    if (ppValues.Any(x => x > 0))
                        levels.RemoveAt(i);
                    else
                        ++i;
                }
            }
        }

        public override IDictionary<string, string> GetAppliedSettings()
        {
            return new Dictionary<string, string>
            {
                { RankedSettingName, RankedAppliedValue.ToString() },
                { MinEnabledSettingName, MinEnabledAppliedValue.ToString() },
                { MaxEnabledSettingName, MaxEnabledAppliedValue.ToString() },
                { MinSettingName, MinAppliedValue.ToString() },
                { MaxSettingName, MaxAppliedValue.ToString() }
            };
        }

        private void OnScoreSaberDataSourceAvailabilityChanged()
        {
            InvokePropertyChanged(nameof(IsAvailable));
            InvokePropertyChanged(nameof(IsNotAvailable));

            InvokeAvailabilityChanged();
        }

        private void UpdateMinSettingIncrementButtonInteractable() => _minSetting.EnableInc = _minStagingValue < MinSettingMaxValue;

        private void UpdateMaxSettingDecrementButtonInteractable() => _maxSetting.EnableDec = _maxStagingValue > MaxSettingMinValue;

        [UIAction("ranked-formatter")]
        private string RankedFormatter(object obj)
        {
            var value = (RankedOptions)obj;

            switch (value)
            {
                case RankedOptions.Off:
                    return "Off";

                case RankedOptions.Ranked:
                    return "Keep ranked";

                case RankedOptions.Unranked:
                    return "Remove ranked";

                default:
                    return "Error";
            }
        }

        [UIAction("pp-formatter")]
        private string PPFormatter(float density) => density.ToString("0. 'pp'");

        public enum RankedOptions
        {
            Off,
            Ranked,
            Unranked
        }
    }
}
