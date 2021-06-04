using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using IPA.Config.Data;
using IPA.Config.Stores;
using HUIFilters.Filters;

namespace HUIFilters.Converters
{
    internal class SavedFilterSettingsConverter : ValueConverter<SavedFilterSettings>
    {
        public const char FilterSeparator = '|';
        public const char SettingsStartIdentifier = '{';
        public const char SettingsEndIdentifier = '}';
        public const char SettingsSeparator = '/';
        public const char KeyValueSeparator = ':';

        private const string NameKey = "Name";
        private const string SettingsKey = "Settings";

        public override Value ToValue(SavedFilterSettings obj, object parent)
        {
            Map map = Value.Map();

            map.Add(NameKey, Value.Text(obj.Name));

            StringBuilder sb = new StringBuilder();
            foreach (var (filterName, settings) in obj.FilterSettings)
            {
                sb.Append(filterName);
                sb.Append(SettingsStartIdentifier);

                foreach (var (key, value) in settings)
                {
                    sb.Append(key);
                    sb.Append(KeyValueSeparator);
                    sb.Append(value);
                    sb.Append(SettingsSeparator);
                }

                sb.Remove(sb.Length - 1, 1);

                sb.Append(SettingsEndIdentifier);
                sb.Append(FilterSeparator);
            }

            sb.Remove(sb.Length - 1, 1);

            map.Add(SettingsKey, Value.Text(sb.ToString()));

            return map;
        }

        public override SavedFilterSettings FromValue(Value value, object parent)
        {
            if (!(value is Map map))
                throw new ArgumentException("Value cannot be parsed into a SavedFilterSettings object", nameof(value));

            string name = null;
            if (map.TryGetValue(NameKey, out Value mapValue) && mapValue is Text nameValue)
                name = nameValue.Value;

            string serializedFilterSettings = null;
            if (map.TryGetValue(SettingsKey, out mapValue) && mapValue is Text settingsValue)
                serializedFilterSettings = settingsValue.Value;

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"The {NameKey} field cannot be empty");
            else if (string.IsNullOrWhiteSpace(serializedFilterSettings))
                throw new ArgumentException($"The {SettingsKey} field cannot be empty");

            var filterSettings = new Dictionary<string, IReadOnlyDictionary<string, string>>();
            foreach (var filterSettingsString in serializedFilterSettings.Split(FilterSeparator))
            {
                int settingsStartIndex = filterSettingsString.IndexOf(SettingsStartIdentifier);
                int settingsEndIndex = filterSettingsString.IndexOf(SettingsEndIdentifier);

                if (settingsStartIndex < 0 || settingsEndIndex < 0 || settingsEndIndex < settingsStartIndex)
                    throw new ArgumentException($"The {SettingsKey} field contains a malformed serialization of settings");

                Dictionary<string, string> settings = new Dictionary<string, string>();
                string filterName = filterSettingsString.Substring(0, settingsStartIndex);
                string settingsString = filterSettingsString.Substring(settingsStartIndex + 1, settingsEndIndex - settingsStartIndex - 1);
                foreach (var settingString in settingsString.Split(SettingsSeparator))
                {
                    int separatorIndex = settingString.IndexOf(KeyValueSeparator);
                    if (separatorIndex < 0)
                        throw new ArgumentException($"The {SettingsKey} field contains a malformed serialization of settings");

                    settings.Add(settingString.Substring(0, separatorIndex), settingString.Substring(separatorIndex + 1));
                }

                filterSettings.Add(filterName, new ReadOnlyDictionary<string, string>(settings));
            }

            if (filterSettings.Count == 0)
                throw new ArgumentException($"The {SettingsKey} field is empty");

            return new SavedFilterSettings(name, new ReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>(filterSettings));
        }
    }
}
