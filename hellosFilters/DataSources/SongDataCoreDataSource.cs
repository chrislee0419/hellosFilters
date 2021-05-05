using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Zenject;
using IPA.Utilities.Async;
using SongDataCore.BeatStar;
using HUI.Utilities;
using HUIFilters.DataSources.DataTypes;
using SDCPlugin = SongDataCore.Plugin;

namespace HUIFilters.DataSources
{
    internal class SongDataCoreDataSource : IInitializable, IDisposable, IBeatmapDataSource, IScoreSaberDataSource, IBeatSaverDataSource
    {
        public event Action AvailabilityChanged;

        public bool IsDataAvailable => !_isProcessingData && (_beatmapData.Count > 0 || _scoreSaberData.Count > 0 || _beatSaverData.Count > 0);

        private Dictionary<string, BeatmapMetaData> _beatmapData = new Dictionary<string, BeatmapMetaData>(StringComparer.InvariantCultureIgnoreCase);
        private ReadOnlyDictionary<string, BeatmapMetaData> _readOnlyBeatmapData = null;
        public IReadOnlyDictionary<string, BeatmapMetaData> BeatmapData
        {
            get
            {
                if (_readOnlyBeatmapData == null)
                    _readOnlyBeatmapData = new ReadOnlyDictionary<string, BeatmapMetaData>(_beatmapData);
                return _readOnlyBeatmapData;
            }
        }

        private Dictionary<string, ScoreSaberData> _scoreSaberData = new Dictionary<string, ScoreSaberData>(StringComparer.InvariantCultureIgnoreCase);
        private ReadOnlyDictionary<string, ScoreSaberData> _readOnlyScoreSaberData = null;
        public IReadOnlyDictionary<string, ScoreSaberData> ScoreSaberData
        {
            get
            {
                if (_readOnlyScoreSaberData == null)
                    _readOnlyScoreSaberData = new ReadOnlyDictionary<string, ScoreSaberData>(_scoreSaberData);
                return _readOnlyScoreSaberData;
            }
        }

        private Dictionary<string, BeatSaverData> _beatSaverData = new Dictionary<string, BeatSaverData>(StringComparer.InvariantCultureIgnoreCase);
        private ReadOnlyDictionary<string, BeatSaverData> _readOnlyBeatSaverData = null;
        public IReadOnlyDictionary<string, BeatSaverData> BeatSaverData
        {
            get
            {
                if (_readOnlyBeatSaverData == null)
                    _readOnlyBeatSaverData = new ReadOnlyDictionary<string, BeatSaverData>(_beatSaverData);
                return _readOnlyBeatSaverData;
            }
        }

        private bool _isProcessingData = false;

        public void Initialize()
        {
            const int WaitTime = 1000;

            UnityMainThreadTaskScheduler.Factory.StartNew(async delegate ()
            {
                Plugin.Log.DebugOnly("Waiting for SongDataCore to initialize");

                int tries = 10;
                while (SDCPlugin.Songs == null && (tries--) > 0)
                    await Task.Delay(WaitTime);

                if (SDCPlugin.Songs == null)
                {
                    Plugin.Log.Error("Unable to detect SongDataCore initialization, SongDataCoreDataSource will not be able to provide data");
                    return;
                }

                Plugin.Log.DebugOnly("Installing listeners to SongDataCore");
                SDCPlugin.Songs.OnStartDownloading += OnSongDataCoreStartDownloading;
                SDCPlugin.Songs.OnDataFinishedProcessing += OnSongDataCoreFinishedProcessing;

                if (SDCPlugin.Songs.IsDataAvailable())
                    await StartProcessingDataAsync();
            });
        }

        public void Dispose()
        {
            if (SDCPlugin.Songs != null)
            {
                SDCPlugin.Songs.OnStartDownloading -= OnSongDataCoreStartDownloading;
                SDCPlugin.Songs.OnDataFinishedProcessing -= OnSongDataCoreFinishedProcessing;
            }
        }

        private void OnSongDataCoreStartDownloading()
        {
            _beatmapData.Clear();
            _scoreSaberData.Clear();
            _beatSaverData.Clear();

            this.CallAndHandleAction(AvailabilityChanged, nameof(AvailabilityChanged));
        }

        private void OnSongDataCoreFinishedProcessing()
        {
            // SongDataCore seems to have some issue where the data field isn't populated
            // when the OnDataFinishedProcessing event is fired, even though it can be
            // populated a bit later
            // i'll probably report it later, for now, just add a delay in the check
            UnityMainThreadTaskScheduler.Factory.StartNew(async delegate ()
            {
                int tries = 20;
                while (!SDCPlugin.Songs.IsDataAvailable() && (tries--) > 0)
                    await Task.Delay(1000);

                if (SDCPlugin.Songs.IsDataAvailable())
                    await StartProcessingDataAsync();
                else
                    Plugin.Log.Error("Unable to process data from SongDataCore (data is not available)");
            });
        }

        private async Task StartProcessingDataAsync()
        {
            Plugin.Log.Info("Processing data from SongDataCore");
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                _isProcessingData = true;

                ThreadPool.QueueUserWorkItem(ProcessDataThread);

                const int PollTimeMilliseconds = 100;
                while (_isProcessingData)
                    await Task.Delay(PollTimeMilliseconds);
            }
            catch (NotSupportedException)
            {
                Plugin.Log.Warn("Unable to process SongDataCore data using ThreadPool, falling back to async loading");

                await ProcessDataAsync();
            }

            sw.Stop();
            Plugin.Log.Info($"Finished processing data from SongDataCore in {sw.ElapsedMilliseconds}ms");

            this.CallAndHandleAction(AvailabilityChanged, nameof(AvailabilityChanged));
        }

        private void ProcessDataThread(object state)
        {
            foreach (var (hash, song) in SDCPlugin.Songs.Data.Songs)
            {
                var beatmapCharacteristics = new Dictionary<string, Dictionary<BeatmapDifficulty, BeatmapMetaData.DifficultyData>>();
                var scoreSabercharacteristics = new Dictionary<string, Dictionary<BeatmapDifficulty, ScoreSaberData.DifficultyData>>();

                foreach (var (characteristic, difficultiesData) in song.characteristics)
                {
                    var beatmapDifficulties = new Dictionary<BeatmapDifficulty, BeatmapMetaData.DifficultyData>();
                    var scoreSaberDifficulties = new Dictionary<BeatmapDifficulty, ScoreSaberData.DifficultyData>();

                    foreach (var difficulty in difficultiesData.Values)
                    {
                        BeatmapDifficulty diffEnum = StringToBeatmapDifficulty(difficulty.diff);

                        beatmapDifficulties.Add(
                            diffEnum,
                            new BeatmapMetaData.DifficultyData(difficulty.njs, difficulty.nts, difficulty.bmb, difficulty.obs));

                        scoreSaberDifficulties.Add(
                            diffEnum,
                            new ScoreSaberData.DifficultyData(difficulty.star, difficulty.pp));
                    }

                    string characteristicSerializedName = BeatStarCharacteristicsToSerializedName(characteristic);
                    beatmapCharacteristics.Add(characteristicSerializedName, beatmapDifficulties);
                    scoreSabercharacteristics.Add(characteristicSerializedName, scoreSaberDifficulties);
                }

                _beatmapData.Add(hash, new BeatmapMetaData(beatmapCharacteristics, song.bpm));
                _scoreSaberData.Add(hash, new ScoreSaberData(scoreSabercharacteristics));
                _beatSaverData.Add(hash, new BeatSaverData(song.downloadCount, song.upVotes, song.downVotes, song.heat, song.rating));
            }

            _isProcessingData = false;
        }

        private async Task ProcessDataAsync()
        {
            const int WorkTimeLimitMilliseconds = 5;
            Stopwatch workLimitStopwatch = Stopwatch.StartNew();

            foreach (var (hash, song) in SDCPlugin.Songs.Data.Songs)
            {
                var beatmapCharacteristics = new Dictionary<string, Dictionary<BeatmapDifficulty, BeatmapMetaData.DifficultyData>>();
                var scoreSabercharacteristics = new Dictionary<string, Dictionary<BeatmapDifficulty, ScoreSaberData.DifficultyData>>();

                if (workLimitStopwatch.ElapsedMilliseconds > WorkTimeLimitMilliseconds)
                {
                    await Task.Yield();
                    workLimitStopwatch.Restart();
                }

                foreach (var (characteristic, difficultiesData) in song.characteristics)
                {
                    var beatmapDifficulties = new Dictionary<BeatmapDifficulty, BeatmapMetaData.DifficultyData>();
                    var scoreSaberDifficulties = new Dictionary<BeatmapDifficulty, ScoreSaberData.DifficultyData>();

                    foreach (var difficulty in difficultiesData.Values)
                    {
                        BeatmapDifficulty diffEnum = StringToBeatmapDifficulty(difficulty.diff);

                        beatmapDifficulties.Add(
                            diffEnum,
                            new BeatmapMetaData.DifficultyData(difficulty.njs, difficulty.nts, difficulty.bmb, difficulty.obs));

                        scoreSaberDifficulties.Add(
                            diffEnum,
                            new ScoreSaberData.DifficultyData(difficulty.star, difficulty.pp));
                    }

                    string characteristicSerializedName = BeatStarCharacteristicsToSerializedName(characteristic);
                    beatmapCharacteristics.Add(characteristicSerializedName, beatmapDifficulties);
                    scoreSabercharacteristics.Add(characteristicSerializedName, scoreSaberDifficulties);
                }

                _beatmapData.Add(hash, new BeatmapMetaData(beatmapCharacteristics, song.bpm));
                _scoreSaberData.Add(hash, new ScoreSaberData(scoreSabercharacteristics));
                _beatSaverData.Add(hash, new BeatSaverData(song.downloadCount, song.upVotes, song.downVotes, song.heat, song.rating));
            }

            workLimitStopwatch.Stop();

            _isProcessingData = false;
        }

        private static BeatmapDifficulty StringToBeatmapDifficulty(string str)
        {
            if (str == "Expert+")
                return BeatmapDifficulty.ExpertPlus;
            else if (Enum.TryParse(str, out BeatmapDifficulty diff))
                return diff;

            Plugin.Log.DebugOnly($"Unable to parse difficulty string from SongDataCore data to BeatmapDifficulty ({str})");
            return BeatmapDifficulty.Easy;
        }

        private static string BeatStarCharacteristicsToSerializedName(BeatStarCharacteristics characteristic)
        {
            switch (characteristic)
            {
                case BeatStarCharacteristics.Standard:
                    return "Standard";

                case BeatStarCharacteristics.OneSaber:
                    return "OneSaber";

                case BeatStarCharacteristics.NoArrows:
                    return "NoArrows";

                case BeatStarCharacteristics.Degree90:
                    return "90Degree";

                case BeatStarCharacteristics.Degree360:
                    return "360Degree";

                case BeatStarCharacteristics.Lightshow:
                    return "Lightshow";

                case BeatStarCharacteristics.Lawless:
                    return "Lawless";

                default:
                    return "Unknown";
            }
        }
    }
}
