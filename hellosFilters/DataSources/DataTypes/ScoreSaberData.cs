using System.Collections.Generic;

namespace HUIFilters.DataSources.DataTypes
{
    public readonly struct ScoreSaberData
    {
        private readonly Dictionary<string, Dictionary<BeatmapDifficulty, DifficultyData>> _characteristics;
        public Dictionary<string, Dictionary<BeatmapDifficulty, DifficultyData>> Characteristics => _characteristics;

        public ScoreSaberData(Dictionary<string, Dictionary<BeatmapDifficulty, DifficultyData>> characteristics)
        {
            _characteristics = characteristics;
        }

        public readonly struct DifficultyData
        {
            private readonly double _starRating;
            public double StarRating => _starRating;
            private readonly double _pp;
            public double PP => _pp;

            public DifficultyData(double starRating, double pp)
            {
                _starRating = starRating;
                _pp = pp;
            }
        }
    }
}
