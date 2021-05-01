using System.Collections.Generic;

namespace HUIFilters.DataSources.DataTypes
{
    public readonly struct ScoreSaberData
    {
        private readonly Dictionary<string, List<DifficultyData>> _characteristics;
        public Dictionary<string, List<DifficultyData>> Characteristics => _characteristics;

        public ScoreSaberData(Dictionary<string, List<DifficultyData>> characteristics)
        {
            _characteristics = characteristics;
        }

        public readonly struct DifficultyData
        {
            private readonly BeatmapDifficulty _difficulty;
            public BeatmapDifficulty Difficulty => _difficulty;
            private readonly double _starRating;
            public double StarRating => _starRating;
            private readonly double _pp;
            public double PP => _pp;

            public DifficultyData(BeatmapDifficulty difficulty, double starRating, double pp)
            {
                _difficulty = difficulty;
                _starRating = starRating;
                _pp = pp;
            }
        }
    }
}
