using MySqlConnector;

namespace StcDataSyphon
{
    internal class CopyDataTask
    {
        private SyphonConfig config;
        private SyphonLogger logger;

        public CopyDataTask(SyphonConfig syphonConfig, SyphonLogger syphonLogger)
        {
            this.config = syphonConfig;
            this.logger = syphonLogger;
        }

        // public entry point - use this method name for all tasks
        public void ExecuteTask()
        {
            // check config flag first
            if (!config.configIsValid)
            {
                // log something first, then...
                logger.addLogEntry("Invalid config supplied! Exiting task execution.");
                return;
            }

            CopyData();
        }

        private void CopyData()
        {
            // the data copy table list is the same as the data conversion list
            logger.addLogEntry($"Data copy task: The following tables will be processed - {string.Join(", ", config.StgTableList)}");

            foreach (var table in config.StgTableList)
            {
                using (MySqlConnection mySqlCoreConn = new MySqlConnection(config.MySqlCoreConnectionString))
                {
                    logger.addLogEntry($"Table: {table} - Open Connection");
                    mySqlCoreConn.Open();
                    logger.addLogEntry($"Table: {table} - Get stored procedure call and create command");
                    var copyCommand = new MySqlCommand(GetStoredProcCall(table), mySqlCoreConn);
                    copyCommand.CommandTimeout = config.CommandTimeout;
                    logger.addLogEntry($"Table: {table} - Execute data copy command");
                    copyCommand.ExecuteNonQuery();
                    logger.addLogEntry($"Table: {table} - Command has executed");
                }
            }
        }

        private string GetStoredProcCall(string table)
        {
            string procCall = string.Empty;
            switch (table)
            {
                case "customers":
                    procCall = SqlResources.dataCopyProcCustomers;
                    break;
                case "stock":
                    procCall = SqlResources.dataCopyProcStock;
                    break;
                case "stockinventorylevels":
                    procCall = SqlResources.dataCopyProcStockInventoryLevels;
                    break;
                case "stockprices":
                    procCall = SqlResources.dataCopyProcStockPrices;
                    break;
                case "transactionlines":
                    procCall = SqlResources.dataCopyProcTransactionLines;
                    break;
                case "transactions":
                    procCall = SqlResources.dataCopyProcTransactions;
                    break;
            }
            return procCall;
        }
    }
}
