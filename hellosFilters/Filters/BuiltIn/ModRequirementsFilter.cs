using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zenject;
using IPA.Utilities.Async;
using SongCore;
using BeatSaberMarkupLanguage.Attributes;
using HUI.Utilities;
using HUIFilters.Utilities;
using UnityEngine;

namespace HUIFilters.Filters.BuiltIn
{
    public sealed class ModRequirementsFilter : NotifiableBSMLViewFilterBase, IInitializable, IDisposable
    {
        public override string Name => "Mod Requirements";
        public override bool IsAvailable => true;

        public override bool IsApplied => Capabilities.Any(x => x.RequiredAppliedValue != RequirementOptions.Off);
        public override bool HasChanges => Capabilities.Any(x => x.RequiredAppliedValue != x.RequiredStagingValue);

        [UIValue("capabilities-list")]
        public Capability[] Capabilities { get; private set; }

        private bool _isLoading = false;
        [UIValue("is-loading")]
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value)
                    return;

                _isLoading = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsNotLoading));
            }
        }
        [UIValue("is-not-loading")]
        public bool IsNotLoading => !IsLoading;

        protected override string AssociatedBSMLFile => "HUIFilters.UI.Views.Filters.ModRequirementsFilterView.bsml";

        private bool _extraSongDataLoaded = false;

        private static readonly List<object> RequirementOptionsOptions = FilterUtilities.EnumerateEnumValuesAsObjectList<RequirementOptions>();
        private static readonly Dictionary<string, string> RequirementsMapping = new Dictionary<string, string>
        {
            { "Chroma Lighting Events", "Chroma" },
            { "Mapping Extensions-Precision Placement", "Mapping Extensions" },
            { "Mapping Extensions-Extra Note Angles", "Mapping Extensions" },
            { "Mapping Extensions-More Lanes", "Mapping Extensions" }
        };

        public void Initialize()
        {
            Capabilities = Collections.capabilities
                .Select(cap => RequirementsMapping.TryGetValue(cap, out var mapping) ? mapping : cap)
                .Distinct()
                .Select(x => new Capability(x))
                .ToArray();

            foreach (var capability in Capabilities)
            {
                capability.SettingChanged += delegate (Capability cap)
                {
                    if (this.IsStagingValuesFromUI)
                        InvokeSettingChanged();
                    else
                        cap.InvokeRequiredPropertyChanged();
                };
            }

            Loader.LoadingStartedEvent += OnSongCoreLoaderStartedLoading;
            Loader.SongsLoadedEvent += OnSongCoreLoaderFinishedLoading;
        }

        public void Dispose()
        {
            Loader.LoadingStartedEvent -= OnSongCoreLoaderStartedLoading;
            Loader.SongsLoadedEvent -= OnSongCoreLoaderFinishedLoading;
        }

        public override void ShowView(GameObject parentGO)
        {
            if (!_extraSongDataLoaded)
                LoadExtraSongData();

            base.ShowView(parentGO);
        }

        public override void HideView()
        {
            IsLoading = false;

            base.HideView();
        }

        protected override void InternalSetDefaultValuesToStaging()
        {
            foreach (var capability in Capabilities)
                capability.RequiredStagingValue = RequirementOptions.Off;
        }

        protected override void InternalSetAppliedValuesToStaging()
        {
            foreach (var capability in Capabilities)
                capability.RequiredStagingValue = capability.RequiredAppliedValue;
        }

        protected override void InternalSetSavedValuesToStaging(IReadOnlyDictionary<string, string> settings)
        {
            int errorCount = 0;

            foreach (var capability in Capabilities)
            {
                if (settings.TryGetValue(capability.Name, out var value) && Enum.TryParse(value, out RequirementOptions enumValue))
                    capability.RequiredStagingValue = enumValue;
                else
                    Plugin.Log.Debug($"Unable to load '{capability.Name}' value for Mod Requirements filter ({++errorCount})");
            }

            if (errorCount > 0)
                Plugin.Log.Warn("Some value(s) could not be loaded by the Mod Requirements filter");
        }

        public override void ApplyStagingValues()
        {
            foreach (var capability in Capabilities)
                capability.RequiredAppliedValue = capability.RequiredStagingValue;
        }

        public override void ApplyDefaultValues()
        {
            foreach (var capability in Capabilities)
                capability.RequiredAppliedValue = RequirementOptions.Off;
        }

        public override void FilterLevels(ref List<IPreviewBeatmapLevel> levels)
        {
            bool anyRequired = Capabilities.Any(x => x.RequiredAppliedValue == RequirementOptions.RequiredOrSuggested);
            for (int i = 0; i < levels.Count;)
            {
                // ost levels
                if (!(levels[i] is CustomPreviewBeatmapLevel customLevel))
                {
                    if (anyRequired)
                        levels.RemoveAt(i);
                    else
                        ++i;

                    continue;
                }

                var extraSongData = Collections.RetrieveExtraSongData(BeatmapUtilities.GetCustomLevelHash(customLevel));
                if (extraSongData == null)
                {
                    Plugin.Log.DebugOnly($"Unable to apply Mod Requirements filter to '{customLevel.songName}' by {customLevel.songAuthorName} (could not get ExtraSongData))");

                    ++i;
                    continue;
                }

                HashSet<string> requirements = extraSongData._difficulties
                    .SelectMany(x => x.additionalDifficultyData._requirements)
                    .Select(cap => RequirementsMapping.TryGetValue(cap, out var mapping) ? mapping : cap)
                    .ToHashSet();

                HashSet<string> suggestions = extraSongData._difficulties
                    .SelectMany(x => x.additionalDifficultyData._suggestions)
                    .Select(cap => RequirementsMapping.TryGetValue(cap, out var mapping) ? mapping : cap)
                    .ToHashSet();

                bool remove = false;
                foreach (var capability in Capabilities)
                {
                    bool required = requirements.Contains(capability.Name);
                    bool suggested = suggestions.Contains(capability.Name);
                    if ((capability.RequiredAppliedValue == RequirementOptions.RequiredOrSuggested && !(required || suggested)) ||
                        (capability.RequiredAppliedValue == RequirementOptions.NotRequired && required) ||
                        (capability.RequiredAppliedValue == RequirementOptions.NotRequiredOrSuggested && (required || suggested)))
                    {
                        remove = true;
                        break;
                    }
                }

                if (remove)
                    levels.RemoveAt(i);
                else
                    ++i;
            }
        }

        public override IDictionary<string, string> GetAppliedSettings()
        {
            return new Dictionary<string, string>(Capabilities.Select(x => new KeyValuePair<string, string>(x.Name, x.RequiredAppliedValue.ToString())));
        }

        private void OnSongCoreLoaderStartedLoading(Loader loader)
        {
            IsLoading = false;
            _extraSongDataLoaded = false;
        }

        private void OnSongCoreLoaderFinishedLoading(Loader loader, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> customLevels)
        {
            if (_viewGO?.activeInHierarchy ?? false)
                LoadExtraSongData();
        }

        private void LoadExtraSongData()
        {
            IsLoading = true;
            Plugin.Log.Debug("Starting ExtraSongData loading operation");

            UnityMainThreadTaskScheduler.Factory.StartNew(async delegate ()
            {
                const int PollTimeMilliseconds = 100;

                Stopwatch sw = Stopwatch.StartNew();
                ICollection<CustomPreviewBeatmapLevel> levelsToLoad = Loader.CustomLevels.Values;
                try
                {
                    ThreadPool.QueueUserWorkItem(LoadExtraSongDataThread, levelsToLoad);

                    while (!_extraSongDataLoaded)
                        await Task.Delay(PollTimeMilliseconds);
                }
                catch (NotSupportedException)
                {
                    Plugin.Log.Warn("Unable to load ExtraSongData using ThreadPool, falling back to async loading");

                    Task loadTask = LoadExtraSongDataAsync(levelsToLoad);
                    while (!loadTask.IsCompleted)
                        await Task.Delay(PollTimeMilliseconds);
                }

                Plugin.Log.Info($"ExtraSongData loading operation took {sw.ElapsedMilliseconds}ms");

                IsLoading = false;
                Collections.SaveExtraSongData();
            });
        }

        private void LoadExtraSongDataThread(object state)
        {
            var levelsToLoad = (ICollection<CustomPreviewBeatmapLevel>)state;
            int errorCount = 0;

            foreach (var level in levelsToLoad)
            {
                if (!_isLoading)
                {
                    // loading cancelled
                    return;
                }

                if (Collections.RetrieveExtraSongData(BeatmapUtilities.GetCustomLevelHash(level), level.customLevelPath) == null)
                    Plugin.Log.Debug($"Unable to load ExtraSongData for '{level.songName}' by {level.songAuthorName} ({++errorCount})");
            }

            if (errorCount > 0)
                Plugin.Log.Warn($"Unable to load ExtraSongData for {errorCount} beatmaps");

            _extraSongDataLoaded = true;
        }

        private async Task LoadExtraSongDataAsync(ICollection<CustomPreviewBeatmapLevel> levelsToLoad)
        {
            const int WorkTimeLimitMilliseconds = 5;
            int errorCount = 0;
            Stopwatch workLimitStopwatch = Stopwatch.StartNew();

            foreach (var level in levelsToLoad)
            {
                if (!_isLoading)
                {
                    // loading cancelled
                    return;
                }
                else if (workLimitStopwatch.ElapsedMilliseconds > WorkTimeLimitMilliseconds)
                {
                    await Task.Yield();
                    workLimitStopwatch.Restart();
                }

                if (Collections.RetrieveExtraSongData(level.levelID, level.customLevelPath) == null)
                    Plugin.Log.Debug($"Unable to load ExtraSongData for '{level.songName}' by {level.songAuthorName} ({++errorCount})");
            }

            workLimitStopwatch.Stop();

            if (errorCount > 0)
                Plugin.Log.Warn($"Unable to load ExtraSongData for {errorCount} beatmaps");

            _extraSongDataLoaded = true;
        }

        public class Capability : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public event Action<Capability> SettingChanged;

            private RequirementOptions _requiredStagingValue = RequirementOptions.Off;
            [UIValue("required-value")]
            public RequirementOptions RequiredStagingValue
            {
                get => _requiredStagingValue;
                internal set
                {
                    if (_requiredStagingValue == value)
                        return;

                    _requiredStagingValue = value;
                    this.CallAndHandleAction(SettingChanged, nameof(SettingChanged), this);
                }
            }

            public RequirementOptions RequiredAppliedValue { get; internal set; } = RequirementOptions.Off;

            public string Name { get; private set; }

            [UIValue("capability-text")]
            public string CapabilityText => $"'{Name}' Required";

            [UIValue("required-options")]
            private static List<object> RequiredOptions => RequirementOptionsOptions;

            public Capability(string capabilityName)
            {
                Name = capabilityName;
            }

            [UIAction("required-formatter")]
            private string RequiredFormatter(object obj)
            {
                var option = (RequirementOptions)obj;

                switch (option)
                {
                    case RequirementOptions.Off:
                        return "Off";

                    case RequirementOptions.RequiredOrSuggested:
                        return "Keep Required/Suggested";

                    case RequirementOptions.NotRequired:
                        return "Remove Required";

                    case RequirementOptions.NotRequiredOrSuggested:
                        return "Remove Required/Suggested";

                    default:
                        return "Error";
                }
            }

            internal void InvokeRequiredPropertyChanged() => this.CallAndHandleAction(PropertyChanged, nameof(RequiredStagingValue));
        }

        public enum RequirementOptions
        {
            Off,
            RequiredOrSuggested,
            NotRequired,
            NotRequiredOrSuggested
        }
    }
}
