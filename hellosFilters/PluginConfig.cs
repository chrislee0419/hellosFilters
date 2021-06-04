using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using HUIFilters.Converters;
using HUIFilters.Filters;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace HUIFilters
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        [NonNullable]
        [UseConverter(typeof(ExceptionRecoverableListConverter<SavedFilterSettings, SavedFilterSettingsConverter>))]
        public virtual List<SavedFilterSettings> SavedFilterSettings { get; set; } = new List<SavedFilterSettings>();

        ///// <summary>
        ///// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        ///// </summary>
        //public virtual void OnReload()
        //{
        //    // Do stuff after config is read from disk.
        //}

        ///// <summary>
        ///// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        ///// </summary>
        //public virtual void Changed()
        //{
        //    // Do stuff when the config is changed.
        //}

        ///// <summary>
        ///// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        ///// </summary>
        //public virtual void CopyFrom(PluginConfig other)
        //{
        //    // This instance's members populated from other
        //}
    }
}