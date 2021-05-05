using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HUIFilters.DataSources.DataTypes;

namespace HUIFilters.DataSources
{
    internal class EmptyDataSource : IBeatmapDataSource, IScoreSaberDataSource, IBeatSaverDataSource
    {
#pragma warning disable CS0067
        public event Action AvailabilityChanged;
#pragma warning restore CS0067

        public bool IsDataAvailable => false;

        private ReadOnlyDictionary<string, BeatmapMetaData> _beatmapData = new ReadOnlyDictionary<string, BeatmapMetaData>(new Dictionary<string, BeatmapMetaData>(0));
        public IReadOnlyDictionary<string, BeatmapMetaData> BeatmapData => _beatmapData;

        private ReadOnlyDictionary<string, ScoreSaberData> _scoreSaberData = new ReadOnlyDictionary<string, ScoreSaberData>(new Dictionary<string, ScoreSaberData>(0));
        public IReadOnlyDictionary<string, ScoreSaberData> ScoreSaberData => _scoreSaberData;

        private ReadOnlyDictionary<string, BeatSaverData> _beatSaverData = new ReadOnlyDictionary<string, BeatSaverData>(new Dictionary<string, BeatSaverData>(0));
        public IReadOnlyDictionary<string, BeatSaverData> BeatSaverData => _beatSaverData;
    }
}
