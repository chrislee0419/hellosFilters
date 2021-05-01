using System;
using Zenject;
using IPA.Loader;
using HUIFilters.DataSources;

namespace HUIFilters.Installers
{
    public class DataSourceInstaller : Installer
    {
        private const string SongDataCoreID = "SongDataCore";

        public override void InstallBindings()
        {
            if (PluginManager.GetPluginFromId(SongDataCoreID) != null)
                InstallSongDataCoreDataSource();

            InstallEmptyDataSource();
        }

        private void InstallSongDataCoreDataSource()
        {
            SongDataCoreDataSource sdcDataSource = null;

            if (!Container.HasBinding<IDataSource>())
            {
                Plugin.Log.Info("Installing SongDataCoreDataSource to IDataSource");
                sdcDataSource = CreateSongDataCoreDataSourceInstance();

                Container.Bind<IDataSource>().To<SongDataCoreDataSource>().FromInstance(sdcDataSource);
            }

            if (!Container.HasBinding<IScoreSaberDataSource>())
            {
                Plugin.Log.Info("Installing SongDataCoreDataSource to IScoreSaberDataSource");
                if (sdcDataSource == null)
                    sdcDataSource = CreateSongDataCoreDataSourceInstance();

                Container.Bind<IScoreSaberDataSource>().To<SongDataCoreDataSource>().FromInstance(sdcDataSource);
            }

            if (!Container.HasBinding<IBeatSaverDataSource>())
            {
                Plugin.Log.Info("Installing SongDataCoreDataSource to IBeatSaverDataSource");
                if (sdcDataSource == null)
                    sdcDataSource = CreateSongDataCoreDataSourceInstance();

                Container.Bind<IBeatSaverDataSource>().To<SongDataCoreDataSource>().FromInstance(sdcDataSource);
            }
        }

        private SongDataCoreDataSource CreateSongDataCoreDataSourceInstance()
        {
            var sdcDataSource = new SongDataCoreDataSource();

            Container.Bind<IInitializable>().To<SongDataCoreDataSource>().FromInstance(sdcDataSource);
            Container.Bind<IDisposable>().To<SongDataCoreDataSource>().FromInstance(sdcDataSource);

            return sdcDataSource;
        }

        private void InstallEmptyDataSource()
        {
            var emptyDataSource = new EmptyDataSource();

            if (!Container.HasBinding<IDataSource>())
            {
                Plugin.Log.Info("Installing EmptyDataSource to IDataSource");
                Container.Bind<IDataSource>().To<EmptyDataSource>().FromInstance(emptyDataSource);
            }

            if (!Container.HasBinding<IScoreSaberDataSource>())
            {
                Plugin.Log.Info("Installing EmptyDataSource to IScoreSaberDataSource");
                Container.Bind<IScoreSaberDataSource>().To<EmptyDataSource>().FromInstance(emptyDataSource);
            }

            if (!Container.HasBinding<IBeatSaverDataSource>())
            {
                Plugin.Log.Info("Installing EmptyDataSource to IBeatSaverDataSource");
                Container.Bind<IBeatSaverDataSource>().To<EmptyDataSource>().FromInstance(emptyDataSource);
            }
        }
    }
}
