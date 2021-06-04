using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using HUIFilters.Utilities;

namespace HUIFilters.Filters
{
    public class SavedFilterSettings
    {
        public string Name { get; private set; }
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> FilterSettings { get; private set; }

        public static readonly Regex AllowedCharactersRegex = new Regex("^[A-za-z0-9_.,+\\-*'\" ]+$");

        internal SavedFilterSettings(string name, List<IFilter> filters)
        {
            if (!AllowedCharactersRegex.IsMatch(name))
                throw new ArgumentException("The name of the SavedFilterSettings is invalid", nameof(name));

            Name = name;

            SetFilterSettings(filters);
        }

        internal SavedFilterSettings(string name, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> deserializedSettings)
        {
            if (!AllowedCharactersRegex.IsMatch(name))
                throw new ArgumentException("The name of the SavedFilterSettings is invalid", nameof(name));

            Name = name;

            foreach (var (filterName, filterSettings) in deserializedSettings)
            {
                if (!AllowedCharactersRegex.IsMatch(filterName))
                    throw new ArgumentException("A filter's name from deserialized settings contains an invalid character", nameof(deserializedSettings));

                foreach (var (key, value) in filterSettings)
                {
                    if (!AllowedCharactersRegex.IsMatch(key) || !AllowedCharactersRegex.IsMatch(value))
                        throw new ArgumentException($"The deserialized settings for the filter '{filterName}' contained an invalid character", nameof(deserializedSettings));
                }
            }

            FilterSettings = deserializedSettings;
        }

        internal void SetFilterSettings(List<IFilter> filters)
        {
            var filterSettings = new Dictionary<string, IReadOnlyDictionary<string, string>>(filters.Count);
            foreach (var filter in filters)
            {
                if (filter.IsApplied)
                {
                    var appliedSettings = filter.GetAppliedSettings();
                    foreach (var (key, value) in appliedSettings)
                    {
                        if (!AllowedCharactersRegex.IsMatch(key) || !AllowedCharactersRegex.IsMatch(value))
                            throw new ArgumentException("Some key or value returned by IFilter.GetAppliedSettings contains an invalid character");
                    }

                    filterSettings.Add(filter.GetIdentifier(), new ReadOnlyDictionary<string, string>(appliedSettings));
                }
            }

            FilterSettings = new ReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>(filterSettings);
        }
    }
}
