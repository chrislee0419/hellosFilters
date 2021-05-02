using System.Collections.Generic;

namespace HUIFilters.DataSources.DataTypes
{
    public readonly struct BeatmapMetaData
    {
        private readonly Dictionary<string, Dictionary<BeatmapDifficulty, DifficultyData>> _characteristics;
        public Dictionary<string, Dictionary<BeatmapDifficulty, DifficultyData>> Characteristics => _characteristics;

        private readonly double _bpm;
        public double BPM => _bpm;

        public BeatmapMetaData(Dictionary<string, Dictionary<BeatmapDifficulty, DifficultyData>> characteristics, double bpm)
        {
            _characteristics = characteristics;
            _bpm = bpm;
        }

        public readonly struct DifficultyData
        {
            private readonly int _njs;
            public int NJS => _njs;

            private readonly int _notes;
            public int NoteCount => _notes;
            private readonly int _bombs;
            public int BombCount => _bombs;
            private readonly int _obstacles;
            public int ObstacleCount => _obstacles;

            public DifficultyData(int njs, int noteCount, int bombCount, int obstacleCount)
            {
                _njs = njs;
                _notes = noteCount;
                _bombs = bombCount;
                _obstacles = obstacleCount;
            }
        }
    }
}
