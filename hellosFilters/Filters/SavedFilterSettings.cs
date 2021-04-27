using System.Collections.Generic;

namespace HUIFilters.Filters
{
    internal class SavedFilterSettings
    {
        public string Name { get; private set; }
        public Dictionary<string, Dictionary<string, string>> FilterSettings { get; private set; }
    }
}
