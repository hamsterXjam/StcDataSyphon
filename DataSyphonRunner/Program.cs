using StcDataSyphon;
using System;
using System.Configuration;
using static StcDataSyphon.SyphonConfig;

namespace RunDataSyphonConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunDataSyphon();

            // todo: pass in a command line argument to skip this - no pause required when being run as a scheduled task
            Console.ReadKey();
        }

        #region production code

        private static void RunDataSyphon()
        {
            var configInit = GetConfigurationSettings();
            var controller = new DataSyphonController(configInit);
            controller.ExecuteTasks();
        }

        // builds a config initialiser object from the app.config values
        private static ConfigInitialiser GetConfigurationSettings()
        {
            return new ConfigInitialiser
            {
                LogFolder = GetStringFromAppConfig("LogFolder"),
                LogFileName = GetStringFromAppConfig("LogFileName"),
                RawTableCopySettings = GetStringFromAppConfig("RawTableCopySettings"),
                CommandTimeout = GetIntFromAppConfig("CommandTimeout"),
                NotifyAfterRows = GetIntFromAppConfig("NotifyAfterRows"),
                StgTableCopySettings = GetStringFromAppConfig("StgTableCopySettings"),
                PSqlConnectionString = GetConnectionStringFromAppConfig("PSqlConnection"),
                MySqlConnectionBase = GetConnectionStringFromAppConfig("MySqlConnection"),
                RawTablePrefix = GetStringFromAppConfig("RawTablePrefix"),
                StgTablePrefix = GetStringFromAppConfig("StgTablePrefix"),
                StagingDatabaseName = GetStringFromAppConfig("StagingDatabaseName"),
                CoreDatabaseName = GetStringFromAppConfig("CoreDatabaseName")
            };
        }


        #endregion


        #region helper methods

        private static int GetIntFromAppConfig(string settingName)
        {

            // todo: wrap in a try/catch
            int returnInt = int.Parse(ConfigurationManager.AppSettings.Get(settingName));

            return returnInt;
        }

        private static string GetStringFromAppConfig(string settingName)
        {

            // todo: wrap in a try/catch
            string returnString = ConfigurationManager.AppSettings.Get(settingName);

            return returnString;
        }


        private static string GetConnectionStringFromAppConfig(string connectionName)
        {

            // todo: wrap in a try/catch
            string connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;

            return connectionString;
        }

        #endregion

    }
}
