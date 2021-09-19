using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using HUI.Utilities;
using HUIFilters.DataSources;

namespace HUIFilters.Filters.BuiltIn
{
    public sealed class NoteDensityFilter : NotifiableBSMLViewFilterBase, IInitializable, IDisposable
    {
        public override string Name => "Note Density";
        [UIValue("is-available")]
        public override bool IsAvailable => _beatmapDataSource.IsDataAvailable;

        [UIValue("is-not-available")]
        public bool IsNotAvailable => !IsAvailable;
        [UIValue("is-mod-available")]
        public bool IsModAvailable => !(_beatmapDataSource is EmptyDataSource);

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
        private float _minStagingValue = DefaultMinValue;
        [UIValue("min-value")]
        public float MinStagingValue
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
        private float _maxStagingValue = DefaultMaxValue;
        [UIValue("max-value")]
        public float MaxStagingValue
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
        public float MinAppliedValue { get; private set; } = DefaultMinValue;
        public float MaxAppliedValue { get; private set; } = DefaultMaxValue;

        [UIValue("min-setting-interactable")]
        public bool MinSettingInteractable => _minEnabledStagingValue;
        [UIValue("max-setting-interactable")]
        public bool MaxSettingInteractable => _maxEnabledStagingValue;

        public float MinSettingMaxValue => _maxEnabledStagingValue ? _maxStagingValue - IncrementValue : MaximumValue;
        public float MaxSettingMinValue => _minEnabledStagingValue ? _minStagingValue + IncrementValue : MinimumValue;

        protected override string AssociatedBSMLFile => "HUIFilters.UI.Views.Filters.NoteDensityFilterView.bsml";

#pragma warning disable CS0649
        [UIComponent("min-setting")]
        private IncrementSetting _minSetting;
        [UIComponent("max-setting")]
        private IncrementSetting _maxSetting;
#pragma warning restore CS0649

        private IBeatmapDataSource _beatmapDataSource;

        private const string MinEnabledSettingName = "minEnabled";
        private const string MaxEnabledSettingName = "maxEnabled";
        private const string MinSettingName = "minValue";
        private const string MaxSettingName = "maxValue";

        private const float DefaultMinValue = 4;
        private const float DefaultMaxValue = 6;

        [UIValue("inc-value")]
        private const float IncrementValue = 0.5f;
        [UIValue("setting-min-value")]
        private const float MinimumValue = IncrementValue;
        [UIValue("setting-max-value")]
        private const float MaximumValue = 30f;

        [Inject]
        public NoteDensityFilter(IBeatmapDataSource beatmapDataSource)
        {
            _beatmapDataSource = beatmapDataSource;
        }

        public void Initialize()
        {
            _beatmapDataSource.AvailabilityChanged += OnBeatmapDataSourceAvailabilityChanged;
        }

        public void Dispose()
        {
            if (_beatmapDataSource != null)
                _beatmapDataSource.AvailabilityChanged -= OnBeatmapDataSourceAvailabilityChanged;
        }

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
                Plugin.Log.Debug($"Unable to load '{MinEnabledSettingName}' value for Note Density filter ({++errorCount})");

            if (settings.TryGetValue(MaxEnabledSettingName, out value) && bool.TryParse(value, out boolValue))
                MaxEnabledStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{MaxEnabledSettingName}' value for Note Density filter ({++errorCount})");

            if (settings.TryGetValue(MinSettingName, out value) && float.TryParse(value, out float floatValue))
                MinStagingValue = (float)(Math.Round(floatValue / IncrementValue) * IncrementValue);
            else
                Plugin.Log.Debug($"Unable to load '{MinSettingName}' value for Note Density filter ({++errorCount})");

            if (settings.TryGetValue(MaxSettingName, out value) && float.TryParse(value, out floatValue))
                MaxStagingValue = (float)(Math.Round(floatValue / IncrementValue) * IncrementValue);
            else
                Plugin.Log.Debug($"Unable to load '{MaxSettingName}' value for Note Density filter ({++errorCount})");

            if (errorCount > 0)
                Plugin.Log.Warn("Some value(s) could not be loaded by the Note Density filter");
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
                IEnumerable<float> noteDensities;
                if (levels[i] is CustomPreviewBeatmapLevel customLevel)
                {
                    string levelHash = BeatmapUtilities.GetCustomLevelHash(customLevel);
                    if (!_beatmapDataSource.BeatmapData.TryGetValue(levelHash, out var beatmapMetadata))
                    {
                        Plugin.Log.Debug($"Unable to get note densities for '{levels[i].songName}' by '{levels[i].songAuthorName}'");

                        ++i;
                        continue;
                    }

                    noteDensities = beatmapMetadata.Characteristics.Values
                        .SelectMany(x => x.Values)
                        .Select(x => x.NoteCount / customLevel.songDuration);
                }
                else if (levels[i] is IBeatmapLevel ostLevel)
                {
                    noteDensities = ostLevel.beatmapLevelData.difficultyBeatmapSets
                        .SelectMany(x => x.difficultyBeatmaps)
                        .Select(x => x.beatmapData.cuttableNotesCount / ostLevel.songDuration);
                }
                else
                {
                    Plugin.Log.Debug($"Unable to get note densities for '{levels[i].songName}' by '{levels[i].songAuthorName}'");

                    ++i;
                    continue;
                }

                if (noteDensities.All(density => (MinEnabledAppliedValue && density < MinAppliedValue) || (MaxEnabledAppliedValue && density > MaxAppliedValue)))
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

        private void OnBeatmapDataSourceAvailabilityChanged()
        {
            this.InvokePropertyChanged(nameof(IsAvailable));
            this.InvokePropertyChanged(nameof(IsNotAvailable));

            InvokeAvailabilityChanged();
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

        [UIAction("note-density-formatter")]
        private string NoteDensityFormatter(float density) => density.ToString("0.0 'NPS'");
    }
}
