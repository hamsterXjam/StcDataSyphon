using MySqlConnector;
using Pervasive.Data.SqlClient;
using System;

namespace StcDataSyphon
{
    public class TakingThePsqlTask
    {
        private SyphonConfig config;
        private SyphonLogger logger;

        // todo: pass in the logger instance & config object to the constructor
        public TakingThePsqlTask(SyphonConfig syphonConfig, SyphonLogger syphonLogger)
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

            takeThePsql();
        }


        // this starts everything off
        private void takeThePsql()
        {
            // loose loop structure:
            // createPsqlConnection in using statement to do reader & stuff
            // createMySqlConnection in a nested using statement to do truncate table & bulk copy
            // close everything
            
            logger.addLogEntry(string.Format("Data extraction task: The following tables will be processed - {0}", string.Join(", ", config.RawTableList)));

            // do copy
            foreach (string table in config.RawTableList)
            {
                logger.addLogEntry($"Table: {table} - Begin the data copy process");
                using (PsqlConnection psqlConn = new PsqlConnection(config.PSqlConnectionString))
                {
                    psqlConn.Open();
                    // 1. get select statement
                    // 2. create command from select statement
                    // 3. create reader from command

                    // 1. get select statement
                    logger.addLogEntry($"Table: {table} - Getting the select statement");
                    var selectStatement = getPsqlSelectStatement(table);

                    // 2. create command from select statement
                    PsqlCommand psqlCommand = new PsqlCommand(selectStatement, psqlConn);

                    // 3. create reader from command
                    logger.addLogEntry($"Table: {table} - Getting the pSql data via a DataReader");
                    PsqlDataReader psqlReader = psqlCommand.ExecuteReader();


                    // send to MySql
                    using (MySqlConnection mySqlConn = new MySqlConnection(config.MySqlStagingConnectionString))
                    {
                        mySqlConn.Open();
                        logger.addLogEntry($"Table: {table} - Begin sending data to MySql");

                        // 1. Truncate the MySql destination table
                        // "Table: {table} - Removing all rows using TRUNCATE TABLE"
                        logger.addLogEntry($"Table: {table} - Removing all rows using TRUNCATE TABLE");
                        var deleteCommand = new MySqlCommand(string.Format(SqlResources.truncateTableSql, config.RawTablePrefix + table), mySqlConn);
                        deleteCommand.ExecuteNonQuery();

                        // 2. prepare the bulk copier
                        MySqlBulkCopy bulkCopier = new MySqlBulkCopy(mySqlConn);
                        bulkCopier.NotifyAfter = config.NotifyAfterRows;
                        bulkCopier.MySqlRowsCopied += new MySqlRowsCopiedEventHandler(onRowsCopied);

                        // 3. send the data via bulk copy
                        logger.addLogEntry($"Table: {table} - Inserting data into MySql database using bulk copy...");
                        bulkCopier.DestinationTableName = config.RawTablePrefix + table;
                        bulkCopier.WriteToServer(psqlReader);

                        // todo: is there a completion event or something on the bulk copier to hook into here?
                        logger.addLogEntry($"Table: {table} - Data copy complete - closing connections");
                    }

                    psqlReader.Close();
                }
            }

            // return some info?
            logger.addLogEntry("Data extraction task: All tables have now been processed");
        }

        // uses the table name to determine whether to call custom SQL or not
        // todo: does it need refactoring to make the config more robust?
        private string getPsqlSelectStatement(string tableName)
        {
            string selectStatement;
            switch (tableName)
            {
                case "Transactions":
                    selectStatement = SqlResources.transactionsQueryRestricted;
                    break;
                case "TransactionLines":
                    selectStatement = SqlResources.transactionLinesQueryRestricted;
                    break;
                default:
                    selectStatement = string.Format(SqlResources.selectAllRowsSql, tableName);
                    break;
            }

            return selectStatement;
        }

        // MySqlBulkCopy event handler for the notify after / rows copied event
        private void onRowsCopied(object sender, MySqlRowsCopiedEventArgs e)
        {
            var copyObject = (MySqlBulkCopy)sender;

            logger.addLogEntry(String.Format("Table: {0} - Rows inserted: {1}", copyObject.DestinationTableName, e.RowsCopied));
        }
    }
}
