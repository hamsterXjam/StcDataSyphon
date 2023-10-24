using System;
using static StcDataSyphon.SyphonConfig;

namespace StcDataSyphon
{
    public class DataSyphonController
    {
        private SyphonConfig config;
        public SyphonLogger logger { get; set; }

        // earlier version without a config initialised - deprecated
        public DataSyphonController()
        {
            // Maybe not passing in the config to the controller? Instead check it when starting the tasks?

            // todo: the controller should create the logger and pass it back to the calling class
            // or at least expose it as an object
            this.logger = new SyphonLogger();
            logger.addLogEntry("I am the Controller. I have created a logger.");
        }

        public DataSyphonController(ConfigInitialiser configInit)
        {
            // initialise config
            this.config = new SyphonConfig(configInit);
            if (!config.configIsValid)
            {
                Console.WriteLine("Invalid config supplied! Exiting task execution.");
                return;
            }

            //initialise logger using config
            this.logger = new SyphonLogger(config.LogFolder, config.LogFileName);
            logger.addLogEntry("DataSyphonController has initialised the configuration and created a logger");

        }

        // version that uses the config created in the constructor
        public void ExecuteTasks()
        {
            if (!config.configIsValid)
            {
                logger.addLogEntry("Invalid config supplied! Exiting task execution.");
                return;
            }

            // log and output some key config values
            logger.addLogEntry("DataSyphonController: Entering ExecuteTasks()");
            logger.addLogEntry($"Config status: {(config.configIsValid? "Valid" : "Invalid")}");
            logger.addLogEntry($"Config stage1 table list includes: {string.Join(", ", config.RawTableList)}");
            logger.addLogEntry($"Config stage2 table list includes: {string.Join(", ", config.StgTableList)}");

            logger.addLogEntry("DataSyphonController: Starting the task list");
            ControllerExecuteTasks();
            logger.addLogEntry("DataSyphonController: Task execution complete - all done!");
        }


        // earier version where the config is passed in
        public void ExecuteTasks(SyphonConfig syphonConfig)
        {
            if (!syphonConfig.configIsValid)
            {
                logger.addLogEntry("Invalid config supplied! Exiting task execution.");
                return;
            }
            this.config = syphonConfig;
            logger.addLogEntry("About to start on the task list");
            ControllerExecuteTasks();
            logger.addLogEntry("****** Ok, I think we're done here ******");
        }

        private void ControllerExecuteTasks()
        {
            // todo: find a way to pass the task list in as config or in some structured format
            // will do it manually for now

            // I. Copy raw data from PSql database into MySql database
            var dataExtractionTask = new TakingThePsqlTask(config, logger);
            logger.addLogEntry("DataSyphonController: Begin Data Extraction Task");
            dataExtractionTask.ExecuteTask();

            // II. Run data conversions
            var dataConversionTask = new ConvertDataTask(config, logger);
            logger.addLogEntry("DataSyphonController: Begin Data Conversion Task");
            dataConversionTask.ExecuteTask();

            // III. Copy converted data to core database
            var dataCopyTask = new CopyDataTask(config, logger);
            logger.addLogEntry("DataSyphonController: Begin Data Copy Task");
            dataCopyTask.ExecuteTask();
        }
    }
}
