using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using HUIFilters.Installers;
using IPALogger = IPA.Logging.Logger;

namespace HUIFilters
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Config conf, Zenjector zenjector)
        {
            Instance = this;
            Plugin.Log = logger;

            PluginConfig.Instance = conf.Generated<PluginConfig>();

            zenjector.OnMenu<FilterInstaller>();
            zenjector.OnMenu<DataSourceInstaller>();
        }


        [OnEnable]
        public void OnEnable()
        {
        }

        [OnDisable]
        public void OnDisable()
        {
        }
    }
}
