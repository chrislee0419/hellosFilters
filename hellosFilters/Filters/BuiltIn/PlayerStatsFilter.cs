using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zenject;
using SongCore;
using BeatSaberMarkupLanguage.Attributes;
using HUI.Utilities;
using HUIFilters.DataSources;
using HUIFilters.Utilities;

namespace HUIFilters.Filters.BuiltIn
{
    public sealed class PlayerStatsFilter : NotifiableBSMLViewFilterBase
    {
        public override string Name => "Player Stats";
        public override bool IsAvailable => true;

        public override bool IsApplied => 
            CompletedAppliedValue != FilterOptions.Off ||
            FullComboAppliedValue != FilterOptions.Off ||
            LowestRankAppliedValue != LowestRankDefaultValue ||
            HighestRankAppliedValue != HighestRankDefaultValue;
        public override bool HasChanges =>
            CompletedAppliedValue != CompletedStagingValue ||
            FullComboAppliedValue != FullComboStagingValue ||
            LowestRankAppliedValue != LowestRankStagingValue ||
            HighestRankAppliedValue != HighestRankStagingValue;

        private FilterOptions _completedStagingValue = FilterOptions.Off;
        [UIValue("completed-value")]
        public FilterOptions CompletedStagingValue
        {
            get => _completedStagingValue;
            set
            {
                if (_completedStagingValue == value)
                    return;

                _completedStagingValue = value;
                OnPropertyChanged();
            }
        }
        private FilterOptions _fullComboStagingValue = FilterOptions.Off;
        [UIValue("full-combo-value")]
        public FilterOptions FullComboStagingValue
        {
            get => _fullComboStagingValue;
            set
            {
                if (_fullComboStagingValue == value)
                    return;

                _fullComboStagingValue = value;
                OnPropertyChanged();
            }
        }
        private RankModel.Rank _lowestRankStagingValue = LowestRankDefaultValue;
        [UIValue("lowest-rank-value")]
        public RankModel.Rank LowestRankStagingValue
        {
            get => _lowestRankStagingValue;
            set
            {
                if (_lowestRankStagingValue == value)
                    return;

                _lowestRankStagingValue = value;
                OnPropertyChanged();
            }
        }
        private RankModel.Rank _highestRankStagingValue = HighestRankDefaultValue;
        [UIValue("highest-rank-value")]
        public RankModel.Rank HighestRankStagingValue
        {
            get => _highestRankStagingValue;
            set
            {
                if (_highestRankStagingValue == value)
                    return;

                _highestRankStagingValue = value;
                OnPropertyChanged();
            }
        }

        public FilterOptions CompletedAppliedValue { get; private set; } = FilterOptions.Off;
        public FilterOptions FullComboAppliedValue { get; private set; } = FilterOptions.Off;
        public RankModel.Rank LowestRankAppliedValue { get; private set; } = LowestRankDefaultValue;
        public RankModel.Rank HighestRankAppliedValue { get; private set; } = HighestRankDefaultValue;

        protected override string AssociatedBSMLFile => "HUIFilters.UI.Views.Filters.PlayerStatsFilterView.bsml";

        private PlayerDataModel _playerDataModel;
        private LocalLeaderboardsModel _localLeaderboardsModel;
        private IBeatmapDataSource _beatmapDataSource;
        private DifficultyFilter _difficultyFilter;
        private CharacteristicFilter _characteristicFilter;

        private const string CompletedSettingName = "completed";
        private const string FullComboSettingName = "fullCombo";
        private const string LowestRankSettingName = "lowestRank";
        private const string HighestRankSettingName = "highestRank";

        private const RankModel.Rank LowestRankDefaultValue = RankModel.Rank.E;
        private const RankModel.Rank HighestRankDefaultValue = RankModel.Rank.SSS;

        [UIValue("filter-options")]
        private static readonly List<object> FilterOptionsOptions = FilterUtilities.EnumerateEnumValuesAsObjectList<FilterOptions>();
        [UIValue("lowest-rank-options")]
        private static readonly List<object> LowestRankOptions = FilterUtilities.EnumerateEnumValuesAsObjectList<RankModel.Rank>();
        [UIValue("highest-rank-options")]
        private static readonly List<object> HighestRankOptions = FilterUtilities.EnumerateEnumValues<RankModel.Rank>().Reverse().Select(x => (object)x).ToList();

        [Inject]
        public PlayerStatsFilter(
            PlayerDataModel playerDataModel,
            LocalLeaderboardViewController localLeaderboardViewController,
            IBeatmapDataSource beatmapDataSource,
            DifficultyFilter difficultyFilter,
            CharacteristicFilter characteristicFilter)
        {
            _playerDataModel = playerDataModel;
            _localLeaderboardsModel = localLeaderboardViewController.leaderboardsModel;
            _beatmapDataSource = beatmapDataSource;
            _difficultyFilter = difficultyFilter;
            _characteristicFilter = characteristicFilter;
        }

        protected override void InternalSetDefaultValuesToStaging()
        {
            CompletedStagingValue = FilterOptions.Off;
            FullComboStagingValue = FilterOptions.Off;
            LowestRankStagingValue = LowestRankDefaultValue;
            HighestRankStagingValue = HighestRankDefaultValue;
        }

        protected override void InternalSetAppliedValuesToStaging()
        {
            CompletedStagingValue = CompletedAppliedValue;
            FullComboStagingValue = FullComboAppliedValue;
            LowestRankStagingValue = LowestRankAppliedValue;
            HighestRankStagingValue = HighestRankAppliedValue;
        }

        protected override void InternalSetSavedValuesToStaging(IReadOnlyDictionary<string, string> settings)
        {
            int errorCount = 0;

            if (settings.TryGetValue(CompletedSettingName, out string value) && Enum.TryParse(value, out FilterOptions enumValue))
                CompletedStagingValue = enumValue;
            else
                Plugin.Log.Debug($"Unable to load '{CompletedSettingName}' value for Player Stats filter ({++errorCount})");

            if (settings.TryGetValue(FullComboSettingName, out value) && Enum.TryParse(value, out enumValue))
                FullComboStagingValue = enumValue;
            else
                Plugin.Log.Debug($"Unable to load '{FullComboSettingName}' value for Player Stats filter ({++errorCount})");

            if (settings.TryGetValue(LowestRankSettingName, out value) && Enum.TryParse(value, out RankModel.Rank rankValue))
                LowestRankStagingValue = rankValue;
            else
                Plugin.Log.Debug($"Unable to load '{LowestRankSettingName}' value for Player Stats filter ({++errorCount})");

            if (settings.TryGetValue(HighestRankSettingName, out value) && Enum.TryParse(value, out  rankValue))
                HighestRankStagingValue = rankValue;
            else
                Plugin.Log.Debug($"Unable to load '{HighestRankSettingName}' value for Player Stats filter ({++errorCount})");

            if (errorCount > 0)
                Plugin.Log.Warn("Some value(s) could not be loaded by the Player Stats filter");
        }

        public override void ApplyStagingValues()
        {
            CompletedAppliedValue = _completedStagingValue;
            FullComboAppliedValue = _fullComboStagingValue;
            LowestRankAppliedValue = _lowestRankStagingValue;
            HighestRankAppliedValue = _highestRankStagingValue;
        }

        public override void ApplyDefaultValues()
        {
            CompletedAppliedValue = FilterOptions.Off;
            FullComboAppliedValue = FilterOptions.Off;
            LowestRankAppliedValue = LowestRankDefaultValue;
            HighestRankAppliedValue = HighestRankDefaultValue;
        }

        public override void FilterLevels(ref List<IPreviewBeatmapLevel> levels)
        {
            HashSet<string> characteristicsToCheck = _characteristicFilter.GetSerializedNamesOfAppliedCharacteristics();
            HashSet<BeatmapDifficulty> difficultiesToCheck = _difficultyFilter.GetAppliedDifficulties();

            var levelsToRemove = levels.AsParallel().Where(delegate (IPreviewBeatmapLevel level)
            {
                List<BeatmapPlayerStats> stats = GetBeatmapPlayerStatsForLevel(level, characteristicsToCheck, difficultiesToCheck);

                if (CompletedAppliedValue != FilterOptions.Off)
                {
                    bool levelCompleted = stats.Count > 0;
                    if ((CompletedAppliedValue == FilterOptions.OptionUnfulfilled && levelCompleted) ||
                        (CompletedAppliedValue == FilterOptions.OptionFulfilled && !levelCompleted))
                    {
                        return true;
                    }
                }

                if (FullComboAppliedValue != FilterOptions.Off)
                {
                    bool hasFullCombo = stats.Any(x => x.fullCombo);
                    if ((FullComboAppliedValue == FilterOptions.OptionUnfulfilled && hasFullCombo) ||
                        (FullComboAppliedValue == FilterOptions.OptionFulfilled && !hasFullCombo))
                    {
                        return true;
                    }
                }

                if (LowestRankAppliedValue != LowestRankDefaultValue ||
                    HighestRankAppliedValue != HighestRankDefaultValue)
                {
                    bool hasRankData = stats.Any(x => x.maxRank.HasValue);

                    if (!hasRankData && LowestRankAppliedValue != LowestRankDefaultValue)
                    {
                        return true;
                    }
                    else if (hasRankData)
                    {
                        if (LowestRankAppliedValue != LowestRankDefaultValue &&
                            stats.All(x => (x.maxRank.HasValue ? x.maxRank.Value : LowestRankDefaultValue) < LowestRankAppliedValue))
                        {
                            return true;
                        }
                        else if (HighestRankAppliedValue != HighestRankDefaultValue &&
                            stats.All(x => (x.maxRank.HasValue ? x.maxRank.Value : HighestRankDefaultValue) > HighestRankAppliedValue))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }).ToHashSet();

            for (int i = 0; i < levels.Count;)
            {
                if (levelsToRemove.Contains(levels[i]))
                    levels.RemoveAt(i);
                else
                    ++i;
            }
        }

        public override IDictionary<string, string> GetAppliedSettings()
        {
            return new Dictionary<string, string>
            {
                { CompletedSettingName, CompletedAppliedValue.ToString() },
                { FullComboSettingName, FullComboAppliedValue.ToString() },
                { LowestRankSettingName, LowestRankAppliedValue.ToString() },
                { HighestRankSettingName, HighestRankAppliedValue.ToString() }
            };
        }

        private List<BeatmapPlayerStats> GetBeatmapPlayerStatsForLevel(IPreviewBeatmapLevel level, HashSet<string> characteristicsToCheck, HashSet<BeatmapDifficulty> difficultiesToCheck)
        {
            List<BeatmapPlayerStats> stats = new List<BeatmapPlayerStats>();

            foreach (var characteristic in level.previewDifficultyBeatmapSets)
            {
                string serializedName = characteristic.beatmapCharacteristic.serializedName;
                if (!characteristicsToCheck.Contains(serializedName))
                    continue;

                foreach (var difficulty in characteristic.beatmapDifficulties)
                {
                    if (!difficultiesToCheck.Contains(difficulty))
                        continue;

                    if (GetBeatmapPlayerStatsForDifficulty(level, serializedName, difficulty, out var beatmapPlayerStats))
                        stats.Add(beatmapPlayerStats);
                }
            }

            return stats;
        }

        private bool GetBeatmapPlayerStatsForDifficulty(IPreviewBeatmapLevel level, string characteristicSerializedName, BeatmapDifficulty difficulty, out BeatmapPlayerStats beatmapPlayerStats)
        {
            string levelID = BeatmapUtilities.GetSimplifiedLevelID(level);
            bool fullCombo = false;
            int score = -1;
            RankModel.Rank? maxRank = null;

            // check solo mode stats
            var validSoloStats = _playerDataModel.playerData.levelsStatsData.Where(x =>
                x.levelID.StartsWith(levelID) &&
                x.beatmapCharacteristic.serializedName == characteristicSerializedName &&
                x.difficulty == difficulty &&
                x.validScore);

            foreach (var entry in validSoloStats)
            {
                fullCombo |= entry.fullCombo;

                if (entry.highScore > score)
                    score = entry.highScore;

                if (!maxRank.HasValue || entry.maxRank > maxRank.Value)
                    maxRank = entry.maxRank;
            }

            // check local party mode stats
            StringBuilder sb = new StringBuilder();
            foreach (var duplicateLevelID in GetDuplicateLevelIDsForLevelID(levelID))
            {
                sb.Clear();
                sb.Append(duplicateLevelID);

                if (characteristicSerializedName != CharacteristicFilter.StandardSerializedName)
                    sb.Append(characteristicSerializedName);

                sb.Append(difficulty.ToString());

                var validPartyStats = _localLeaderboardsModel.GetScores(sb.ToString(), LocalLeaderboardsModel.LeaderboardType.AllTime);
                if (validPartyStats == null)
                    continue;

                foreach (var entry in validPartyStats)
                {
                    fullCombo |= entry._fullCombo;

                    if (entry._score > score)
                        score = entry._score;

                    if (GetRankFromScore(level, entry._score, characteristicSerializedName, difficulty, out var calculatedRank))
                    {
                        if (!maxRank.HasValue || calculatedRank > maxRank.Value)
                            maxRank = calculatedRank;
                    }
                }
            }

            if (score >= 0)
            {
                beatmapPlayerStats = new BeatmapPlayerStats(level, characteristicSerializedName, difficulty, fullCombo, score, maxRank);
                return true;
            }
            else
            {
                beatmapPlayerStats = default;
                return false;
            }
        }

        /// <summary>
        /// Since a user can have duplicates of the same map, this function gets all the level IDs from all duplicates, 
        /// given the simplified version of the level ID.
        /// 
        /// <para>
        /// The simplified level ID of a level can be obtained through <see cref="BeatmapUtilities.GetSimplifiedLevelID(IPreviewBeatmapLevel)"/>.
        /// </para>
        /// </summary>
        /// <param name="levelID">A simplified level ID.</param>
        /// <returns>A <see cref="List{string}"/> containing all levels that match the given level ID.</returns>
        private List<string> GetDuplicateLevelIDsForLevelID(string levelID)
        {
            List<string> duplicateLevelIDs = new List<string>();

            if (!levelID.StartsWith(CustomLevelLoader.kCustomLevelPrefixId))
            {
                duplicateLevelIDs.Add(levelID);
                return duplicateLevelIDs;
            }

            foreach (var level in Loader.CustomLevelsCollection.beatmapLevels.Where(x => x.levelID.StartsWith(levelID)))
                duplicateLevelIDs.Add(level.levelID);

            return duplicateLevelIDs;
        }

        private bool GetRankFromScore(IPreviewBeatmapLevel level, int score, string characteristicSerializedName, BeatmapDifficulty difficulty, out RankModel.Rank calculatedRank)
        {
            // NOTE: calculations here to get the max rank do not take modifiers into account, since that information isn't stored
            // it is assumed that none of the scores set have used modifiers
            if (level is CustomPreviewBeatmapLevel customLevel &&
                _beatmapDataSource.IsDataAvailable &&
                _beatmapDataSource.BeatmapData.TryGetValue(BeatmapUtilities.GetCustomLevelHash(customLevel), out var metadata) &&
                metadata.Characteristics.TryGetValue(characteristicSerializedName, out var difficultiesMetadata) &&
                difficultiesMetadata.TryGetValue(difficulty, out var difficultyMetadata))
            {
                // custom levels on local leaderboards need an IBeatmapDataSource to get the rank
                // since the rank is calculated via note count
                int maxRawScore = ScoreModel.MaxRawScoreForNumberOfNotes(difficultyMetadata.NoteCount);
                calculatedRank = RankModel.GetRankForScore(score, score, maxRawScore, maxRawScore);

                return true;
            }
            else if (level is IBeatmapLevel ostLevel)
            {
                var characteristicData = ostLevel.beatmapLevelData.difficultyBeatmapSets.FirstOrDefault(x => x.beatmapCharacteristic.serializedName == characteristicSerializedName);
                if (characteristicData != null)
                {
                    var difficultyData = characteristicData.difficultyBeatmaps.FirstOrDefault(x => x.difficulty == difficulty);
                    if (difficultyData != null)
                    {
                        int maxRawScore = ScoreModel.MaxRawScoreForNumberOfNotes(difficultyData.beatmapData.cuttableNotesType);
                        calculatedRank = RankModel.GetRankForScore(score, score, maxRawScore, maxRawScore);

                        return true;
                    }
                }
            }

            calculatedRank = default;
            return false;
        }

        [UIAction("completed-formatter")]
        private string CompletedFormatter(object obj)
        {
            var option = (FilterOptions)obj;

            switch (option)
            {
                case FilterOptions.Off:
                    return "Off";

                case FilterOptions.OptionFulfilled:
                    return "Keep Completed";

                case FilterOptions.OptionUnfulfilled:
                    return "Keep Never Completed";

                default:
                    return "Error";
            }
        }

        [UIAction("full-combo-formatter")]
        private string FullComboFormatter(object obj)
        {
            var option = (FilterOptions)obj;

            switch (option)
            {
                case FilterOptions.Off:
                    return "Off";

                case FilterOptions.OptionFulfilled:
                    return "Keep Levels With FC";

                case FilterOptions.OptionUnfulfilled:
                    return "Keep Levels Without FC";

                default:
                    return "Error";
            }
        }

        [UIAction("lowest-rank-formatter")]
        private string LowestRankFormatter(object obj)
        {
            var rank = (RankModel.Rank)obj;

            if (rank == LowestRankDefaultValue)
                return "Off";
            else
                return rank.ToString();
        }

        [UIAction("highest-rank-formatter")]
        private string HighestRankFormatter(object obj)
        {
            var rank = (RankModel.Rank)obj;

            if (rank == HighestRankDefaultValue)
                return "Off";
            else
                return rank.ToString();
        }

        public enum FilterOptions
        {
            Off,
            OptionFulfilled,
            OptionUnfulfilled
        }

        private readonly struct BeatmapPlayerStats
        {
            public readonly IPreviewBeatmapLevel level;
            public readonly string characteristicSerializedName;
            public readonly BeatmapDifficulty difficulty;

            public readonly bool fullCombo;
            public readonly int score;
            public readonly RankModel.Rank? maxRank;

            public BeatmapPlayerStats(IPreviewBeatmapLevel level,
                string characteristicSerializedName,
                BeatmapDifficulty difficulty,
                bool fullCombo,
                int score,
                RankModel.Rank? maxRank)
            {
                this.level = level;
                this.characteristicSerializedName = characteristicSerializedName;
                this.difficulty = difficulty;
                this.fullCombo = fullCombo;
                this.score = score;
                this.maxRank = maxRank;
            }
        }
    }
}
