using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Zenject;
using SongCore;
using BeatSaberMarkupLanguage.Attributes;
using HUI.Utilities;
using HUIFilters.DataSources;

namespace HUIFilters.Filters.BuiltIn
{
    public class CharacteristicFilter : NotifiableBSMLViewFilterBase, IInitializable
    {
        public override string Name => "Characteristics";
        public override bool IsAvailable => true;

        public override bool IsApplied =>
            OneSaberAppliedValue ||
            NoArrowsAppliedValue ||
            Mode90DegreeAppliedValue ||
            Mode360DegreeAppliedValue ||
            LightshowAppliedValue ||
            CustomCharacteristics.Any(x => x.RequiredAppliedValue);
        public override bool HasChanges =>
            OneSaberAppliedValue != OneSaberStagingValue ||
            NoArrowsAppliedValue != NoArrowsStagingValue ||
            Mode90DegreeAppliedValue != Mode90DegreeStagingValue ||
            Mode360DegreeAppliedValue != Mode360DegreeStagingValue ||
            LightshowAppliedValue != LightshowStagingValue ||
            CustomCharacteristics.Any(x => x.RequiredAppliedValue != x.RequiredStagingValue);

        [UIValue("data-source-available")]
        public bool DataSourceAvailable => _beatmapDataSource.IsDataAvailable;

        private bool _oneSaberStagingValue = false;
        [UIValue("one-saber-value")]
        public bool OneSaberStagingValue
        {
            get => _oneSaberStagingValue;
            set
            {
                if (_oneSaberStagingValue == value)
                    return;

                _oneSaberStagingValue = value;
                OnPropertyChanged();
            }
        }
        private bool _noArrowsSettingName = false;
        [UIValue("no-arrows-value")]
        public bool NoArrowsStagingValue
        {
            get => _noArrowsSettingName;
            set
            {
                if (_noArrowsSettingName == value)
                    return;

                _noArrowsSettingName = value;
                OnPropertyChanged();
            }
        }
        private bool _mode90DegreeStagingValue = false;
        [UIValue("90-degree-value")]
        public bool Mode90DegreeStagingValue
        {
            get => _mode90DegreeStagingValue;
            set
            {
                if (_mode90DegreeStagingValue == value)
                    return;

                _mode90DegreeStagingValue = value;
                OnPropertyChanged();
            }
        }
        private bool _mode360DegreeStagingValue = false;
        [UIValue("360-degree-value")]
        public bool Mode360DegreeStagingValue
        {
            get => _mode360DegreeStagingValue;
            set
            {
                if (_mode360DegreeStagingValue == value)
                    return;

                _mode360DegreeStagingValue = value;
                OnPropertyChanged();
            }
        }
        private bool _lightshowStagingValue = false;
        [UIValue("lightshow-value")]
        public bool LightshowStagingValue
        {
            get => _lightshowStagingValue;
            set
            {
                if (_lightshowStagingValue == value)
                    return;

                _lightshowStagingValue = value;
                OnPropertyChanged();
            }
        }

        public bool OneSaberAppliedValue { get; private set; } = false;
        public bool NoArrowsAppliedValue { get; private set; } = false;
        public bool Mode90DegreeAppliedValue { get; private set; } = false;
        public bool Mode360DegreeAppliedValue { get; private set; } = false;
        public bool LightshowAppliedValue { get; private set; } = false;

        [UIValue("custom-characteristics-list")]
        public CustomCharacteristic[] CustomCharacteristics { get; private set; }

        protected override string AssociatedBSMLFile => "HUIFilters.UI.Views.Filters.CharacteristicFilterView.bsml";

        private IBeatmapDataSource _beatmapDataSource;

        private HashSet<string> _allCharacteristics = null;

        private const string OneSaberSettingName = "oneSaber";
        private const string NoArrowsSettingName = "noArrows";
        private const string Mode90DegreeSettingName = "90";
        private const string Mode360DegreeSettingName = "360";
        private const string LightshowSettingName = "lightshow";

        public const string StandardSerializedName = "Standard";
        public const string OneSaberSerializedName = "OneSaber";
        public const string NoArrowsSerializedName = "NoArrows";
        public const string Mode90DegreeSerializedName = "90Degree";
        public const string Mode360DegreeSerializedName = "360Degree";
        public const string LightshowSerializedName = "Lightshow";
        public const string MissingCustomCharacteristicSerializedName = "MissingCharacteristic";

        [Inject]
        public CharacteristicFilter(IBeatmapDataSource beatmapDataSource)
        {
            _beatmapDataSource = beatmapDataSource;
        }

        public void Initialize()
        {
            CustomCharacteristics = Collections.customCharacteristics
                .Where(x => x.serializedName != LightshowSerializedName && x.serializedName != MissingCustomCharacteristicSerializedName)
                .Select(x => new CustomCharacteristic(x))
                .ToArray();

            foreach (var customCharacteristic in CustomCharacteristics)
            {
                customCharacteristic.SettingChanged += delegate (CustomCharacteristic characteristic)
                {
                    if (this.IsStagingValuesFromUI)
                        InvokeSettingChanged();
                    else
                        characteristic.InvokeRequiredPropertyChanged();
                };
            }
        }

        protected override void InternalSetDefaultValuesToStaging()
        {
            OneSaberStagingValue = false;
            NoArrowsStagingValue = false;
            Mode90DegreeStagingValue = false;
            Mode360DegreeStagingValue = false;
            LightshowStagingValue = false;

            foreach (var customCharacteristic in CustomCharacteristics)
                customCharacteristic.RequiredStagingValue = false;
        }

        protected override void InternalSetAppliedValuesToStaging()
        {
            OneSaberStagingValue = OneSaberAppliedValue;
            NoArrowsStagingValue = NoArrowsAppliedValue;
            Mode90DegreeStagingValue = Mode90DegreeAppliedValue;
            Mode360DegreeStagingValue = Mode360DegreeAppliedValue;
            LightshowStagingValue = LightshowAppliedValue;

            foreach (var customCharacteristic in CustomCharacteristics)
                customCharacteristic.RequiredStagingValue = customCharacteristic.RequiredAppliedValue;
        }

        protected override void InternalSetSavedValuesToStaging(IReadOnlyDictionary<string, string> settings)
        {
            int errorCount = 0;

            if (settings.TryGetValue(OneSaberSettingName, out string value) && bool.TryParse(value, out bool boolValue))
                OneSaberStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{OneSaberSettingName}' value for Characteristic filter ({++errorCount})");

            if (settings.TryGetValue(NoArrowsSettingName, out value) && bool.TryParse(value, out boolValue))
                NoArrowsStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{NoArrowsSettingName}' value for Characteristic filter ({++errorCount})");

            if (settings.TryGetValue(Mode90DegreeSettingName, out value) && bool.TryParse(value, out boolValue))
                Mode90DegreeStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{Mode90DegreeSettingName}' value for Characteristic filter ({++errorCount})");

            if (settings.TryGetValue(Mode360DegreeSettingName, out value) && bool.TryParse(value, out boolValue))
                Mode360DegreeStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{Mode360DegreeSettingName}' value for Characteristic filter ({++errorCount})");

            if (settings.TryGetValue(LightshowSettingName, out value) && bool.TryParse(value, out boolValue))
                LightshowStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{LightshowSettingName}' value for Characteristic filter ({++errorCount})");

            foreach (var customCharacteristic in CustomCharacteristics)
            {
                if (settings.TryGetValue(customCharacteristic.CharacteristicSerializedName, out value) && bool.TryParse(value, out boolValue))
                    customCharacteristic.RequiredStagingValue = boolValue;
                else
                    Plugin.Log.Debug($"Unable to load '{customCharacteristic.CharacteristicSerializedName}' value for Characteristic filter ({++errorCount})");
            }

            if (errorCount > 0)
                Plugin.Log.Warn("Some value(s) could not be loaded by the Characteristic filter");
        }

        public override void ApplyStagingValues()
        {
            OneSaberAppliedValue = OneSaberStagingValue;
            NoArrowsAppliedValue = NoArrowsStagingValue;
            Mode90DegreeAppliedValue = Mode90DegreeStagingValue;
            Mode360DegreeAppliedValue = Mode360DegreeStagingValue;
            LightshowAppliedValue = LightshowStagingValue;

            foreach (var customCharacteristic in CustomCharacteristics)
                customCharacteristic.RequiredAppliedValue = customCharacteristic.RequiredStagingValue;
        }

        public override void ApplyDefaultValues()
        {
            OneSaberAppliedValue = false;
            NoArrowsAppliedValue = false;
            Mode90DegreeAppliedValue = false;
            Mode360DegreeAppliedValue = false;
            LightshowAppliedValue = false;

            foreach (var customCharacteristic in CustomCharacteristics)
                customCharacteristic.RequiredAppliedValue = false;
        }

        public override void FilterLevels(ref List<IPreviewBeatmapLevel> levels)
        {
            HashSet<string> levelCharacteristics = new HashSet<string>();

            for (int i = 0; i < levels.Count;)
            {
                foreach (var characteristic in levels[i].previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic.serializedName))
                    levelCharacteristics.Add(characteristic);

                if ((OneSaberAppliedValue && levelCharacteristics.Contains(OneSaberSerializedName)) ||
                    (NoArrowsAppliedValue && levelCharacteristics.Contains(NoArrowsSerializedName)) ||
                    (Mode90DegreeAppliedValue && levelCharacteristics.Contains(Mode90DegreeSerializedName)) ||
                    (Mode360DegreeAppliedValue && levelCharacteristics.Contains(Mode360DegreeSerializedName)) ||
                    (LightshowAppliedValue && (levelCharacteristics.Contains(LightshowSerializedName) || LightshowExistsForLevel(levels[i]))) ||
                    CustomCharacteristics.Any(x => x.RequiredAppliedValue && levelCharacteristics.Contains(x.CharacteristicSerializedName)))
                {
                    ++i;
                }
                else
                {
                    levels.RemoveAt(i);
                }

                levelCharacteristics.Clear();
            }
        }

        public override IDictionary<string, string> GetAppliedSettings()
        {
            var settings = new Dictionary<string, string>
            {
                { OneSaberSettingName, OneSaberAppliedValue.ToString() },
                { NoArrowsSettingName, NoArrowsAppliedValue.ToString() },
                { Mode90DegreeSettingName, Mode90DegreeAppliedValue.ToString() },
                { Mode360DegreeSettingName, Mode360DegreeAppliedValue.ToString() },
                { LightshowSettingName, LightshowAppliedValue.ToString() }
            };

            foreach (var customCharacteristic in CustomCharacteristics)
                settings.Add(customCharacteristic.CharacteristicSerializedName, customCharacteristic.RequiredAppliedValue.ToString());

            return settings;
        }

        /// <summary>
        /// Get the serialized names of all characteristics that are applied or all possible characteristics 
        /// if this filter is not applied.
        /// </summary>
        /// <returns>A <see cref="HashSet{string}"/> containing the serialized names of characteristics that pass the filter.</returns>
        public HashSet<string> GetSerializedNamesOfAppliedCharacteristics()
        {
            if (!this.IsApplied)
            {
                if (_allCharacteristics == null)
                {
                    _allCharacteristics = new HashSet<string>(6 + CustomCharacteristics.Length);

                    _allCharacteristics.Add(StandardSerializedName);
                    _allCharacteristics.Add(OneSaberSerializedName);
                    _allCharacteristics.Add(NoArrowsSerializedName);
                    _allCharacteristics.Add(Mode90DegreeSerializedName);
                    _allCharacteristics.Add(Mode360DegreeSerializedName);
                    _allCharacteristics.Add(LightshowSerializedName);

                    foreach (var customCharacteristic in CustomCharacteristics)
                        _allCharacteristics.Add(customCharacteristic.CharacteristicSerializedName);
                }

                return _allCharacteristics;
            }

            HashSet<string> characteristics = new HashSet<string>();
            if (OneSaberAppliedValue)
                characteristics.Add(OneSaberSerializedName);

            if (NoArrowsAppliedValue)
                characteristics.Add(NoArrowsSerializedName);

            if (Mode90DegreeAppliedValue)
                characteristics.Add(Mode90DegreeSerializedName);

            if (Mode360DegreeAppliedValue)
                characteristics.Add(Mode360DegreeSerializedName);

            if (LightshowAppliedValue)
                characteristics.Add(LightshowSerializedName);

            foreach (var customCharacteristic in CustomCharacteristics)
            {
                if (customCharacteristic.RequiredAppliedValue)
                    characteristics.Add(customCharacteristic.CharacteristicSerializedName);
            }

            return characteristics;
        }

        private bool LightshowExistsForLevel(IPreviewBeatmapLevel level)
        {
            return _beatmapDataSource.IsDataAvailable &&
                level is CustomPreviewBeatmapLevel customLevel &&
                _beatmapDataSource.BeatmapData.TryGetValue(BeatmapUtilities.GetCustomLevelHash(customLevel), out var metadata) &&
                metadata.Characteristics.Values.Any(x => x.Values.Any(y => y.NoteCount == 0));
        }

        public class CustomCharacteristic : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public event Action<CustomCharacteristic> SettingChanged;

            private bool _requiredStagingValue = false;
            [UIValue("required-value")]
            public bool RequiredStagingValue
            {
                get => _requiredStagingValue;
                internal set
                {
                    if (_requiredStagingValue == value)
                        return;

                    _requiredStagingValue = value;
                    this.CallAndHandleAction(SettingChanged, nameof(SettingChanged), this);
                }
            }

            public bool RequiredAppliedValue { get; internal set; } = false;

            public string CharacteristicSerializedName { get; private set; }
            [UIValue("characteristic-name")]
            public string CharacteristicName { get; private set; }

            public CustomCharacteristic(BeatmapCharacteristicSO characteristic)
            {
                CharacteristicName = characteristic.characteristicNameLocalizationKey;
                CharacteristicSerializedName = characteristic.serializedName;
            }

            internal void InvokeRequiredPropertyChanged() => this.CallAndHandleAction(PropertyChanged, nameof(RequiredStagingValue));
        }
    }
}
