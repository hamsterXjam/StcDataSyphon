using MySqlConnector;
using System;
using System.Data;
using System.Collections.Generic;


namespace StcDataSyphon
{
    internal class ConvertDataTask
    {
        private SyphonConfig config;
        private SyphonLogger logger;

        private OleServer instanceOleServer;

        public ConvertDataTask(SyphonConfig syphonConfig, SyphonLogger syphonLogger)
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

            ConvertData();
        }

        private void ConvertData()
        {
            // loop through list of tables and process each one
            logger.addLogEntry($"Data conversion task: The following tables will be processed - {string.Join(", ", config.StgTableList)}");

            // The mysteries of COM and the Exchequer OLE Server:
            // creating a new OleServer object within each table iteration turner out to be a bad idea and caused random exceptions
            // race conditions? poor object disposal? either way, the solution seems to be to create one object at the class level and re-use it.
            this.instanceOleServer = new OleServer();

            foreach (var table in config.StgTableList)
            {
                var selectStatement = string.Empty;

                // this is crap, but will do for now...
                switch (table)
                {
                    case "customers":
                        selectStatement = SqlResources.selectStatementStgCustomers;
                        break;
                    case "stock":
                        selectStatement = SqlResources.selectStatementStgStock;
                        break;
                    case "stockinventorylevels":
                        selectStatement = SqlResources.selectStatementStgStockInventoryLevels;
                        break;
                    case "stockprices":
                        selectStatement = SqlResources.selectStatementStgStockPrices;
                        break;
                    case "transactionlines":
                        selectStatement = SqlResources.selectStatementStgTransactionLinesRestricted;
                        break;
                    case "transactions":
                        selectStatement = SqlResources.selectStatementStgTransactionsRestricted;
                        break;
                }
                CopyDataToTable(selectStatement, table);
            }

            logger.addLogEntry("Data conversion task: All tables have now been processed");
        }

        // query: affix the stg prefix to the table name at the start of the process seeing as it won't change?
        private void CopyDataToTable(string selectStatement, string destinationTable)
        {
            // 1. create a connection to use for the whole table operation
            // 2. truncate the destination table
            // 3. create a database command from a given select statement & create a DataReader
            // 4. get the schema table from the reader - DataTable schemaTable = reader.GetSchemaTable();
            // 5. create a data template from the schema table
            // 6. create a data table from the data template
            // 7. read from the reader and populate the data table
            // 8. Send the data table to the destination table via bulk update
            using (MySqlConnection mySqlStgConn = new MySqlConnection(config.MySqlStagingConnectionString))
            {
                mySqlStgConn.Open();

                logger.addLogEntry($"Table: {destinationTable} - Removing all rows using TRUNCATE TABLE");
                TruncateTable(mySqlStgConn, destinationTable);

                logger.addLogEntry($"Table: {destinationTable} - Create a DataReader based on the select statement");
                var reader = CreateDataReader(mySqlStgConn, selectStatement);

                logger.addLogEntry($"Table: {destinationTable} - Get the schema info from the reader");
                DataTable schemaTable = reader.GetSchemaTable();

                logger.addLogEntry($"Table: {destinationTable} - Get the data template for building the results table");
                var dataTemplate = CreateDataTableTemplate(schemaTable);

                logger.addLogEntry($"Table: {destinationTable} - Build a results DataTable from the template");
                var resultsTable = BuildDataTable(dataTemplate);

                logger.addLogEntry($"Table: {destinationTable} - Populate the results DataTable");
                PopulateResultsTable(resultsTable, dataTemplate, reader);

                // debug output test
                logger.addLogEntry($"Table: {destinationTable} - Data table has {resultsTable.Columns.Count} columns and {resultsTable.Rows.Count} rows");

                // close the reader to release the connection
                reader.Close();

                logger.addLogEntry($"Table: {destinationTable} - Copy the results DataTable to the destination table");
                var bulkCopier = new MySqlBulkCopy(mySqlStgConn);
                bulkCopier.DestinationTableName = config.StgTablePrefix + destinationTable;
                bulkCopier.WriteToServer(resultsTable);

                logger.addLogEntry($"Table: {destinationTable} - Data copy complete - closing connections");
            }


        }

        private void TruncateTable(MySqlConnection stagingConnection, string tableName)
        {
             var deleteCommand = new MySqlCommand(string.Format(SqlResources.truncateTableSql, config.StgTablePrefix + tableName), stagingConnection);
             deleteCommand.ExecuteNonQuery();
        }


        private MySqlDataReader CreateDataReader(MySqlConnection stagingConnection, string selectStatement)
        {
            // Create a data reader from the select statement
            var command = new MySqlCommand(selectStatement, stagingConnection);
            return command.ExecuteReader();
        }


        // uses the schema definition from a data reader to create a template for a new data table to hold the selected or converted values
        // the template is a list of type Tuple<string, string, bool>. Each tuple is the definition of a field in the destination data set
        // the tuple is made up of: the field name, the data type (effectively type.ToString()), and isSplitIntegerField
        private static List<Tuple<string, string, bool>> CreateDataTableTemplate(DataTable schemaTable)
        {
            // the return variable
            List<Tuple<string, string, bool>> fieldResults = new List<Tuple<string, string, bool>>();

            bool isCandidateField = false;
            bool isSplitIntegerField = false;
            string candidateFieldName = string.Empty;

            foreach (DataRow row in schemaTable.Rows)
            {
                // prepare variables for the column data
                string newColName = string.Empty;
                string newDataType = string.Empty;

                // get the name of the current field/column in the schema
                var colName = row["ColumnName"].ToString();

                // we are looking for fields that end in _1 or _2 as they will most likely be split integer fields
                // we can't use String.Split('_') efficiently because there are fields with more than one underscore in the name, e.g. "thVAT_Standard_1"
                // but String.EndsWith() seems to do a good job

                if (colName.EndsWith("_1"))
                {
                    if (row["DataType"].ToString() == "System.Int16")
                    {
                        // this is the first part of a split integer field - store the details in a variable for the next iteration of the loop
                        isCandidateField = true;
                        candidateFieldName = colName.Substring(0, colName.Length - 2);
                    }
                    else
                    {
                        // unlikely, but just in case - store this as a field definition
                        newColName = colName;
                        newDataType = row["DataType"].ToString();
                        isSplitIntegerField = false;
                    }
                }
                else if (colName.EndsWith("_2"))
                {
                    if (row["DataType"].ToString() == "System.Int32" & isCandidateField & candidateFieldName == colName.Substring(0, colName.Length - 2))
                    {
                        // checking against the values from the previous pass, we have a match
                        newColName = candidateFieldName;
                        newDataType = "System.Double";
                        isSplitIntegerField = true;
                    }
                    else
                    {
                        // unlikely, but just in case - store this as a field definition
                        newColName = colName;
                        newDataType = row["DataType"].ToString();
                        isSplitIntegerField = false;
                    }

                    // reset variables
                    isCandidateField = false;
                    candidateFieldName = string.Empty;
                }
                else
                {
                    newColName = colName;
                    newDataType = row["DataType"].ToString();
                    isSplitIntegerField = false;
                }

                // add whatever has been found into the template object, but only if we are not waiting for a second pass
                if (!isCandidateField)
                {
                    fieldResults.Add(Tuple.Create(newColName, newDataType, isSplitIntegerField));
                }
            }

            return fieldResults;
        }


        // takes the data template and creates a data table to contain the converted results
        private DataTable BuildDataTable(List<Tuple<string, string, bool>> dataTableTemplate)
        {
            // todo: check - does the data table need a name? give it a name based on the table being processed?
            var templatedTable = new DataTable();

            // we don't need to know about the split integer fields at this point, we're just building the data table for the converted data
            foreach (var rowDefinition in dataTableTemplate)
            {
                templatedTable.Columns.Add(rowDefinition.Item1, Type.GetType(rowDefinition.Item2));
            }

            return templatedTable;
        }


        private void PopulateResultsTable(DataTable resultsTable, List<Tuple<string, string, bool>> dataTemplate, MySqlDataReader reader)
        {
            while (reader.Read())
            {
                var row = resultsTable.NewRow();
                foreach (var rowDefinition in dataTemplate)
                {
                    // check if we have a standard field or a split integer that needs converting
                    if (!rowDefinition.Item3)
                    {
                        // ok so I really wanted to do something clever here and call a reader.Get... method based on the known type from the defintion
                        // but you can't pass a type as a variable as T into GetFieldValue<T>
                        // and creating a massive switch statement to call GetString or GetIn32 based on the known type rather defeats the object
                        // I was going to try casting using Convert.ChangeType(readerValue, Type.GetType(rowDefinition.Item2))
                        // but hey, guess what - thanks to the wonders of .net and a robustly stuctured DataTable we can just use reader.GetValue() and implicit casting takes care of the rest!

                        // it's just as shame reader.GetValue() only works with the ordinal and not with the column name...
                        var ordinal = reader.GetOrdinal(rowDefinition.Item1);
                        row[rowDefinition.Item1] = reader.GetValue(ordinal);
                    }
                    else
                    {
                        // we have a split integer - call the COM function to convert it back to a Double
                        row[rowDefinition.Item1] = instanceOleServer.getDouble(reader.GetInt16(rowDefinition.Item1 + "_1"), reader.GetInt32(rowDefinition.Item1 + "_2"));
                    }
                }
                resultsTable.Rows.Add(row);
            }
        }
    }
}
