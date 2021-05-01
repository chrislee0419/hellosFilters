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
            Container.BindInterfacesAndSelfTo<DurationFilter>().AsSingle();

            var externalFilters = InstallerUtilities.GetAutoInstallDerivativeTypesFromAllAssemblies(typeof(IFilter));
            foreach (var type in externalFilters)
                Container.BindInterfacesAndSelfTo(type).AsSingle();
        }
    }
}
