using System;
using System.IO;
using System.Text;

namespace StcDataSyphon
{
    public class SyphonLogger
    {
        // these will be moved to a config settings object later
        private const string defaultLogFolder = "logs";
        private const string defaultLogFileName = "DataSyphonLog_{0}.txt";

        // the name of the log file in use
        public string logFile { get; private set; }

        // earier version without values provided by config - can be used as a fall-back
        public SyphonLogger()
        {
            setupLogFile(defaultLogFolder, defaultLogFileName);
        }

        public SyphonLogger(string configLogFolder, string configLogFileName)
        {
            setupLogFile(configLogFolder, configLogFileName);
        }

        private void setupLogFile(string logFolder, string logFileName)
        {
            var logHeadBuilder = new StringBuilder();
            logHeadBuilder.AppendLine("**************************************************");
            logHeadBuilder.AppendLine("********       STC Data Syphon Tool       ********");
            logHeadBuilder.AppendLine("**************************************************");

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Check if the logs folder exists in the current working directory
            if (!Directory.Exists(logFolder))
            {
                logHeadBuilder.AppendFormat("Couldn't find a folder called {0}, will create one.{1}", logFolder, Environment.NewLine);
                Directory.CreateDirectory(logFolder);
            }

            logFile = Path.Combine(logFolder, string.Format(logFileName, timestamp));
            logHeadBuilder.AppendFormat("Log file name and path will be {0}{1}", logFile, Environment.NewLine);
            addLogEntry(logHeadBuilder.ToString(), false);
            addLogEntry("Log file initialised");
        }

        // write line(s) to the log file - returns the content written as a string to the calling code
        // todo: find a way to pass the or inclide the calling class easily (without re-writing log4net)
        // class passes in 'this'/'this.nameProperty' as a variable?
        // create a separate logger in each class but persist the log file name?
        public string addLogEntry(string logEntry, bool addTimeStamp = true)
        {
            // todo: format the datestamp a bit better - use milliseconds?
            var lineToLog = addTimeStamp ? string.Format("{0}\t{1}", DateTime.Now.ToString("O"), logEntry) : logEntry;

            // hang on, does this *just work* ...?
            // Apparently yes. It probably just vanishes somewhere if there's no console, but maybe let's have an isFromConsole flag?
            Console.WriteLine(lineToLog);

            // todo: ok so we can call Console.WriteLine from here and output does return to a console app without any additional plumbing
            // if the calling code is from a winforms app, what would that do, or how could we send log output back?
            // a StreamWriter maybe? would that feed into a text box in the forms app maybe?

            // todo: not sure this is totally robust if the file path strings are incorrectly set
            // todo: use a FileInfo instead?
            using (var writer = File.AppendText(logFile))
            {
                writer.WriteLine(lineToLog);
            }

            return lineToLog;
        }
    }
}
