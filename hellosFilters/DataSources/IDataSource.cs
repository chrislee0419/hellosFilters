using System.Collections.Generic;
using HUIFilters.DataSources.DataTypes;

namespace HUIFilters.DataSources
{
    public interface IDataSource
    {
        bool IsDataAvailable { get; }
    }

    public interface IBeatmapDataSource : IDataSource
    {
        IReadOnlyDictionary<string, BeatmapMetaData> BeatmapData { get; }
    }

    public interface IScoreSaberDataSource : IDataSource
    {
        IReadOnlyDictionary<string, ScoreSaberData> ScoreSaberData { get; }
    }

    public interface IBeatSaverDataSource : IDataSource
    {
        IReadOnlyDictionary<string, BeatSaverData> BeatSaverData { get; }
    }
}
