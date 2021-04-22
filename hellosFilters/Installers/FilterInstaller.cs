using Zenject;
using HUIFilters.Filters;

namespace HUIFilters.Installers
{
    public class FilterInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<FilterManager>().AsSingle();
        }
    }
}
