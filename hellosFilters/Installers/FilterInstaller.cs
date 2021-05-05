using Zenject;
using HUI.Utilities;
using HUIFilters.Filters;
using HUIFilters.Filters.BuiltIn;

namespace HUIFilters.Installers
{
    public class FilterInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<FilterManager>().AsSingle();

            Container.BindInterfacesAndSelfTo<DifficultyFilter>().AsSingle();
            Container.BindInterfacesAndSelfTo<CharacteristicFilter>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteDensityFilter>().AsSingle();
            Container.BindInterfacesAndSelfTo<DurationFilter>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerStatsFilter>().AsSingle();
            Container.BindInterfacesAndSelfTo<ModRequirementsFilter>().AsSingle();

            var externalFilters = InstallerUtilities.GetAutoInstallDerivativeTypesFromAllAssemblies(typeof(IFilter));
            foreach (var type in externalFilters)
                Container.BindInterfacesAndSelfTo(type).AsSingle();
        }
    }
}
