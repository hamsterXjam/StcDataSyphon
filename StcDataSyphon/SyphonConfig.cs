using System.Collections.Generic;

namespace StcDataSyphon
{
    public class SyphonConfig
    {
        // properties required for construction
        public string LogFolder { get; set; }
        public string LogFileName { get; set; }
        public int  CommandTimeout { get; set; }
        public int NotifyAfterRows { get; set; }
        public string PSqlConnectionString { get; set; }
        public string MySqlConnectionBase { get; set; }
        public string RawTablePrefix { get; set; }
        public string StgTablePrefix { get; set; }
        public string StagingDatabaseName { get; set; } // is this needed as property?
        public string CoreDatabaseName { get; set; } // is this needed as property?

        // properties created during constrction
        public string MySqlStagingConnectionString { get; set; }
        public string MySqlCoreConnectionString { get; set; }
        public List<string> RawTableList { get; set; }
        public List<string> StgTableList { get; set; }

        public bool configIsValid { get; set; }

        // member variables
        private string RawTableCopySettings;
        private string StgTableCopySettings;

        public SyphonConfig(SyphonConfig.ConfigInitialiser configInit)
        {
            // todo: test passing in Nulls for these
            this.LogFolder = configInit.LogFolder;
            this.LogFileName = configInit.LogFileName;
            this.CommandTimeout = configInit.CommandTimeout;
            this.NotifyAfterRows = configInit.NotifyAfterRows;
            this.PSqlConnectionString = configInit.PSqlConnectionString;
            this.MySqlConnectionBase = configInit.MySqlConnectionBase;
            this.RawTablePrefix = configInit.RawTablePrefix;
            this.StgTablePrefix = configInit.StgTablePrefix;
            this.StagingDatabaseName = configInit.StagingDatabaseName;
            this.CoreDatabaseName = configInit.CoreDatabaseName;
            this.RawTableCopySettings = configInit.RawTableCopySettings;
            this.StgTableCopySettings = configInit.StgTableCopySettings;

            this.configIsValid = ValidateConfig();
        }
        
        private bool ValidateConfig()
        {
            // use Assert to check these are vaguely valid?

            // Assuming all is good, create the additional properties
            SetProperties();

            return true;
        }

        private void SetProperties()
        {
            this.RawTableList = GetFilteredTableList(this.RawTableCopySettings);
            this.StgTableList = GetFilteredTableList(this.StgTableCopySettings);
            this.MySqlStagingConnectionString = string.Format(this.MySqlConnectionBase, this.StagingDatabaseName);
            this.MySqlCoreConnectionString = string.Format(this.MySqlConnectionBase, this.CoreDatabaseName);
        }

        private List<string> GetFilteredTableList(string tableSettings)
        {
            var tableSettingsConfig = tableSettings.Split(',');

            var tableList = new List<string>();
            foreach (var tableEntry in tableSettingsConfig)
            {
                // each table setting in the config file is formatted as [string tableName]|[bool process]
                var tableSetting = tableEntry.Split('|');
                if (bool.Parse(tableSetting[1]))
                {
                    tableList.Add(tableSetting[0]);
                }
            }
            return tableList;
        }


        // an object used as an initialiser for the SyphonConfig class
        public class ConfigInitialiser
        {
            public string LogFolder { get; set; }
            public string LogFileName { get; set; }
            public string RawTableCopySettings { get; set; }
            public string StgTableCopySettings { get; set; }
            public int CommandTimeout { get; set; }
            public int NotifyAfterRows { get; set; }
            public string PSqlConnectionString { get; set; }
            public string MySqlConnectionBase { get; set; }
            public string RawTablePrefix { get; set; }
            public string StgTablePrefix { get; set; }
            public string StagingDatabaseName { get; set; }
            public string CoreDatabaseName { get; set; }
        }
    }
}
