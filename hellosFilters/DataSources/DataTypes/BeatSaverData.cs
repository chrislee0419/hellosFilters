
namespace HUIFilters.DataSources.DataTypes
{
    public readonly struct BeatSaverData
    {
        private readonly int _downloads;
        public int DownloadCount => _downloads;
        private readonly int _upvotes;
        public int UpVotes => _upvotes;
        private readonly int _downvotes;
        public int DownVotes => _downvotes;
        private readonly double _heat;
        public double Heat => _heat;
        private readonly double _rating;
        public double Rating => _rating;

        public BeatSaverData(int downloadCount, int upvotes, int downvotes, double heat, double rating)
        {
            _downloads = downloadCount;
            _upvotes = upvotes;
            _downvotes = downvotes;
            _heat = heat;
            _rating = rating;
        }
    }
}
