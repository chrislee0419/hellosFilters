using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
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
                bool completed;
                bool fullCombo;
                RankModel.Rank? lowestRankAchieved;
                RankModel.Rank? highestRankAchieved;
                (completed, fullCombo, lowestRankAchieved, highestRankAchieved) = GetBeatmapPlayerStatsForLevel(level, characteristicsToCheck, difficultiesToCheck);

                if (CompletedAppliedValue != FilterOptions.Off &&
                    ((CompletedAppliedValue == FilterOptions.OptionUnfulfilled && completed) ||
                    (CompletedAppliedValue == FilterOptions.OptionFulfilled && !completed)))
                {
                    return true;
                }

                if (FullComboAppliedValue != FilterOptions.Off &&
                    ((FullComboAppliedValue == FilterOptions.OptionUnfulfilled && fullCombo) ||
                    (FullComboAppliedValue == FilterOptions.OptionFulfilled && !fullCombo)))
                {
                    return true;
                }

                if (LowestRankAppliedValue != LowestRankDefaultValue || HighestRankAppliedValue != HighestRankDefaultValue)
                {
                    if (!lowestRankAchieved.HasValue)
                    {
                        if (LowestRankAppliedValue != LowestRankDefaultValue)
                            return true;
                    }
                    else if ((LowestRankAppliedValue != LowestRankDefaultValue && highestRankAchieved < LowestRankAppliedValue) ||
                            (HighestRankAppliedValue != HighestRankDefaultValue && lowestRankAchieved > HighestRankAppliedValue))
                    {
                        // we want to remove levels where ranks from ALL difficulties do not pass the filter
                        // ex: if a level has S rank on normal and B rank on hard, the level will not be removed if
                        //     LowestRankAppliedValue is set to A because the normal difficulty passes the filter
                        return true;
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

        private (bool, bool, RankModel.Rank?, RankModel.Rank?) GetBeatmapPlayerStatsForLevel(
            IPreviewBeatmapLevel level,
            HashSet<string> characteristicsToCheck,
            HashSet<BeatmapDifficulty> difficultiesToCheck)
        {
            bool completed = false;
            bool fullCombo = false;
            RankModel.Rank? lowestRankAchieved = null;
            RankModel.Rank? highestRankAchieved = null;

            string simplifiedLevelID = BeatmapUtilities.GetSimplifiedLevelID(level);
            var soloStats = _playerDataModel.playerData.levelsStatsData.Where(x =>
                x.levelID.StartsWith(simplifiedLevelID) &&
                x.validScore).ToList();
            var localLeaderboards = _localLeaderboardsModel
                .GetLeaderboardsData(LocalLeaderboardsModel.LeaderboardType.AllTime)
                .Where(x => x._leaderboardId.StartsWith(simplifiedLevelID))
                .ToList();
            foreach (var characteristic in level.previewDifficultyBeatmapSets)
            {
                string serializedName = characteristic.beatmapCharacteristic.serializedName;
                if (!characteristicsToCheck.Contains(serializedName))
                    continue;

                foreach (var difficulty in characteristic.beatmapDifficulties)
                {
                    if (!difficultiesToCheck.Contains(difficulty))
                        continue;

                    string leaderboardIDSuffix = $"{(serializedName == CharacteristicFilter.StandardSerializedName ? "" : serializedName)}{difficulty}";
                    var levelSpecificSoloStats = soloStats.Where(x => x.beatmapCharacteristic.serializedName == serializedName && x.difficulty == difficulty);
                    var levelSpecificLocalLoaderboards = localLeaderboards.Where(x => x._leaderboardId.EndsWith(leaderboardIDSuffix));
                    int maxRawScore = GetMaxRawScore(level, serializedName, difficulty);
                    if (GetBeatmapPlayerStatsForLevel(levelSpecificSoloStats, levelSpecificLocalLoaderboards, maxRawScore, out var hasFullCombo, out var bestRank))
                    {
                        completed = true;
                        fullCombo |= hasFullCombo;

                        if (bestRank.HasValue)
                        {
                            if (!lowestRankAchieved.HasValue || bestRank.Value < lowestRankAchieved.Value)
                                lowestRankAchieved = bestRank;
                            if (!highestRankAchieved.HasValue || bestRank.Value > highestRankAchieved.Value)
                                highestRankAchieved = bestRank;
                        }
                    }
                }
            }

            return (completed, fullCombo, lowestRankAchieved, highestRankAchieved);
        }

        private bool GetBeatmapPlayerStatsForLevel(
            IEnumerable<PlayerLevelStatsData> levelSpecificsoloStats,
            IEnumerable<LocalLeaderboardsModel.LeaderboardData> levelSpecificlocalLeaderboards,
            int maxRawScore,
            out bool fullCombo,
            out RankModel.Rank? bestRank)
        {
            int score = -1;
            fullCombo = false;
            bestRank = null;

            // check solo mode stats
            foreach (var entry in levelSpecificsoloStats)
            {
                fullCombo |= entry.fullCombo;

                if (entry.highScore > score)
                    score = entry.highScore;

                if (!bestRank.HasValue || entry.maxRank > bestRank.Value)
                    bestRank = entry.maxRank;
            }

            // check local party mode stats
            foreach (var leaderboard in levelSpecificlocalLeaderboards)
            {
                foreach (var entry in leaderboard._scores)
                {
                    fullCombo |= entry._fullCombo;

                    if (entry._score > score)
                    {
                        score = entry._score;

                        if (maxRawScore >= 0)
                        {
                            var calculatedRank = RankModel.GetRankForScore(score, score, maxRawScore, maxRawScore);
                            if (!bestRank.HasValue || calculatedRank > bestRank.Value)
                                bestRank = calculatedRank;
                        }
                    }
                }
            }

            return score >= 0;
        }

        private int GetMaxRawScore(IPreviewBeatmapLevel level, string characteristicSerializedName, BeatmapDifficulty difficulty)
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
                return ScoreModel.MaxRawScoreForNumberOfNotes(difficultyMetadata.NoteCount);
            }
            else if (level is IBeatmapLevel ostLevel)
            {
                var characteristicData = ostLevel.beatmapLevelData.difficultyBeatmapSets.FirstOrDefault(x => x.beatmapCharacteristic.serializedName == characteristicSerializedName);
                if (characteristicData != null)
                {
                    var difficultyData = characteristicData.difficultyBeatmaps.FirstOrDefault(x => x.difficulty == difficulty);
                    if (difficultyData != null)
                        return ScoreModel.MaxRawScoreForNumberOfNotes(difficultyData.beatmapData.cuttableNotesType);
                }
            }

            return -1;
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
    }
}
