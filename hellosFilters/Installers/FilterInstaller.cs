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

            Container.Bind<IFilter>().To<DifficultyFilter>().AsSingle();
            Container.Bind<IFilter>().To<DurationFilter>().AsSingle();

            var externalFilters = InstallerUtilities.GetAutoInstallDerivativeTypesFromAllAssemblies(typeof(IFilter));
            foreach (var type in externalFilters)
                Container.BindInterfacesAndSelfTo(type).AsSingle();
        }
    }
}
