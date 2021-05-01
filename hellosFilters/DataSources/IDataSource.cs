using System.Collections.Generic;
using HUIFilters.DataSources.DataTypes;

namespace HUIFilters.DataSources
{
    public interface IDataSource
    {
        IReadOnlyDictionary<string, BeatmapMetaData> BeatmapData { get; }
    }

    public interface IScoreSaberDataSource
    {
        IReadOnlyDictionary<string, ScoreSaberData> ScoreSaberData { get; }
    }

    public interface IBeatSaverDataSource
    {
        IReadOnlyDictionary<string, BeatSaverData> BeatSaverData { get; }
    }
}
