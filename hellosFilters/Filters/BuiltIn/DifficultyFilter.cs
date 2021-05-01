using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;

namespace HUIFilters.Filters.BuiltIn
{
    public sealed class DifficultyFilter : NotifiableBSMLViewFilterBase
    {
        public override string Name => "Difficulty";
        public override bool IsAvailable => true;

        public override bool IsApplied =>
            EasyAppliedValue ||
            NormalAppliedValue ||
            HardAppliedValue ||
            ExpertAppliedValue ||
            ExpertPlusAppliedValue;
        public override bool HasChanges =>
            EasyAppliedValue != _easyStagingValue ||
            NormalAppliedValue != _normalStagingValue ||
            HardAppliedValue != _hardStagingValue ||
            ExpertAppliedValue != _expertStagingValue ||
            ExpertPlusAppliedValue != _expertPlusStagingValue;

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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        public bool EasyAppliedValue { get; private set; } = false;
        public bool NormalAppliedValue { get; private set; } = false;
        public bool HardAppliedValue { get; private set; } = false;
        public bool ExpertAppliedValue { get; private set; } = false;
        public bool ExpertPlusAppliedValue { get; private set; } = false;

        protected override string AssociatedBSMLFile => "HUIFilters.UI.Views.Filters.DifficultyFilterView.bsml";

        private const string EasySettingName = "easy";
        private const string NormalSettingName = "normal";
        private const string HardSettingName = "hard";
        private const string ExpertSettingName = "expert";
        private const string ExpertPlusSettingName = "expertplus";

        protected override void InternalSetDefaultValuesToStaging()
        {
            EasyStagingValue = false;
            NormalStagingValue = false;
            HardStagingValue = false;
            ExpertStagingValue = false;
            ExpertPlusStagingValue = false;
        }

        protected override void InternalSetAppliedValuesToStaging()
        {
            EasyStagingValue = EasyAppliedValue;
            NormalStagingValue = NormalAppliedValue;
            HardStagingValue = HardAppliedValue;
            ExpertStagingValue = ExpertAppliedValue;
            ExpertPlusStagingValue = ExpertPlusAppliedValue;
        }

        protected override void InternalSetSavedValuesToStaging(IReadOnlyDictionary<string, string> settings)
        {
            int errorCount = 0;

            if (settings.TryGetValue(EasySettingName, out string value) && bool.TryParse(value, out bool boolValue))
                EasyStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{EasySettingName}' value for Difficulty filter ({++errorCount})");

            if (settings.TryGetValue(NormalSettingName, out value) && bool.TryParse(value, out boolValue))
                NormalStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{NormalSettingName}' value for Difficulty filter ({++errorCount})");

            if (settings.TryGetValue(HardSettingName, out value) && bool.TryParse(value, out boolValue))
                HardStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{HardSettingName}' value for Difficulty filter ({++errorCount})");

            if (settings.TryGetValue(ExpertSettingName, out value) && bool.TryParse(value, out boolValue))
                ExpertStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{ExpertSettingName}' value for Difficulty filter ({++errorCount})");

            if (settings.TryGetValue(ExpertPlusSettingName, out value) && bool.TryParse(value, out boolValue))
                ExpertPlusStagingValue = boolValue;
            else
                Plugin.Log.Debug($"Unable to load '{ExpertPlusSettingName}' value for Difficulty filter ({++errorCount})");

            if (errorCount > 0)
                Plugin.Log.Warn("Some value(s) could not be loaded by the Difficulty filter");
        }

        public override void ApplyStagingValues()
        {
            EasyAppliedValue = _easyStagingValue;
            NormalAppliedValue = _normalStagingValue;
            HardAppliedValue = _hardStagingValue;
            ExpertAppliedValue = _expertStagingValue;
            ExpertPlusAppliedValue = _expertPlusStagingValue;
        }

        public override void ApplyDefaultValues()
        {
            EasyAppliedValue = false;
            NormalAppliedValue = false;
            HardAppliedValue = false;
            ExpertAppliedValue = false;
            ExpertPlusAppliedValue = false;
        }

        public override void FilterLevels(ref List<IPreviewBeatmapLevel> levels)
        {
            for (int i = 0; i < levels.Count;)
            {
                var diffs = levels[i].previewDifficultyBeatmapSets.SelectMany(x => x.beatmapDifficulties).ToHashSet();
                if ((EasyAppliedValue && !diffs.Contains(BeatmapDifficulty.Easy)) ||
                    (NormalAppliedValue && !diffs.Contains(BeatmapDifficulty.Normal)) ||
                    (HardAppliedValue && !diffs.Contains(BeatmapDifficulty.Hard)) ||
                    (ExpertAppliedValue && !diffs.Contains(BeatmapDifficulty.Expert)) ||
                    (ExpertPlusAppliedValue && !diffs.Contains(BeatmapDifficulty.ExpertPlus)))
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
                { EasySettingName, EasyAppliedValue.ToString() },
                { NormalSettingName, NormalAppliedValue.ToString() },
                { HardSettingName, HardAppliedValue.ToString() },
                { ExpertSettingName, ExpertAppliedValue.ToString() },
                { ExpertPlusSettingName, ExpertPlusAppliedValue.ToString() }
            };
        }
    }
}
