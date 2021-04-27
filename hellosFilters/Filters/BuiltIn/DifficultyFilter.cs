using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;

namespace HUIFilters.Filters.BuiltIn
{
    internal class DifficultyFilter : BSMLViewFilterBase
    {
        public override string Name => "Difficulty";
        public override bool IsAvailable => true;

        public override bool IsApplied =>
            _easyAppliedValue ||
            _normalAppliedValue ||
            _hardAppliedValue ||
            _expertAppliedValue ||
            _expertPlusAppliedValue;
        public override bool HasChanges =>
            _easyAppliedValue != _easyStagingValue ||
            _normalAppliedValue != _normalStagingValue ||
            _hardAppliedValue != _hardStagingValue ||
            _expertAppliedValue != _expertStagingValue ||
            _expertPlusAppliedValue != _expertPlusStagingValue;

        private bool _easyStagingValue = false;
        [UIValue("easy-value")]
        public bool EasyStagingValue
        {
            get => _easyStagingValue;
            set
            {
                if (_easyStagingValue == value)
                    return;

                _easyStagingValue = value;

                if (_settingMultipleStagingValues)
                    InvokePropertyChanged();
                else
                    InvokeSettingChanged();
            }
        }
        private bool _normalStagingValue = false;
        [UIValue("normal-value")]
        public bool NormalStagingValue
        {
            get => _normalStagingValue;
            set
            {
                if (_normalStagingValue == value)
                    return;

                _normalStagingValue = value;

                if (_settingMultipleStagingValues)
                    InvokePropertyChanged();
                else
                    InvokeSettingChanged();
            }
        }
        private bool _hardStagingValue = false;
        [UIValue("hard-value")]
        public bool HardStagingValue
        {
            get => _hardStagingValue;
            set
            {
                if (_hardStagingValue == value)
                    return;

                _hardStagingValue = value;

                if (_settingMultipleStagingValues)
                    InvokePropertyChanged();
                else
                    InvokeSettingChanged();
            }
        }
        private bool _expertStagingValue = false;
        [UIValue("expert-value")]
        public bool ExpertStagingValue
        {
            get => _expertStagingValue;
            set
            {
                if (_expertStagingValue == value)
                    return;

                _expertStagingValue = value;

                if (_settingMultipleStagingValues)
                    InvokePropertyChanged();
                else
                    InvokeSettingChanged();
            }
        }
        private bool _expertPlusStagingValue = false;
        [UIValue("expert-plus-value")]
        public bool ExpertPlusStagingValue
        {
            get => _expertPlusStagingValue;
            set
            {
                if (_expertPlusStagingValue == value)
                    return;

                _expertPlusStagingValue = value;

                if (_settingMultipleStagingValues)
                    InvokePropertyChanged();
                else
                    InvokeSettingChanged();
            }
        }

        protected override string AssociatedBSMLFile => "HUIFilters.UI.Views.Filters.DifficultyFilterView.bsml";

        private bool _easyAppliedValue = false;
        private bool _normalAppliedValue = false;
        private bool _hardAppliedValue = false;
        private bool _expertAppliedValue = false;
        private bool _expertPlusAppliedValue = false;

        private bool _settingMultipleStagingValues = false;

        private const string EasySettingName = "easy";
        private const string NormalSettingName = "normal";
        private const string HardSettingName = "hard";
        private const string ExpertSettingName = "expert";
        private const string ExpertPlusSettingName = "expertplus";

        public override void SetDefaultValuesToStaging()
        {
            _settingMultipleStagingValues = true;

            EasyStagingValue = false;
            NormalStagingValue = false;
            HardStagingValue = false;
            ExpertStagingValue = false;
            ExpertPlusStagingValue = false;

            _settingMultipleStagingValues = false;
        }

        public override void SetAppliedValuesToStaging()
        {
            _settingMultipleStagingValues = true;

            EasyStagingValue = _easyAppliedValue;
            NormalStagingValue = _normalAppliedValue;
            HardStagingValue = _hardAppliedValue;
            ExpertStagingValue = _expertAppliedValue;
            ExpertPlusStagingValue = _expertPlusAppliedValue;

            _settingMultipleStagingValues = false;
        }

        public override void SetSavedValuesToStaging(IReadOnlyDictionary<string, string> settings)
        {
            int errorCount = 0;

            if (settings.TryGetValue(EasySettingName, out string value) && bool.TryParse(value, out bool boolValue))
                _easyStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{EasySettingName}' value for Difficulty filter ({++errorCount})");

            if (settings.TryGetValue(NormalSettingName, out value) && bool.TryParse(value, out boolValue))
                _normalStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{NormalSettingName}' value for Difficulty filter ({++errorCount})");

            if (settings.TryGetValue(HardSettingName, out value) && bool.TryParse(value, out boolValue))
                _hardStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{HardSettingName}' value for Difficulty filter ({++errorCount})");

            if (settings.TryGetValue(ExpertSettingName, out value) && bool.TryParse(value, out boolValue))
                _expertStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{ExpertSettingName}' value for Difficulty filter ({++errorCount})");

            if (settings.TryGetValue(ExpertPlusSettingName, out value) && bool.TryParse(value, out boolValue))
                _expertPlusStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{ExpertPlusSettingName}' value for Difficulty filter ({++errorCount})");

            if (errorCount > 0)
                Plugin.Log.Warn("Some value(s) could not be loaded by the Difficulty filter");
        }

        public override void ApplyStagingValues()
        {
            _easyAppliedValue = _easyStagingValue;
            _normalAppliedValue = _normalStagingValue;
            _hardAppliedValue = _hardStagingValue;
            _expertAppliedValue = _expertStagingValue;
            _expertPlusAppliedValue = _expertPlusStagingValue;
        }

        public override void ApplyDefaultValues()
        {
            _easyAppliedValue = false;
            _normalAppliedValue = false;
            _hardAppliedValue = false;
            _expertAppliedValue = false;
            _expertPlusAppliedValue = false;
        }

        public override void FilterLevels(ref List<IPreviewBeatmapLevel> levels)
        {
            for (int i = 0; i < levels.Count; ++i)
            {
                var diffs = levels[i].previewDifficultyBeatmapSets.SelectMany(x => x.beatmapDifficulties).ToHashSet();
                if ((_easyAppliedValue && !diffs.Contains(BeatmapDifficulty.Easy)) ||
                    (_normalAppliedValue && !diffs.Contains(BeatmapDifficulty.Normal)) ||
                    (_hardAppliedValue && !diffs.Contains(BeatmapDifficulty.Hard)) ||
                    (_expertAppliedValue && !diffs.Contains(BeatmapDifficulty.Expert)) ||
                    (_expertPlusAppliedValue && !diffs.Contains(BeatmapDifficulty.ExpertPlus)))
                {
                    levels.RemoveAt(i--);
                }
            }
        }

        public override IDictionary<string, string> GetAppliedSettings()
        {
            return new Dictionary<string, string>
            {
                { EasySettingName, _easyAppliedValue.ToString() },
                { NormalSettingName, _normalAppliedValue.ToString() },
                { HardSettingName, _hardAppliedValue.ToString() },
                { ExpertSettingName, _expertAppliedValue.ToString() },
                { ExpertPlusSettingName, _expertPlusAppliedValue.ToString() }
            };
        }
    }
}
